mod card;
mod connection;
mod game;
mod packets;
mod protocol;

use std::io;
use std::net::SocketAddr;
use std::sync::Arc;
use std::time::Duration;

use connection::Connection;
use dashmap::DashMap;
use game::Game;
use packets::{ClientboundPacket, ServerboundPacket};
use rand::Rng;
use tokio::net::{TcpListener, TcpStream};
use tokio::sync::RwLock;
use tracing::{debug, info};

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
        let mut connection = Connection::new(stream).await?;

        let packet = tokio::time::timeout(HANDSHAKE_TIMEOUT, connection.next())
            .await
            .map_err(|e| WsError::Io(e.into()))?;

        match packet {
            Some(ServerboundPacket::RequestLobby) => {
                debug!(addr = ?addr, "Client requested lobby creation");
                self.handle_play_friend(connection).await
            }
            Some(ServerboundPacket::JoinQueue) => {
                debug!(addr = ?addr, "Client joined matchmaking queue");
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

        debug!(lobby = lobby_code, "Creating new lobby");

        connection
            .send(ClientboundPacket::LobbyResponse {
                code: lobby_code.clone(),
            })
            .await;

        self.open_lobbies.insert(lobby_code.clone(), connection);
        debug!(lobby = lobby_code, "Lobby created and awaiting opponent");
    }

    async fn handle_random_opponent(self: &Arc<Self>, connection: Connection) {
        let mut opponent = self.opponent_to_match.write().await;
        match opponent.take() {
            Some(opp) => {
                debug!("Matching players for random game");
                let game = Game::new(connection, opp);
                tokio::spawn(async move { game.start().await });
            }
            None => {
                debug!("Player joined matchmaking queue");
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
    tracing_subscriber::fmt()
        .with_max_level(tracing::Level::DEBUG)
        .with_target(true)
        .with_thread_ids(true)
        .with_file(true)
        .with_line_number(true)
        .init();

    info!("Starting Speed card game server");
    let server = Server::new().await.unwrap();
    info!("Server initialized, accepting connections");
    server.run().await;
}
