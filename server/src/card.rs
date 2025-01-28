use std::ops::Range;

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum Suit {
    Hearts,
    Diamonds,
    Spades,
    Clubs,
}

impl Suit {
    pub const VALUES: [Suit; 4] = [Suit::Hearts, Suit::Diamonds, Suit::Spades, Suit::Clubs];
}

#[derive(Clone, Copy, Debug, PartialEq)]
pub struct Card(pub Suit, pub u8); // ace = 0, king = 12

impl Card {
    pub const RANK_RANGE: Range<u8> = 0..12;

    pub fn serialize(&self) -> u8 {
        self.0 as u8 | (self.1 << 2)
    }

    pub fn deserialize(data: u8) -> Option<Self> {
        let suit = Suit::VALUES.get(data as usize & 0b11)?;
        let rank = (data >> 2) as u8;
        Some(Card(*suit, rank))
    }

    pub fn stackable_on(&self, other: &Card) -> bool {
        let diff = (self.1 as i32 - other.1 as i32).abs();
        return diff == 1 || diff == 12;
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_card_stackable_on() {
        for rank in 0..=12 {
            assert!(Card::stackable_on(
                &Card(Suit::Spades, rank),
                &Card(Suit::Spades, (rank + 1) % 13)
            ));
        }
    }
}
