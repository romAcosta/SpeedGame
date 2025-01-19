
using System.Collections.Generic;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class GameLogic : MonoBehaviour
{
    List<(int Rank, string Suit)> _deck = new List<(int Rank, string Suit)>
    {
        (1, "D"), (2, "D"), (3, "D"), (4, "D"), (5, "D"), (6, "D"), (7, "D"), (8, "D"), (9, "D"), (10, "D"), (11, "D"), (12, "D"), (13, "D"),
        (1, "C"), (2, "C"), (3, "C"), (4, "C"), (5, "C"), (6, "C"), (7, "C"), (8, "C"), (9, "C"), (10, "C"), (11, "C"), (12, "C"), (13, "C"),
        (1, "H"), (2, "H"), (3, "H"), (4, "H"), (5, "H"), (6, "H"), (7, "H"), (8, "H"), (9, "H"), (10, "H"), (11, "H"), (12, "H"), (13, "H"),
        (1, "S"), (2, "S"), (3, "S"), (4, "S"), (5, "S"), (6, "S"), (7, "S"), (8, "S"), (9, "S"), (10, "S"), (11, "S"), (12, "S"), (13, "S")
    };
    
    
    void Start()
    {
        Shuffle(_deck);
        foreach (var card in _deck)
        { 
            Debug.Log(card.Rank  + card.Suit);
        }
    }

    // Update is called once per frame
    // void Update()
    // {
    //     
    // }
    
    
    
    
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
}
