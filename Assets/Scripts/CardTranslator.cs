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
                Debug.Log("Null");
                return null;
            default:
                Debug.Log("Invalid Suit");
                return null;
        }
    }
    public static (int , string) DecodeCard(byte card){
        Debug.Log("Value:" + card);
        string newCard = Convert.ToString( card & 0b111111, 2).PadLeft(6, '0');
        Debug.Log(newCard);
        // Convert.ToInt16(newCard.Substring(2,4), 2)
        // DecodeSuit(Convert.ToInt16(newCard.Substring(0, 2), 2))
        return (card >> 2, DecodeSuit(card & 0b11));
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
