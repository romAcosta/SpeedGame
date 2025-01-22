using UnityEngine;

public class HandCardRenderer : MonoBehaviour
{
    [SerializeField] private bool player = true;
    [SerializeField] private int position = 1;
    [SerializeField] private GameLogic gameLogic;
    [SerializeField] private CardTranslator translator;
    
    
    private (int Rank, string Suit) _lastCard = (0, null);
    private bool change = false;
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = null;
    }
    
    void Update()
    {
        if (GetCard() != _lastCard)
        {
            ChangeSprite();
        }
    }

    void ChangeSprite()
    {
        GetComponent<SpriteRenderer>().sprite = translator.GetCardSprite(GetCard());
    }

    (int Rank, string Suit) GetCard()
    {
        return (player) ? gameLogic.PlayerHand[position] : gameLogic.OpponentHand[position];  
    }
    
    
}
