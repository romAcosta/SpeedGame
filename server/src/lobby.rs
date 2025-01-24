use crate::connection::Connection;

pub struct Lobby {}

impl Lobby {
    pub fn new() -> Self {
        Lobby {}
    }

    pub fn add_player(&mut self, connection: Connection) {}
}
