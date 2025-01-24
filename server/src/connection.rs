use std::io::{self, ErrorKind};
use std::net::SocketAddr;

use futures::stream::{SplitSink, SplitStream};
use futures::{SinkExt, StreamExt};
use tokio::net::TcpStream;
use tokio::sync::mpsc;
use tokio_tungstenite::tungstenite::Message;
use tokio_tungstenite::WebSocketStream;

use crate::packets::{ClientboundPacket, ServerboundPacket};

type Socket = WebSocketStream<TcpStream>;
type WsError = tokio_tungstenite::tungstenite::Error;

pub struct Connection {
    outbound_tx: mpsc::Sender<Message>,
    inbound_rx: mpsc::Receiver<(SocketAddr, ServerboundPacket)>,
}

impl Connection {
    pub async fn new(stream: TcpStream, address: SocketAddr) -> Result<Connection, WsError> {
        let (write, read) = tokio_tungstenite::accept_async(stream).await?.split();
        let (outbound_tx, outbound_rx) = mpsc::channel(16);
        let (inbound_tx, inbound_rx) = mpsc::channel(16);

        tokio::spawn(Self::read_loop(read, address, inbound_tx));
        tokio::spawn(Self::write_loop(write, outbound_rx));

        Ok(Connection {
            outbound_tx,
            inbound_rx,
        })
    }

    pub async fn read_loop(
        mut read: SplitStream<Socket>,
        address: SocketAddr,
        inbound_tx: mpsc::Sender<(SocketAddr, ServerboundPacket)>,
    ) -> Result<(), tokio_tungstenite::tungstenite::Error> {
        while let Some(Ok(msg)) = read.next().await {
            let packet = Self::parse_packet(msg).await?;
            if let Err(_) = inbound_tx.send((address, packet)).await {
                break;
            }
        }

        Ok(())
    }

    pub async fn write_loop(
        mut write: SplitSink<Socket, Message>,
        mut outbound_rx: mpsc::Receiver<Message>,
    ) -> Result<(), tokio_tungstenite::tungstenite::Error> {
        while let Some(msg) = outbound_rx.recv().await {
            write.send(msg).await?;
        }

        Ok(())
    }

    pub async fn next(&mut self) -> Option<(SocketAddr, ServerboundPacket)> {
        self.inbound_rx.recv().await
    }

    pub async fn send(&self, packet: ClientboundPacket) {
        let message = Message::Binary(packet.serialize().into());
        let _ = self.outbound_tx.send(message).await;
    }

    async fn parse_packet(message: Message) -> Result<ServerboundPacket, io::Error> {
        let bytes = match message {
            Message::Binary(b) => b,
            _ => {
                return Err(io::Error::new(
                    ErrorKind::InvalidData,
                    "non-binary packet type",
                ))
            }
        };

        ServerboundPacket::deserialize(bytes.to_vec())
    }
}
