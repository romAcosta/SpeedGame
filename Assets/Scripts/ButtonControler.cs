
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
public class ButtonUI : MonoBehaviour
{
    public Connection connection;
    [SerializeField] private string startScene = "Speed";
    public void Start_Game(){
        SceneManager.LoadScene(startScene);
    }
}
