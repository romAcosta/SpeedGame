using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using NativeWebSocket;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using System.Net.Security;
using System.Linq;

public class Connection : MonoBehaviour
{
  WebSocket websocket;
  [SerializeField] private GameLogic gameLogic;
  [SerializeField] private string startScene = "Speed";
  void Awake()
  {
    DontDestroyOnLoad(this.gameObject);
  }
  // Start is called before the first frame update
  async void Start()
  {
    websocket = new WebSocket("ws://127.0.0.1:8080");

    websocket.OnOpen += () =>
    {
      Debug.Log("Connection open!");
    };

    websocket.OnError += (e) =>
    {
      Debug.Log("Error! " + e);
    };

    websocket.OnClose += (e) =>
    {
      Debug.Log("Connection closed!");
    };

    websocket.OnMessage += (bytes) =>
    {
      switch (bytes [0])
      {
        case 0:
          Debug.Log("Join_Queue_Request Successfull In Queue");
          Debug.Log(bytes);
          break;
        case 1:
          Debug.Log("GameStart Successfull");
          SceneManager.LoadScene(startScene);
          break;
        case 2:
          Debug.Log("Game Setup Successfull");
          foreach (byte value in bytes .Skip(1))
          { 
            gameLogic.DrawCard(value);
          }
          break;
        case 3:
            gameLogic.FlipMiddleStacks(bytes[1], bytes[2]);   
            Debug.Log("Card Flip Successfull");
          break;
        case 4:
        
          Debug.Log("Card Play Has Been Accepted");
          for (int i = 0; i < bytes.Length - 1; i++)
          {
            Debug.Log(bytes[i]);
          }
          break;
        case 5:
          Debug.Log("Card Has Been Rejected");
          break;
        case 6:
          Debug.Log("Inactivity Alert Has Been received");
          break;
        case 7:
          Debug.Log("DrawCard Request Successfull");
          break;
        default:
          Debug.Log("Unknown message type: " + bytes [0] + "Server Respone Invalid");
          break;
      }
    };

    // Keep sending messages at every 0.3s
    

    // waiting for messages
    await websocket.Connect();
  }

  void Update()
  {
    #if !UNITY_WEBGL || UNITY_EDITOR
      websocket.DispatchMessageQueue();
    #endif
  }
  public async  void Join_Queue_Request()
  {
    if (websocket.State == WebSocketState.Open)
    {
      Debug.Log("Sending Join_Queue_Request");
      await websocket.Send(new byte[]{0});
    }
  }
  public async  void Lobby_Code_Request()
  {
    if (websocket.State == WebSocketState.Open)
    {
      await websocket.Send(new byte[]{1});
      Debug.Log("Join_Queue_Request Successfull In Queue");
    }
  }
  
  private async void OnApplicationQuit()
  {
    await websocket.Close();
  }
}