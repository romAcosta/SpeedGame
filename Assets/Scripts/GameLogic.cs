using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameLogic : MonoBehaviour
{
    private List<(int Rank, string Suit)> _deck = new List<(int Rank, string Suit)>
    {
        (1, "D"), (2, "D"), (3, "D"), (4, "D"), (5, "D"), (6, "D"), (7, "D"), (8, "D"), (9, "D"), (10, "D"), (11, "D"), (12, "D"), (13, "D"),
        (1, "C"), (2, "C"), (3, "C"), (4, "C"), (5, "C"), (6, "C"), (7, "C"), (8, "C"), (9, "C"), (10, "C"), (11, "C"), (12, "C"), (13, "C"),
        (1, "H"), (2, "H"), (3, "H"), (4, "H"), (5, "H"), (6, "H"), (7, "H"), (8, "H"), (9, "H"), (10, "H"), (11, "H"), (12, "H"), (13, "H"),
        (1, "S"), (2, "S"), (3, "S"), (4, "S"), (5, "S"), (6, "S"), (7, "S"), (8, "S"), (9, "S"), (10, "S"), (11, "S"), (12, "S"), (13, "S")
    };
    
    [SerializeField] TMP_Text countdownText;
    [SerializeField] StateData stateData;
    //Middle Cards
    private Stack<(int Rank, string Suit)> _leftMiddleDeck = new Stack<(int Rank, string Suit)>();
    private Stack<(int Rank, string Suit)> _leftMiddleStack = new Stack<(int Rank, string Suit)>();
    private Stack<(int Rank, string Suit)> _rightMiddleDeck = new Stack<(int Rank, string Suit)>();
    private Stack<(int Rank, string Suit)> _rightMiddleStack = new Stack<(int Rank, string Suit)>();
    
    //Player Cards
    private Stack<(int Rank, string Suit)> _playerStack = new Stack<(int Rank, string Suit)>();
    private Stack<(int Rank, string Suit)> _opponentStack = new Stack<(int Rank, string Suit)>();
    private (int Rank, string Suit)[] _playerHand = new (int Rank, string Suit)[5];
    private bool[] _opponentHand = new bool[5] { true, true, true, true, true };

    
    //Utilities
    [Header("Utilities")] 
    [SerializeField] private float errorTime = 0.5f;
    [SerializeField] private float placeTime = 0.1f;

    private bool error = false;
    private float controlTimer = 0.0f;
    private bool go = true;
    private float timer = 3f;

    private Connection connection;
    
    void Start()
    {
        Connections.MAIN = new Connection();
        Connections.MAIN.OnOpen(NetworkingStart);

        Shuffle(_deck);
    }

    async void NetworkingStart()
    {
        Connections.MAIN.SendPacket(new JoinQueuePacket());
        Connections.MAIN.OnPacketReceived += OnPacket;

        Connections.TEMP = new Connection();
        Connections.TEMP.OnOpen(() => {
            Connections.TEMP.SendPacket(new JoinQueuePacket());
        });
    }

    void OnPacket(ClientboundPacket packet)
    {
        switch (packet.Type)
        {
            case ClientboundPacketType.Setup:
                {
                    var p = (SetupPacket) packet;
                    foreach (Card card in p.Hand)
                    {
                        _playerStack.Push(card.ToTuple());
                    }
                }
                break;

            case ClientboundPacketType.FlipCenter:
                {
                    var p = (FlipCenterPacket) packet;
                    _leftMiddleStack.Push(p.CardA.ToTuple());
                    _rightMiddleStack.Push(p.CardB.ToTuple());
                    timer = 5;
                    go = true;
                }
                break;

            case ClientboundPacketType.DrawCard:
                {
                    var p = (DrawCardPacket) packet;
                    for (int i = 0; i < _playerHand.Length; i++)
                    {
                        if (_playerHand[i] == (0, null))
                        {
                            _playerHand[i] = p.Card.ToTuple();
                            break;
                        }
                    }
                }
                break;

            case ClientboundPacketType.PlayCard:
                {
                    var p = (OpponentPlayCardPacket) packet;
                    bool left = (p.ActionId & 1) == 0;
                    var stack = left ? _leftMiddleStack : _rightMiddleStack;
                    stack.Push(p.Card.ToTuple());
                }
                break;

            case ClientboundPacketType.RejectCard:
                {
                    var p = (RejectCardPacket) packet;
                    bool left = (p.ActionId & 1) == 0;
                    var stack = left ? _leftMiddleStack : _rightMiddleStack;

                    for (int i = 0; i < _playerHand.Length; i++)
                    {
                        if (_playerHand[i] == (0, null))
                        {
                            _playerHand[i] = stack.Pop();
                            break;
                        }
                    }

                    error = true;
                    controlTimer = errorTime;
                }
                break;

            case ClientboundPacketType.RemoveCard:
                {
                    for (int i = 0; i < _opponentHand.Length; i++)
                    {
                        if (_opponentHand[i])
                        {
                            _opponentHand[i] = false;

                            if (i == _opponentHand.Length - 1)
                            {
                                stateData.winnerNum = 2;
                                SceneManager.LoadScene("Win");
                            }
                        }
                    }
                }
                break;

            default:
                Debug.LogError("Unexpected packet: " + packet);
                break;
        }
    }

    void Update()
    {
        Connections.MAIN.Update();
        Connections.TEMP.Update();
    }

    void FixedUpdate()
    {
        DrawCards();
        ControlTimerCountdown();
        if (go)
        {
            PlayMiddleCards();
        }
        else
        {
            if (!CheckCards())
            {
                timer = 5;
                go = true;
            }
        }
    }

    #region Middle Cards

    bool CheckCards()
    {
        return true;
    }

    void FlipMiddleStacks()
    {
        _leftMiddleStack.Reverse();
        _rightMiddleStack.Reverse();
        for (int i = 0; i < _leftMiddleStack.Count; i++)
        {
            _leftMiddleDeck.Push(_leftMiddleStack.Pop());
        }
        for (int i = 0; i < _rightMiddleStack.Count; i++)
        {
            _leftMiddleDeck.Push(_leftMiddleStack.Pop());
        }
        
        _leftMiddleStack.Clear();
        _rightMiddleStack.Clear();
    }

    #endregion
    
    #region Controls
    void ControlTimerCountdown()
    {
        
        if (controlTimer > 0)
        {
            controlTimer -= Time.deltaTime;
        }
        else
        {
            error = false;
        }
        
    }
    public void PlayCard(bool player, int position, bool left)
    {
        if (controlTimer <= 0 && !go)
        {
            (int Rank, string Suit)[] currentHand = _playerHand;
            Stack<(int Rank, string Suit)> currentStack = (left) ? _leftMiddleStack : _rightMiddleStack;
            var card = Card.FromTuple(currentHand[position]);
            Connections.MAIN.SendPacket(new PlayCardPacket(card, (byte) (left ? 0 : 1)));
            currentStack.Push(currentHand[position]);
            currentHand[position] = (0, null);

            for (int i = 0; i < currentHand.Length; i++)
            {
                if (currentHand[i] != (0, null))
                {
                    return;
                }
            }

            stateData.winnerNum = 1;
            SceneManager.LoadScene("Win");
        }

    }
    bool ValidatePlacement((int Rank, string Suit) newCard, (int Rank, string Suit) pastCard)
    {
        if(newCard.Rank == 0) return false;
        if ((newCard.Rank == 1 && pastCard.Rank == 13) || (newCard.Rank == 13 && pastCard.Rank == 1))
        {
            return true;
        }

        if (newCard.Rank == (pastCard.Rank + 1) || newCard.Rank == (pastCard.Rank - 1))
        {
            return true;
        }
        
        return false;
    }
    
    #endregion
    
    #region Set Up
    void PlayMiddleCards()
    {
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
        {
            if (_leftMiddleStack.Count == 0)
            {
                FlipMiddleStacks();
            }
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

    void DrawCards()
    {
        if (controlTimer <= 0)
        {
            for (int i = 0; i < _playerHand.Length; i++)
            {
                if (_playerHand[i] == (0, null) && _playerStack.Count > 0)
                {
                    _playerHand[i] = _playerStack.Pop();
                }

                /*if (_opponentHand[i] == (0, null) && _opponentStack.Count > 0)
                {
                    _opponentHand[i] = _opponentStack.Pop();
                }*/
            }
        }
    }

    #endregion
    
    #region Getters

    public Stack<(int Rank, string Suit)> LeftMiddleDeck => _leftMiddleDeck;
    public Stack<(int Rank, string Suit)> RightMiddleDeck => _rightMiddleDeck;
    public Stack<(int Rank, string Suit)> RightMiddleStack => _rightMiddleStack;
    public Stack<(int Rank, string Suit)> LeftMiddleStack => _leftMiddleStack;
    public Stack<(int Rank, string Suit)> PlayerStack => _playerStack;
    public Stack<(int Rank, string Suit)> OpponentStack => _opponentStack;
    public (int Rank, string Suit)[] PlayerHand => _playerHand;
    public bool[] OpponentHand => _opponentHand;
    

    public bool Error { get => error; }

    #endregion
    
}
