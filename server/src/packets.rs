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

        use ServerboundPacket::*;
        Ok(match id {
            0 => JoinQueue,
            1 => RequestLobby,
            _ => return Err(io::Error::new(ErrorKind::InvalidData, "packet is invalid")),
        })
    }
}

pub enum ClientboundPacket {
    LobbyResponse { code: String },
}

impl ClientboundPacket {
    pub fn serialize(&self) -> Vec<u8> {
        use ClientboundPacket::*;
        match self {
            LobbyResponse { code } => code.bytes().collect(),
        }
    }
}
