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

#[derive(Clone, Debug)]
pub enum ClientboundPacket {
    LobbyResponse { code: String },
    BeginGame,
    Setup { hand: Vec<Card> },
    FlipCenter { a: Card, b: Card },
}

impl ClientboundPacket {
    pub fn id(&self) -> u8 {
        use ClientboundPacket::*;
        match self {
            LobbyResponse { .. } => 0,
            BeginGame => 1,
            Setup { .. } => 2,
            FlipCenter { .. } => 3,
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
        }

        result
    }
}
