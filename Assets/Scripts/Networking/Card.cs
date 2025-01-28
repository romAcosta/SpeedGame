using System;

public enum Suit
{
    Hearts,
    Diamonds,
    Spades,
    Clubs
}

public class Card
{
    public Suit Suit { get; }
    public byte Rank { get; } // ace = 0, king = 12

    public Card(Suit suit, byte rank)
    {
        Suit = suit;
        Rank = rank;
    }

    public byte Serialize()
    {
        return (byte)((byte)Suit | (Rank << 2));
    }

    public static Card Deserialize(byte data)
    {
        var suit = (Suit)(data & 0b11);
        var rank = (byte)(data >> 2);
        return new Card(suit, rank);
    }

    public bool StackableOn(Card other)
    {
        var diff = Math.Abs((int)Rank - (int)other.Rank);
        return diff == 1 || diff == 12;
    }
}
