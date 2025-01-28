using UnityEngine;

[CreateAssetMenu(fileName = "InputData", menuName = "GameData/InputData")]
public class InputData : ScriptableObject
{
    public KeyCode[] leftKeys =
    {
        KeyCode.A,
        KeyCode.S,
        KeyCode.D,
        KeyCode.F,
        KeyCode.G
    };
    
    public KeyCode[] rightKeys =
    {
        KeyCode.H,
        KeyCode.J,
        KeyCode.K,
        KeyCode.L,
        KeyCode.Semicolon
    };
}
