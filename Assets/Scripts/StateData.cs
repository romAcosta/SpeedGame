using UnityEngine;

[CreateAssetMenu(fileName = "StateData", menuName = "GameData/StateData")]
public class StateData : ScriptableObject
{
    [Range(1,2)] public int winnerNum = 1;
}
