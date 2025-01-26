use rand::seq::SliceRandom;
use std::sync::Arc;
use tokio::sync::RwLock;
use tracing::{debug, info};

use crate::card::{Card, Suit};
use crate::connection::Connection;
use crate::packets::ClientboundPacket;

const HAND_SIZE: usize = 5;

pub struct Game {
    pub players: Vec<Connection>,
    pub deck: RwLock<Vec<Card>>,
}

impl Game {
    pub fn new(player_a: Connection, player_b: Connection) -> Arc<Game> {
        let deck = Self::shuffled_deck();

        Arc::new(Game {
            players: vec![player_a, player_b],
            deck: RwLock::new(deck),
        })
    }

    pub async fn start(self: &Arc<Game>) {
        info!("Starting new game with {} players", self.players.len());

        debug!("Sending BeginGame packets");
        for player in &self.players {
            player.send(ClientboundPacket::BeginGame).await;
        }

        debug!("Dealing initial hands");
        for player in &self.players {
            let hand = self.pop_n_cards(HAND_SIZE).await;
            debug!(cards = ?hand, "Dealt hand to player");
            player.send(ClientboundPacket::Setup { hand }).await;
        }

        let center = ClientboundPacket::FlipCenter {
            a: self.pop_card().await,
            b: self.pop_card().await,
        };
        for player in &self.players {
            player.send(center.clone()).await;
        }
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

    pub async fn pop_card(self: &Arc<Self>) -> Card {
        let mut deck = self.deck.write().await;
        deck.pop().expect("deck is empty")
    }

    pub async fn pop_n_cards(self: &Arc<Self>, n: usize) -> Vec<Card> {
        let mut deck = self.deck.write().await;
        let len = deck.len();
        deck.split_off(len - n)
    }
}
