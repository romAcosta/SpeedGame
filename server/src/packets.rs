use std::io::{self, ErrorKind};

use crate::card::Card;

#[derive(Debug)]
pub enum ServerboundPacket {
    JoinQueue,
    RequestLobby,
}

impl ServerboundPacket {
    pub fn deserialize(data: Vec<u8>) -> Result<Self, io::Error> {
        let id = match data.get(0) {
            Some(i) => i,
            None => return Err(io::Error::new(ErrorKind::InvalidData, "packet is empty")),
        };

        use ServerboundPacket::*;
        Ok(match id {
            0 => JoinQueue,
            1 => RequestLobby,
            _ => return Err(io::Error::new(ErrorKind::InvalidData, "packet is invalid")),
        })
    }
}

#[derive(Clone, Copy, Debug)]
#[repr(u8)]
pub enum InactivityLevel {
    Highlight,
    ForcePlay,
}

#[derive(Clone, Debug)]
pub enum ClientboundPacket {
    LobbyResponse { code: String },
    BeginGame,
    Setup { hand: Vec<Card> },
    FlipCenter { a: Card, b: Card },
    PlayCard { card: Card, action_id: u8 },
    RejectCard { action_id: u8 },
    Inactivity { level: InactivityLevel },
    DrawCard,
}

impl ClientboundPacket {
    pub fn id(&self) -> u8 {
        use ClientboundPacket::*;
        match self {
            LobbyResponse { .. } => 0,
            BeginGame => 1,
            Setup { .. } => 2,
            FlipCenter { .. } => 3,
            PlayCard { .. } => 4,
            RejectCard { .. } => 5,
            Inactivity { .. } => 6,
            DrawCard => 7,
        }
    }

    pub fn serialize(&self) -> Vec<u8> {
        let mut result = vec![self.id()];

        use ClientboundPacket::*;
        match self {
            LobbyResponse { code } => result.extend(code.bytes()),
            BeginGame => {}
            Setup { hand } => result.extend(hand.iter().map(|card| card.serialize())),
            FlipCenter { a, b } => result.extend_from_slice(&[a.serialize(), b.serialize()]),
            PlayCard { card, action_id } => {
                result.extend_from_slice(&[card.serialize(), *action_id])
            }
            RejectCard { action_id } => result.push(*action_id),
            Inactivity { level } => result.push(*level as u8),
            DrawCard => {}
        }

        result
    }
}
