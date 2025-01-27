use std::io::{self, ErrorKind};
use tracing::{debug, error, warn};

use futures::{SinkExt, StreamExt};
use tokio::net::TcpStream;
use tokio::sync::{mpsc, Mutex};
use tokio_tungstenite::tungstenite::Message;

use crate::packets::{ClientboundPacket, ServerboundPacket};

type WsError = tokio_tungstenite::tungstenite::Error;

pub struct Connection {
    outbound_tx: mpsc::Sender<Message>,
    inbound_rx: Mutex<mpsc::Receiver<ServerboundPacket>>,
}

impl Connection {
    pub async fn new(stream: TcpStream) -> Result<Connection, WsError> {
        let conn = tokio_tungstenite::accept_async(stream).await?;
        let (outbound_tx, outbound_rx) = mpsc::channel(16);
        let (inbound_tx, inbound_rx) = mpsc::channel(16);

        tokio::spawn(async move {
            if let Err(e) = Self::io_loop(conn, inbound_tx, outbound_rx).await {
                warn!("Error in websocket IO loop: {}", e);
            }
        });

        Ok(Connection {
            outbound_tx,
            inbound_rx: Mutex::new(inbound_rx),
        })
    }

    pub async fn io_loop(
        mut conn: tokio_tungstenite::WebSocketStream<TcpStream>,
        inbound_tx: mpsc::Sender<ServerboundPacket>,
        mut outbound_rx: mpsc::Receiver<Message>,
    ) -> Result<(), tokio_tungstenite::tungstenite::Error> {
        debug!("Starting websocket read loop");

        loop {
            tokio::select! {
                Some(maybe_msg) = conn.next() => {
                    let msg = maybe_msg?;
                    debug!(?msg, "Received websocket message");

                    let packet = Self::parse_packet(msg).await?;
                    if let Err(e) = inbound_tx.send(packet).await {
                        warn!(?e, "Failed to send websocket message");
                        break;
                    }
                }
                Some(msg) = outbound_rx.recv() => {
                    debug!(?msg, "Sending websocket message");
                    conn.send(msg).await?;
                },
                else => {
                    debug!("Websocket channel closed");
                    break;
                }
            }
        }

        Ok(())
    }

    pub async fn next(&self) -> Option<ServerboundPacket> {
        self.inbound_rx.lock().await.recv().await
    }

    pub async fn send(&self, packet: ClientboundPacket) {
        debug!(packet = ?packet, "Sending packet");
        let message = Message::Binary(packet.serialize().into());
        let _ = self.outbound_tx.send(message).await;
    }

    async fn parse_packet(message: Message) -> Result<ServerboundPacket, io::Error> {
        let bytes = match message {
            Message::Binary(b) => b,
            other => {
                debug!(?other, "Received non-binary message type");
                return Err(io::Error::new(
                    ErrorKind::InvalidData,
                    "non-binary packet type",
                ));
            }
        };

        ServerboundPacket::deserialize(bytes.to_vec())
    }
}
