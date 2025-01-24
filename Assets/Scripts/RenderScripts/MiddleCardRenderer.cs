using UnityEngine;

public class MiddleCardRenderer : MonoBehaviour
{
    [SerializeField] private bool left = true;
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
        if (gameLogic.LeftMiddleStack.Count !> 0)
        {
            if (GetCard() != _lastCard)
            {
                ChangeSprite();
            }
        }
    }
        

    void ChangeSprite()
    {
        _spriteRenderer.sprite = translator.GetCardSprite(GetCard());
    }

    (int Rank, string Suit) GetCard()
    {
        return (left) ? gameLogic.LeftMiddleStack.Peek() : gameLogic.RightMiddleStack.Peek();  
    }
    
    
}
