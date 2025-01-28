using System.Collections.Generic;
using UnityEngine;

public class DeckRenderer : MonoBehaviour
{
    [Range (1,4)] [SerializeField] private int position = 1;
    [SerializeField] private GameLogic gameLogic;
    [SerializeField] private Sprite sprite;
    private SpriteRenderer sr;

    private Stack<(int Rank, string Suit)> deck;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        switch (position)
        {
            case 1:
                //deck = gameLogic.PlayerStack;
                break;
            case 2:
                //deck = gameLogic.OpponentStack;
                break;
            case 3:
                //deck = gameLogic.LeftMiddleDeck;
                break;
            case 4:
                //deck = gameLogic.RightMiddleDeck;
                break;
        }
    }
    
    
    void Update()
    {
        /*if (deck.Count > 0)
        {
            sr.sprite = sprite;
        }
        else
        {
            sr.sprite = null;
        }*/
        sr.sprite = sprite;
    }
}
