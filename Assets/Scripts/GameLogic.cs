
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class GameLogic : MonoBehaviour
{
    List<(int Rank, string Suit)> _deck = new List<(int Rank, string Suit)>
    {
        (1, "D"), (2, "D"), (3, "D"), (4, "D"), (5, "D"), (6, "D"), (7, "D"), (8, "D"), (9, "D"), (10, "D"), (11, "D"), (12, "D"), (13, "D"),
        (1, "C"), (2, "C"), (3, "C"), (4, "C"), (5, "C"), (6, "C"), (7, "C"), (8, "C"), (9, "C"), (10, "C"), (11, "C"), (12, "C"), (13, "C"),
        (1, "H"), (2, "H"), (3, "H"), (4, "H"), (5, "H"), (6, "H"), (7, "H"), (8, "H"), (9, "H"), (10, "H"), (11, "H"), (12, "H"), (13, "H"),
        (1, "S"), (2, "S"), (3, "S"), (4, "S"), (5, "S"), (6, "S"), (7, "S"), (8, "S"), (9, "S"), (10, "S"), (11, "S"), (12, "S"), (13, "S")
    };
    
    [SerializeField] TMP_Text countdownText;
    
    private Stack<(int Rank, string Suit)> _leftMiddleDeck = new Stack<(int Rank, string Suit)>();
    private Stack<(int Rank, string Suit)> _leftMiddleStack = new Stack<(int Rank, string Suit)>();
    private Stack<(int Rank, string Suit)> _rightMiddleDeck = new Stack<(int Rank, string Suit)>();
    private Stack<(int Rank, string Suit)> _rightMiddleStack = new Stack<(int Rank, string Suit)>();
    private Stack<(int Rank, string Suit)> _playerStack = new Stack<(int Rank, string Suit)>();
    private Stack<(int Rank, string Suit)> _opponentStack = new Stack<(int Rank, string Suit)>();
    private (int Rank, string Suit)[] _playerHand = new (int Rank, string Suit)[5];
    private (int Rank, string Suit)[] _opponentHand = new (int Rank, string Suit)[5];

    private bool go = true;
    private float timer = 3f;
    void Start()
    {
        Shuffle(_deck);
        SetCards();
        
        
    }

    
    void FixedUpdate()
    {
        DrawCards();
        if (go)
        {
            PlayMiddleCards();
        }
    }


    void PlayMiddleCards()
    {
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
        {
            _leftMiddleStack.Push(_leftMiddleDeck.Pop());
            _rightMiddleStack.Push(_rightMiddleDeck.Pop());
            timer = 3f;
            countdownText.enabled = false;
            go = false;
        }
        else
        {
            countdownText.enabled = true;
            countdownText.text = timer.ToString("0");
        }
    }
    
    
    void Shuffle(List<(int Rank, string Suit)> list)
    {
        System.Random random = new System.Random();
        int cards = list.Count;
        while (cards > 1)
        {
            cards--;
            int k = random.Next(cards + 1);
            (list[k], list[cards]) = (list[cards], list[k]); 
        }
    }

    void SetCards()
    {
        bool left = false;
        for (int i = 0; i < _deck.Count; i++)
        {
            if (left)
            {
                if (i < 10) _leftMiddleDeck.Push((_deck[i]));
                else _playerStack.Push(_deck[i]);
                left = false;
            }
            else
            {
                if (i < 10) _rightMiddleDeck.Push(_deck[i]);
                else _opponentStack.Push(_deck[i]);
                left = true;
            }
        }
    }

    void DrawCards()
    {
        for (int i = 0; i < _playerHand.Length; i++)
        {
            if (_playerHand[i] == (0,null))
            {
                _playerHand[i] = _playerStack.Pop();
            }
            if (_opponentHand[i] == (0,""))
            {
                _opponentHand[i] = _opponentStack.Pop();
            }
        }
    }

    #region Getters

    public Stack<(int Rank, string Suit)> LeftMiddleDeck => _leftMiddleDeck;
    public Stack<(int Rank, string Suit)> RightMiddleDeck => _rightMiddleDeck;
    public Stack<(int Rank, string Suit)> RightMiddleStack => _rightMiddleStack;
    public Stack<(int Rank, string Suit)> LeftMiddleStack => _leftMiddleStack;
    public Stack<(int Rank, string Suit)> PlayerStack => _playerStack;
    public Stack<(int Rank, string Suit)> OpponentStack => _opponentStack;
    public (int Rank, string Suit)[] PlayerHand => _playerHand;
    public (int Rank, string Suit)[] OpponentHand => _opponentHand;

    #endregion
   
    
}
