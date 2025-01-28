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
   
}
