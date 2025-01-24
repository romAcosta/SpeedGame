mod card;
mod connection;
mod lobby;
mod packets;
mod protocol;

use std::io;
use std::net::SocketAddr;
use std::sync::Arc;
use std::time::Duration;

use connection::Connection;
use dashmap::DashMap;
use lobby::Lobby;
use packets::{ClientboundPacket, ServerboundPacket};
use rand::Rng;
use tokio::net::{TcpListener, TcpStream};
use tokio::sync::RwLock;

type WsError = tokio_tungstenite::tungstenite::Error;

const HANDSHAKE_TIMEOUT: Duration = Duration::from_secs(3);
const CODE_CHARS: &[u8] =
    "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".as_bytes();

pub struct Server {
    listener: RwLock<TcpListener>,
    open_lobbies: DashMap<String, Connection>,
    opponent_to_match: RwLock<Option<Connection>>,
}

impl Server {
    pub async fn new() -> Result<Arc<Self>, tokio::io::Error> {
        let listener = TcpListener::bind("127.0.0.1:8080").await?;

        Ok(Arc::new(Server {
            listener: RwLock::new(listener),
            open_lobbies: DashMap::new(),
            opponent_to_match: RwLock::new(None),
        }))
    }

    pub async fn run(self: &Arc<Server>) {
        let listener = self.listener.read().await;
        loop {
            let (socket, addr) = match listener.accept().await {
                Ok((socket, addr)) => (socket, addr),

                Err(e) => {
                    println!("Error: {}", e);
                    break;
                }
            };

            let cloned_self = self.clone();
            tokio::spawn(async move {
                if let Err(e) = cloned_self.handle_accept(socket, addr).await {
                    eprintln!("{}", e);
                }
            });
        }
    }

    async fn handle_accept(
        self: &Arc<Server>,
        stream: TcpStream,
        addr: SocketAddr,
    ) -> Result<(), WsError> {
        let mut connection = Connection::new(stream, addr).await?;

        let packet = tokio::time::timeout(HANDSHAKE_TIMEOUT, connection.next())
            .await
            .map_err(|e| WsError::Io(e.into()))?;

        match packet {
            Some((_, ServerboundPacket::RequestLobby)) => self.handle_play_friend(connection).await,
            Some((_, ServerboundPacket::JoinQueue)) => {
                self.handle_random_opponent(connection).await
            }
            _ => {
                return Err(WsError::Io(io::Error::new(
                    io::ErrorKind::InvalidData,
                    "invalid packet",
                )))
            }
        }

        Ok(())
    }

    async fn handle_play_friend(self: &Arc<Self>, connection: Connection) {
        let lobby_code = self.random_code();

        connection
            .send(ClientboundPacket::LobbyResponse {
                code: lobby_code.clone(),
            })
            .await;

        self.open_lobbies.insert(lobby_code, connection);
    }

    async fn handle_random_opponent(self: &Arc<Self>, connection: Connection) {
        let mut opponent = self.opponent_to_match.write().await;
        match opponent.take() {
            Some(opp) => {
            }
            None => {
                let _ = opponent.insert(connection);
            }
        }
    }

    fn random_code(self: &Arc<Self>) -> String {
        let mut rng = rand::thread_rng();

        loop {
            let mut code = String::new();
            for _ in 0..5 {
                let char_idx = rng.gen_range(0..CODE_CHARS.len());
                code.push(CODE_CHARS[char_idx] as char);
            }

            if !self.open_lobbies.contains_key(&code) {
                return code;
            }
        }
    }
}

#[tokio::main]
async fn main() {
    let server = Server::new().await.unwrap();
    server.run().await;
}
