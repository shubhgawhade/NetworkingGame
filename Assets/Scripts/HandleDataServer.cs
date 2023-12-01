using System;
using System.Collections.Generic;
using UnityEngine;

public class HandleDataServer : MonoBehaviour
{
    private AsynchronousSocketListener server;
    
    public const int MaxPlayers = 4;
    
    public List<int> _playerIDs = new List<int>();
    // private int[] playerIDs = { 1, 2, 3, 4 };


    public GameState gameState = new GameState();
    
    private void Awake()
    {
        server = AsynchronousSocketListener.asynchronousSocketListener;
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < MaxPlayers; i++)
        {
            _playerIDs.Add(i);
        }
        
        AsynchronousSocketListener.ProcessDataServer += ProcessDataServer;
    }

    void ProcessDataServer(Player state, object data, DataUpdateType dataTye)
    {
        switch (state.dataUpdateType)
        {
            case DataUpdateType.Joining:

                JoiningData joiningData = (JoiningData) data;
                
                state.playerName = joiningData.playerName;
                
                state.dataUpdateType = DataUpdateType.JoiningDataReply;
                JoinLeaveData joinLeaveData = (JoinLeaveData) state.returnDataStruct;

                if (_playerIDs.Count == 0)
                {
                    Debug.LogWarning($"{state.playerName} tried to join when SERVER HAS MAX PLAYERS");
                    // MAX PLAYERS : SEND DISCONNECTION

                    server.QuitClient(state.workSocket, state, -1);
                    
                    break;
                }
                
                print(_playerIDs.Count);
                if (_playerIDs.Count > 0)
                {
                    state.PlayerID = _playerIDs[0];
                    Debug.Log($"{state.playerName} CONNECTED!");
                    
                    _playerIDs.Remove(_playerIDs[0]);
                }
                else
                {
                    
                }
                
                joinLeaveData.playersConnected = new string[ExternServer.ConnectedPlayers.Count];
                joinLeaveData.playerIDs = new int[ExternServer.ConnectedPlayers.Count];
                joinLeaveData.ready = new bool[ExternServer.ConnectedPlayers.Count];
                for (int i = 0; i < ExternServer.ConnectedPlayers.Count; i++)
                {
                    joinLeaveData.playersConnected[i] = ExternServer.ConnectedPlayers[i].playerName;
                    joinLeaveData.playerIDs[i] = ExternServer.ConnectedPlayers[i].PlayerID;
                    joinLeaveData.ready[i] = ExternServer.ConnectedPlayers[i].ready;
                }
                // joiningDataReply.playersConnected = "AA";
                state.dataToSend = state.ObjectToByteArray(joinLeaveData);
                Debug.Log($"{state.dataToSend.Length} {state.dataUpdateType}");
                SendData.Send(state, state.dataToSend, SendData.SendType.ReplyAll);
                
                break;
            
            case DataUpdateType.Ready:

                ReadyStatus readyStatus = (ReadyStatus) data;
                if (readyStatus.ready)
                {

                    // SceneManager.LoadScene(1);
                    print("Start Game");

                    gameState.ChangeState(GameState.gameStateEnum.Game);

                    // SpawnPlayers();

                    // state.dataUpdateType = DataUpdateType.StartGame;
                    // StartGameData startGameData = (StartGameData)state.returnDataStruct;
                    // startGameData.playerID = state.PlayerID;

                }
                
                break;
            
            case DataUpdateType.JoiningDataReply:

                JoinLeaveData joinLeaveDataa = (JoinLeaveData) data;
                if (joinLeaveDataa.errorCode != -1)
                {
                    _playerIDs.Insert(0, state.PlayerID);
                }
                
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        AsynchronousSocketListener.ProcessDataServer -= ProcessDataServer;
    }
}