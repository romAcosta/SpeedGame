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
        if (HasCard())
        {
            _spriteRenderer.sprite = sprite;
        }
        else
        {
            _spriteRenderer.sprite = null;
        }
    }
    
    bool HasCard()
    {
        return (player) ? gameLogic.PlayerHand[position] != (0, null) : gameLogic.OpponentHand[position];  
    }
}
