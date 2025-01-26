use std::ops::Range;

#[derive(Clone, Copy, Debug)]
pub enum Suit {
    Hearts,
    Diamonds,
    Spades,
    Clubs,
}

impl Suit {
    pub const VALUES: [Suit; 4] = [Suit::Hearts, Suit::Diamonds, Suit::Spades, Suit::Clubs];
}

#[derive(Clone, Copy, Debug)]
pub struct Card(pub Suit, pub u8); // ace = 0, king = 12

impl Card {
    pub const RANK_RANGE: Range<u8> = 0..12;

    pub fn new(suit: Suit, rank: u8) -> Card {
        debug_assert!(rank >= 0 && rank <= 12);
        Card(suit, rank)
    }

    pub fn serialize(&self) -> u8 {
        self.0 as u8 | (self.1 << 2)
    }
}
