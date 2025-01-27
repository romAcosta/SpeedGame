using System;
using UnityEngine;

public class CardTranslator : MonoBehaviour
{
    [SerializeField] private Sprite[] cardSprites = new Sprite[51];

    public Sprite GetCardSprite((int Rank, string Suit) card)
    {
        int index = card.Rank - 1;
        switch (card.Suit)
        {
            case "C":
                return cardSprites[0 + index];
            case "H":
                return cardSprites[13 + index];
            case "S":
                return cardSprites[26 + index];
            case "D":
                return cardSprites[39 + index];
            case null:
                return null;
            default:
                return null;
        }
    }
    public static (int , string) DecodeCard(byte card){
        int value = (card >> 2) & 0b00111111;
        return (value, DecodeSuit(card & 0b00000011));
    }
    private static string DecodeSuit(int suitValue)
    {
        return suitValue switch
        {
            0 => "H",  // Hearts
            1 => "D",  // Diamonds
            2 => "S",  // Spades
            3 => "C",  // Clubs
            _ => null
        };
    }
}
