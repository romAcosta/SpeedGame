using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameLogic : MonoBehaviour
{
    [SerializeField] TMP_Text countdownText;
    [SerializeField] StateData stateData;
    //Middle Cards
    private Stack<(int Rank, string Suit)> _leftMiddleStack = new Stack<(int Rank, string Suit)>();
    private Stack<(int Rank, string Suit)> _rightMiddleStack = new Stack<(int Rank, string Suit)>();
    
    //Player Cards
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
    
    void Start()
    {
        Connections.MAIN = new Connection();
        Connections.MAIN.OnOpen(NetworkingStart);
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
                    _playerHand = p.Hand.Select(card => card.ToTuple()).ToArray();
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
        ControlTimerCountdown();
        if (go)
        {
            PlayMiddleCards();
        }
    }
    
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
    
    #endregion
    #region Set Up

    void PlayMiddleCards()
    {
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
        {
            if (_leftMiddleStack.Count == 0)
            {
                // todo flip animation
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
    
    #endregion
    
    #region Getters

    public Stack<(int Rank, string Suit)> RightMiddleStack => _rightMiddleStack;
    public Stack<(int Rank, string Suit)> LeftMiddleStack => _leftMiddleStack;
    public (int Rank, string Suit)[] PlayerHand => _playerHand;
    public bool[] OpponentHand => _opponentHand;
    

    public bool Error { get => error; }

    #endregion
    
}
