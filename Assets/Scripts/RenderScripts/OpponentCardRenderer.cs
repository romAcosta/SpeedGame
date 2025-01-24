using UnityEngine;

public class OpponentCardRenderer: MonoBehaviour
{
    [SerializeField] private bool player = false;
    [SerializeField] private int position = 0;
    [SerializeField] private GameLogic gameLogic;
    [SerializeField] private Sprite sprite;
    
    
    
    
    private SpriteRenderer _spriteRenderer;
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = null;
    }
    
    void Update()
    {
        if (GetCard() != (0,null))
        {
            _spriteRenderer.sprite = sprite;
        }
        else
        {
            _spriteRenderer.sprite = null;
        }
    }
    
    (int Rank, string Suit) GetCard()
    {
        return (player) ? gameLogic.PlayerHand[position]: gameLogic.OpponentHand[position];  
    }
    
    
}
