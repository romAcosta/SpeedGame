using UnityEngine;

public class HandController : MonoBehaviour
{
    [SerializeField] private bool player = true;
    [SerializeField] private GameLogic gameLogic;
    
    
    public InputData inputData;

    void Update()
    {
        ReadInput();
    }
    
    
    void ReadInput()
    {
        //left
        if (Input.GetKeyDown(inputData.leftKeys[0]))
        {
            gameLogic.PlayCard(player,0,true);
        }
        if (Input.GetKeyDown(inputData.leftKeys[1]))
        {
            gameLogic.PlayCard(player,1,true);
        }
        if (Input.GetKeyDown(inputData.leftKeys[2]))
        {
            gameLogic.PlayCard(player,2,true);
        }
        if (Input.GetKeyDown(inputData.leftKeys[3]))
        {
            gameLogic.PlayCard(player,3,true);
        }
        if (Input.GetKeyDown(inputData.leftKeys[4]))
        {
            gameLogic.PlayCard(player,4,true);
        }
        
        //right
        if (Input.GetKeyDown(inputData.rightKeys[0]))
        {
            gameLogic.PlayCard(player,0,false);
        }
        if (Input.GetKeyDown(inputData.rightKeys[1]))
        {
            gameLogic.PlayCard(player,1,false);
        }
        if (Input.GetKeyDown(inputData.rightKeys[2]))
        {
            gameLogic.PlayCard(player,2,false);
        }
        if (Input.GetKeyDown(inputData.rightKeys[3]))
        {
            gameLogic.PlayCard(player,3,false);
        }
        if (Input.GetKeyDown(inputData.rightKeys[4]))
        {
            gameLogic.PlayCard(player,4,false);
        }
    }
    
    
    
}
