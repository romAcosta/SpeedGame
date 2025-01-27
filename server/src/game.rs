use rand::seq::SliceRandom;
use std::sync::Arc;
use tokio::sync::{Mutex, RwLock};
use tracing::{debug, info};

use crate::card::{Card, Suit};
use crate::connection::Connection;
use crate::packets::{ClientboundPacket, ServerboundPacket};

const HAND_SIZE: usize = 5;

pub struct Player {
    connection: Connection,
    hand: Mutex<Vec<Card>>,
}

impl Player {
    pub fn new(connection: Connection) -> Self {
        Player {
            connection,
            hand: Mutex::new(Vec::new()),
        }
    }

    pub async fn send(&self, packet: ClientboundPacket) {
        self.connection.send(packet).await;
    }

    pub async fn next(&self) -> Option<ServerboundPacket> {
        self.connection.next().await
    }
}

pub struct Game {
    pub players: [Player; 2],
    pub deck: RwLock<Vec<Card>>,
    play_area: Mutex<[Vec<Card>; 2]>,
}

impl Game {
    pub fn new(player_a: Connection, player_b: Connection) -> Arc<Game> {
        let deck = Self::shuffled_deck();

        Arc::new(Game {
            players: [Player::new(player_a), Player::new(player_b)],
            deck: RwLock::new(deck),
            play_area: Mutex::new([const { Vec::new() }; 2]),
        })
    }

    pub async fn start(self: &Arc<Game>) {
        info!("Game starting");

        for player in &self.players {
            player.send(ClientboundPacket::BeginGame).await;

            let mut hand = player.hand.lock().await;
            hand.extend(self.pop_n_cards(HAND_SIZE).await);
            debug!(cards = ?hand, "Dealt hand to player");

            player
                .send(ClientboundPacket::Setup {
                    hand: hand.to_vec(),
                })
                .await;
        }

        self.flip_center().await;

        while self.tick().await {}
    }

    async fn tick(self: &Arc<Game>) -> bool {
        tokio::select! {
            Some(a_packet) = self.players[0].next() => self.handle_packet(a_packet, 0).await,
            Some(b_packet) = self.players[1].next() => self.handle_packet(b_packet, 1).await,
            else => return false
        }
    }

    async fn handle_packet(self: &Arc<Game>, packet: ServerboundPacket, player_num: usize) -> bool {
        let player = &self.players[player_num];
        let other_player = &self.players[(player_num + 1) % 2];
        let mut hand = player.hand.lock().await;
        let mut play_area = self.play_area.lock().await;

        match packet {
            ServerboundPacket::PlayCard { card, action_id } => {
                let card_index = match hand.iter().position(|c| *c == card) {
                    Some(i) => i,
                    None => return false,
                };

                let deck_id = action_id & 1;
                let deck = &play_area[deck_id as usize];

                let card = hand[card_index];
                if !card.stackable_on(*deck.last().unwrap()) {
                    player
                        .send(ClientboundPacket::RejectCard { action_id })
                        .await;
                    return true;
                }

                hand.remove(card_index);
                play_area[deck_id as usize].push(card);

                other_player
                    .send(ClientboundPacket::PlayCard { card, action_id })
                    .await;
            }
            _ => {}
        }

        true
    }

    pub fn shuffled_deck() -> Vec<Card> {
        let mut cards = Vec::new();
        for suit in Suit::VALUES {
            for rank in Card::RANK_RANGE {
                cards.push(Card(suit, rank));
            }
        }

        let mut rand = rand::thread_rng();
        cards.shuffle(&mut rand);

        cards
    }

    pub async fn pop_n_cards(self: &Arc<Self>, n: usize) -> Vec<Card> {
        let mut deck = self.deck.write().await;
        let len = deck.len();
        deck.split_off(len - n)
    }

    pub async fn flip_center(self: &Arc<Self>) {
        let new_cards = self.pop_n_cards(2).await;

        let mut play_area = self.play_area.lock().await;
        play_area[0].push(new_cards[0]);
        play_area[1].push(new_cards[1]);

        let packet = ClientboundPacket::FlipCenter {
            a: new_cards[0],
            b: new_cards[1],
        };
        for player in &self.players {
            player.send(packet.clone()).await;
        }
    }
}
