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

pub trait ClientboundPacket {
    const ID: u8;
    fn serialize(&self) -> Vec<u8>;
}

pub struct LobbyResponse {
    pub code: String,
}

impl ClientboundPacket for LobbyResponse {
    const ID: u8 = 1;

    fn serialize(&self) -> Vec<u8> {
        return self.code.bytes().collect();
    }
}
