use std::io::{self, ErrorKind};

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

        Ok(match id {
            0 => ServerboundPacket::JoinQueue,
            1 => ServerboundPacket::RequestLobby,
            _ => return Err(io::Error::new(ErrorKind::InvalidData, "packet is invalid")),
        })
    }
}

pub enum ClientboundPacket {
    LobbyResponse { code: String },
}

impl ClientboundPacket {
    pub fn serialize(&self) -> Vec<u8> {
        match self {
            ClientboundPacket::LobbyResponse { code } => code.bytes().collect(),
        }
    }
}
