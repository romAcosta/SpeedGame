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

    public static Card FromTuple((int rank, string suit) tuple)
    {
        Suit suit = Suit.Hearts;
        switch (tuple.suit)
        {
            case "H":
                suit = Suit.Hearts;
                break;
            case "D":
                suit = Suit.Diamonds;
                break;
            case "S":
                suit = Suit.Spades;
                break;
            case "C":
                suit = Suit.Clubs;
                break;
        }
        return new Card(suit, (byte) (tuple.rank - 1));
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

    public (int, string) ToTuple()
    {
        return (Rank + 1, Suit.ToString().Substring(0, 1));
    }

    public override string ToString()
    {
        return (Rank + 1) + " of " + Suit;
    }
}
