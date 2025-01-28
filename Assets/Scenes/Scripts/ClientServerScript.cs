using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ClientServerScript : MonoBehaviour
{
    [SerializeField] private string scene;
    void Awake(){
        DontDestroyOnLoad(this.gameObject);
    }
    public void LobbyJoinRequest()
    {
        SceneManager.LoadScene(scene);
    }

    public string LobbyCodeRequest()
    {
        return "Success";
    }
}
