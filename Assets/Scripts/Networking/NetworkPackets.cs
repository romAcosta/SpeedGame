using System.Collections.Generic;
public enum ServerboundPacketType : byte
{
    JoinQueue = 0,
    RequestLobby = 1,
    PlayCard = 2,
    JoinLobby = 3
}

public enum ClientboundPacketType : byte
{
    LobbyResponse = 0,
    BeginGame = 1,
    Setup = 2,
    FlipCenter = 3,
    PlayCard = 4,
    RejectCard = 5,
    Inactivity = 6,
    DrawCard = 7
}

public abstract class ServerboundPacket
{
    public abstract ServerboundPacketType Type { get; }
    public abstract byte[] Serialize();
}

public class JoinQueuePacket : ServerboundPacket
{
    public override ServerboundPacketType Type => ServerboundPacketType.JoinQueue;
    public override byte[] Serialize() => new[] { (byte)Type };
}

public class RequestLobbyPacket : ServerboundPacket 
{
    public override ServerboundPacketType Type => ServerboundPacketType.RequestLobby;
    public override byte[] Serialize() => new[] { (byte)Type };
}

public class PlayCardPacket : ServerboundPacket
{
    public Card Card { get; }
    public byte ActionId { get; }

    public PlayCardPacket(Card card, byte actionId)
    {
        Card = card;
        ActionId = actionId;
    }

    public override ServerboundPacketType Type => ServerboundPacketType.PlayCard;
    
    public override byte[] Serialize()
    {
        return new[] { (byte)Type, Card.Serialize(), ActionId };
    }
}

public class JoinLobbyPacket : ServerboundPacket
{
    public string Code { get; }

    public JoinLobbyPacket(string code)
    {
        Code = code;
    }

    public override ServerboundPacketType Type => ServerboundPacketType.JoinLobby;

    public override byte[] Serialize()
    {
        var result = new List<byte> { (byte)Type };
        result.AddRange(System.Text.Encoding.UTF8.GetBytes(Code));
        return result.ToArray();
    }
}

public abstract class ClientboundPacket
{
    public abstract ClientboundPacketType Type { get; }
    
    public static ClientboundPacket Deserialize(byte[] data)
    {
        if (data.Length == 0) return null;

        var type = (ClientboundPacketType)data[0];
        var payload = data[1..];

        return type switch
        {
            ClientboundPacketType.LobbyResponse => new LobbyResponsePacket(payload),
            ClientboundPacketType.BeginGame => new BeginGamePacket(),
            ClientboundPacketType.Setup => new SetupPacket(payload),
            ClientboundPacketType.FlipCenter => new FlipCenterPacket(payload),
            ClientboundPacketType.PlayCard => new OpponentPlayCardPacket(payload),
            ClientboundPacketType.RejectCard => new RejectCardPacket(payload),
            ClientboundPacketType.Inactivity => new InactivityPacket(payload),
            ClientboundPacketType.DrawCard => new DrawCardPacket(payload),
            _ => null
        };
    }
}

public class LobbyResponsePacket : ClientboundPacket
{
    public string Code { get; }
    public override ClientboundPacketType Type => ClientboundPacketType.LobbyResponse;

    public LobbyResponsePacket(byte[] payload)
    {
        Code = System.Text.Encoding.UTF8.GetString(payload);
    }
}

public class BeginGamePacket : ClientboundPacket
{
    public override ClientboundPacketType Type => ClientboundPacketType.BeginGame;
}

public class SetupPacket : ClientboundPacket
{
    public List<Card> Hand { get; }
    public override ClientboundPacketType Type => ClientboundPacketType.Setup;

    public SetupPacket(byte[] payload)
    {
        Hand = new List<Card>();
        foreach (var cardByte in payload)
        {
            Hand.Add(Card.Deserialize(cardByte));
        }
    }
}

public class FlipCenterPacket : ClientboundPacket
{
    public Card CardA { get; }
    public Card CardB { get; }
    public override ClientboundPacketType Type => ClientboundPacketType.FlipCenter;

    public FlipCenterPacket(byte[] payload)
    {
        CardA = Card.Deserialize(payload[0]);
        CardB = Card.Deserialize(payload[1]);
    }
}

public class OpponentPlayCardPacket : ClientboundPacket
{
    public Card Card { get; }
    public byte ActionId { get; }
    public override ClientboundPacketType Type => ClientboundPacketType.PlayCard;

    public OpponentPlayCardPacket(byte[] payload)
    {
        Card = Card.Deserialize(payload[0]);
        ActionId = payload[1];
    }
}

public class RejectCardPacket : ClientboundPacket
{
    public byte ActionId { get; }
    public override ClientboundPacketType Type => ClientboundPacketType.RejectCard;

    public RejectCardPacket(byte[] payload)
    {
        ActionId = payload[0];
    }
}

public class InactivityPacket : ClientboundPacket
{
    public InactivityLevel Level { get; }
    public override ClientboundPacketType Type => ClientboundPacketType.Inactivity;

    public InactivityPacket(byte[] payload)
    {
        Level = (InactivityLevel)payload[0];
    }
}

public enum InactivityLevel : byte
{
    Highlight = 0,
    ForcePlay = 1
}

public class DrawCardPacket : ClientboundPacket
{
    public Card Card { get; }
    public override ClientboundPacketType Type => ClientboundPacketType.DrawCard;

    public DrawCardPacket(byte[] payload)
    {
        Card = Card.Deserialize(payload[0]);
    }
}
