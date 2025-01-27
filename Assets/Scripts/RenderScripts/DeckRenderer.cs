using System.Collections.Generic;
using UnityEngine;

public class DeckRenderer : MonoBehaviour
{
    [Range (1,4)] [SerializeField] private int position = 1;
    [SerializeField] private GameLogic gameLogic;
    [SerializeField] private Sprite sprite;
    private SpriteRenderer sr;
    public bool IsEmpty;
    private Stack<(int Rank, string Suit)> deck;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        switch (position)
        {
            case 1:
                sr.sprite = sprite;
                break;
            case 2:
                sr.sprite = sprite;
                break;
            case 3:
                sr.sprite = sprite;
                break;
            case 4:
                sr.sprite = sprite;
                break;
        }
    }
    
    
    void Update()
    {
        if (!IsEmpty)
        {
            sr.sprite = sprite;
        }
        else
        {
            sr.sprite = null;
        }
    }
}
