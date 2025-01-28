using UnityEngine;

public class HandCardRenderer : MonoBehaviour
{
    [SerializeField] private bool player = true;
    [SerializeField] private int position = 1;
    [SerializeField] private GameLogic gameLogic;
    [SerializeField] private CardTranslator translator;
    
    
    private (int Rank, string Suit) _lastCard = (0, null);
    private SpriteRenderer _spriteRenderer;
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = null;
    }
    
    void Update()
    {
        if (gameLogic.Error == true)
        {
            _spriteRenderer.color = new Color32(244, 148, 148, 255);
        }
        else
        {
            _spriteRenderer.color = Color.white;
        }
        
        
        if (GetCard() != _lastCard)
        {
            ChangeSprite();
        }

        if (GetCard() == (0, null))
        {
            _spriteRenderer.sprite = null;
        }
    }

    void ChangeSprite()
    {
        _spriteRenderer.sprite = translator.GetCardSprite(GetCard());
    }

    (int Rank, string Suit) GetCard()
    {
        return (player) ? gameLogic.PlayerHand[position] : gameLogic.OpponentHand[position];  
    }
    
    
}
