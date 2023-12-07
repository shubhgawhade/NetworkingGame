using System;
using System.Collections;
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
        
        // AsynchronousSocketListener.ProcessDataServer += ProcessDataServer;
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
        if (server.receivedDataToHandle.Count > 0)
        {
            DataToHandle data = server.receivedDataToHandle[0];

            switch (server.receivedDataToHandle[0].dataUpdateType)
            {
                case DataUpdateType.Joining:
                    
                    JoiningData joiningData = (JoiningData) data.deserializedData;
                    if (data.player.playerName == null)
                    {
                        data.player.playerName = joiningData.playerName;
                        data.player.dataUpdateType = DataUpdateType.JoiningDataReply;
                        
                        JoinLeaveData joinLeaveData = (JoinLeaveData) data.player.returnDataStruct;

                        if (_playerIDs.Count == 0)
                        {
                            Debug.LogWarning($"{data.player.playerName} tried to join when SERVER HAS MAX PLAYERS");
                            // MAX PLAYERS : SEND DISCONNECTION

                            server.QuitClient(data.player.workSocket, data.player, -1);
                    
                            break;
                        }
                
                        print(_playerIDs.Count);
                        if (_playerIDs.Count > 0)
                        {
                            data.player.PlayerID = _playerIDs[0];
                            Debug.Log($"{data.player.playerName} CONNECTED!");
                    
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
                        data.player.dataToSend = data.player.ObjectToByteArray(joinLeaveData);
                        Debug.Log($"{data.player.dataToSend.Length} {data.player.dataUpdateType}");
                        SendData.Send(data.player, data.player.dataToSend, SendData.SendType.ReplyAll);
                    }
            
                    break;
                
                case DataUpdateType.Ready:

                    ReadyStatus readyStatus = (ReadyStatus) data.deserializedData;
                    // readyStatus.playerID = 1;
                    readyStatus.playerID = data.player.PlayerID;
                    data.player.ready = readyStatus.ready;

                    data.player.dataToSend = data.player.ObjectToByteArray(readyStatus);
                    SendData.Send(data.player, data.player.dataToSend, SendData.SendType.ReplyAll);

                    // if (playersConnected.Count > 1)
                    {
                        bool allReady = false;
                        foreach (Player player in server.playersConnected)
                        {
                            if (!player.ready)
                            {
                                allReady = false;
                                break;
                            }

                            allReady = true;
                            
                        }
                        
                        if (allReady)
                        {
                            print("Start Game");

                            StartCoroutine(Wait());

                            gameState.ChangeState(GameState.gameStateEnum.Game);
                        }
                    }
                
                    break;
                
                case DataUpdateType.Transform:

                    TransformData transformData = (TransformData) data.deserializedData;
                    string content = $"{transformData.pos._posX}, {transformData.pos._posY}, {transformData.pos._posZ}";
                    Debug.Log($"{data.player.playerName} : {content}");
                
                    transformData.playerID = data.player.PlayerID;
                    data.player.dataToSend = data.player.ObjectToByteArray(transformData);
                    SendData.Send(data.player, data.player.dataToSend, SendData.SendType.ReplyAllButSender);
                    
                    break;
                
                case DataUpdateType.JoiningDataReply:

                    JoinLeaveData joinLeaveDataa = (JoinLeaveData) data.deserializedData;
                    if (joinLeaveDataa.errorCode != -1)
                    {
                        _playerIDs.Insert(0, data.player.PlayerID);
                    }
                
                    break;
            }
            
            server.receivedDataToHandle.Remove(server.receivedDataToHandle[0]);
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(2);
        SpawnPlayers();
    }
    
    public List<GameObject> ObjectsInScene = new List<GameObject>();
    [SerializeField] private GameObject playerPrefab;
    
    async void SpawnPlayers()
    {
        // playerObjects = new GameObject[ExternServer.ConnectedPlayers.Count];
        for (int i = 0; i < ExternServer.ConnectedPlayers.Count; i++)
        {
            Vector3 tempPos = new Vector3(i * 2, 0, 0);
            GameObject temp = Instantiate(playerPrefab, tempPos, Quaternion.identity);
            ObjectsInScene.Add(temp); 
            
            OwnedObject ownedObject =  RegisterObject(ExternServer.ConnectedPlayers[i], ObjectsInScene[i]);
            ownedObject.startPos = new Pos
            {
                _posX = tempPos.x,
                _posY = tempPos.y,
                _posZ = tempPos.z
            };
            ownedObject.playerID = ExternServer.ConnectedPlayers[i].PlayerID;
            
            // Send connected players,their objects to spawn
            ExternServer.ConnectedPlayers[i].dataUpdateType = DataUpdateType.OwnedObject;
            ExternServer.ConnectedPlayers[i].dataToSend = ExternServer.ConnectedPlayers[i].ObjectToByteArray(ownedObject);
            await SendData.Send(ExternServer.ConnectedPlayers[i], ExternServer.ConnectedPlayers[i].dataToSend,
                SendData.SendType.ReplyAll);
        }
    }

    private OwnedObject RegisterObject(Player player, GameObject objToOwn)
    {
        OwnedObject ownedObject = new OwnedObject
        {
            objectType = (int)ObjectType.Player
        };

        player.playerOwnedObjects.Add(ownedObject);

        return ownedObject;
    }
    
    private void OnApplicationQuit()
    {
        // AsynchronousSocketListener.ProcessDataServer -= ProcessDataServer;
    }
}