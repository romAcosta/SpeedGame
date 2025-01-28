use rand::seq::SliceRandom;
use std::sync::Arc;
use std::time::Duration;
use tokio::sync::{Mutex, RwLock};
use tracing::{debug, info};

use crate::card::{Card, Suit};
use crate::connection::Connection;
use crate::packets::{ClientboundPacket, InactivityLevel, ServerboundPacket};

const HAND_SIZE: usize = 5;
const FORCE_PLAY_TIMEOUT: Duration = Duration::from_secs(10);

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
            hand.extend(self.pop_n_cards(HAND_SIZE).await.unwrap());
            debug!(cards = ?hand, "Dealt hand to player");

            player
                .send(ClientboundPacket::Setup {
                    hand: hand.to_vec(),
                })
                .await;
        }

        self.flip_center().await;

        loop {
            while self.no_more_moves().await {
                self.flip_center().await;
            }

            if !self.tick().await {
                break;
            }
        }
    }

    async fn tick(self: &Arc<Game>) -> bool {
        match tokio::time::timeout(FORCE_PLAY_TIMEOUT, self.read_next_message()).await {
            Ok(continue_game) => continue_game,
            Err(_) => {
                self.players[0]
                    .send(ClientboundPacket::Inactivity {
                        level: InactivityLevel::ForcePlay,
                    })
                    .await;
                true
            }
        }
    }

    async fn read_next_message(self: &Arc<Self>) -> bool {
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
        let mut deck = self.deck.write().await;

        debug!(player = player_num, ?packet, "Handling player packet");

        match packet {
            ServerboundPacket::PlayCard { card, action_id } => {
                let card_index = match hand.iter().position(|c| *c == card) {
                    Some(i) => i,
                    None => return false,
                };

                let deck_id = action_id & 1;
                let play_deck = &play_area[deck_id as usize];

                let card = hand[card_index];
                if !card.stackable_on(play_deck.last().unwrap()) {
                    debug!(
                        player = player_num,
                        ?card,
                        action_id,
                        "Rejecting invalid card play"
                    );
                    player
                        .send(ClientboundPacket::RejectCard { action_id })
                        .await;
                    return true;
                }

                hand.remove(card_index);
                play_area[deck_id as usize].push(card);

                debug!(
                    player = player_num,
                    ?card,
                    deck = deck_id,
                    "Player played card successfully"
                );

                if let Some(drawn_card) = deck.pop() {
                    other_player
                        .send(ClientboundPacket::PlayCard { card, action_id })
                        .await;

                    player
                        .send(ClientboundPacket::DrawCard { card: drawn_card })
                        .await;

                    hand.push(drawn_card);
                } else {
                    other_player.send(ClientboundPacket::RemoveCard).await;
                }
            }
            _ => return false,
        }

        true
    }

    pub async fn no_more_moves(self: &Arc<Self>) -> bool {
        let play_area = self.play_area.lock().await;

        for player in &self.players {
            for card in player.hand.lock().await.iter() {
                if card.stackable_on(play_area[0].last().unwrap()) {
                    return false;
                }

                if card.stackable_on(play_area[1].last().unwrap()) {
                    return false;
                }
            }
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

    pub async fn pop_n_cards(self: &Arc<Self>, n: usize) -> Option<Vec<Card>> {
        let mut deck = self.deck.write().await;
        let len = deck.len();

        if len < n {
            return None;
        }

        Some(deck.split_off(len - n))
    }

    pub async fn flip_center(self: &Arc<Self>) {
        let new_cards = match self.pop_n_cards(2).await {
            Some(cards) => cards,
            None => return,
        };

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
