using System;
using TMPro;
using UnityEngine;

public class TextUpdate : MonoBehaviour
{
    [SerializeField] StateData stateData;
    private TMP_Text text;

    void Start()
    {
        text = GetComponent<TMP_Text>();
    }
    
    private void Update()
    {
        text.text = "Player " + stateData.winnerNum + " wins!"; 
    }
}
