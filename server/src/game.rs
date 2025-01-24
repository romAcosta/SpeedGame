use rand::seq::SliceRandom;
use std::sync::Arc;

use crate::card::{Card, Suit};
use crate::connection::Connection;
use crate::packets::ClientboundPacket;

const HAND_SIZE: usize = 5;

pub struct Game {
    pub players: Vec<Connection>,
}

impl Game {
    pub fn new(player_a: Connection, player_b: Connection) -> Arc<Game> {
        Arc::new(Game {
            players: vec![player_a, player_b],
        })
    }

    pub async fn start(self: &Arc<Game>) {
        for player in &self.players {
            player.send(ClientboundPacket::BeginGame);
        }

        let mut deck = Self::shuffled_deck();

        for player in &self.players {
            let hand = deck.split_off(deck.len() - HAND_SIZE);
            player.send(ClientboundPacket::Setup { hand });
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
}
