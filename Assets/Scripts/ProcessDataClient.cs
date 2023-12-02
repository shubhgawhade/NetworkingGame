using System;
using UnityEngine;

public class ProcessDataClient : MonoBehaviour
{
    private ClientNew client;
    
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        client = GetComponent<ClientNew>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (client.receivedDataToHandle.Count > 0)
        {
            ClientNew.DataToHandle data = client.receivedDataToHandle[0];
            
            switch (client.receivedDataToHandle[0].dataUpdateType)
            { 
                case DataUpdateType.Ready:

                    ReadyStatus readyStatus = (ReadyStatus) data.deserializedData;
                    foreach (Player player in client.playersConnected)
                    {
                        print($"{player.PlayerID} {readyStatus.ready}");
                        if (player.PlayerID == readyStatus.playerID)
                        {
                            player.ready = readyStatus.ready;
                        }
                    }
                
                    break;
                
                case DataUpdateType.JoiningDataReply:
    
                    JoinLeaveData joinLeaveData = (JoinLeaveData) data.deserializedData;

                    if (joinLeaveData.errorCode == -1)
                    {
                        print("SERVER HAS MAX PLAYERS");
                        break;
                    }
                    
                    // New player joined Server
                    if (client.playersConnected.Count <= joinLeaveData.playersConnected.Length)
                    {
                        for (int i = 0; i < joinLeaveData.playersConnected.Length; i++)
                        {
                            if (client.playersConnected.Count == 0)
                            {
                                print(joinLeaveData.playerIDs[i]);
                                client.localPlayer.PlayerID = joinLeaveData.playerIDs[joinLeaveData.playersConnected.Length - 1];
                            }
                            
                            if (i < client.playersConnected.Count)
                            {
                                client.playersConnected[i].playerName = joinLeaveData.playersConnected[i];
                                client.playersConnected[i].PlayerID = joinLeaveData.playerIDs[i];
                                client.playersConnected[i].ready = joinLeaveData.ready[i];
                            }
                            else
                            {
                                Player player = new Player
                                {
                                    playerName = joinLeaveData.playersConnected[i],
                                    PlayerID = joinLeaveData.playerIDs[i],
                                    ready = joinLeaveData.ready[i]
                                };

                                // print(player.PlayerID);
                                client.playersConnected.Add(player);
                                Debug.Log($"{player.playerName} CONNECTED!");
                            }
                        }
                    }
                    // Player left Server
                    else if (client.playersConnected.Count > joinLeaveData.playersConnected.Length)
                    {
                        foreach (Player player in client.playersConnected)
                        {
                            bool stillConnected = false;
                            foreach (int playerID in joinLeaveData.playerIDs)
                            {
                                if (player.PlayerID == playerID)
                                {
                                    stillConnected = true;
                                    break;
                                }
                            }

                            if (!stillConnected)
                            {
                                client.playersConnected.Remove(player);
                                break;
                            }
                        }
                    }

                    break;
                
                case DataUpdateType.OwnedObject:
                
                    client.gameState.ChangeState(GameState.gameStateEnum.Game);
                    
                    OwnedObject ownedObject = (OwnedObject) data.deserializedData;
                    print(ownedObject.objectType);
                    
                    switch ((ObjectType)ownedObject.objectType)
                    {
                        case ObjectType.Player:

                            Vector3 tempPos = new Vector3(ownedObject.startPos._posX, ownedObject.startPos._posY,
                                ownedObject.startPos._posZ);
                            
                            GameObject temp = Instantiate(playerPrefab, tempPos, Quaternion.identity);
                            client.objectsInScene.Add(temp);

                                    
                            temp.GetComponent<OnlinePlayerController>().id = ownedObject.playerID;
                        
                            break;
                    
                        // case ObjectType.Bullet
                    }

                    if (client.localPlayer.PlayerID == ownedObject.playerID)
                    {
                        client.localPlayer.playerOwnedObjects.Add(ownedObject);
                    }
                
                    break;
                
                case DataUpdateType.Transform:

                    TransformData transformData = (TransformData) data.deserializedData;
                    string content = $"{transformData.pos._posX}, {transformData.pos._posY}, {transformData.pos._posZ}";

                    foreach (GameObject o in client.objectsInScene)
                    {
                        if (o.GetComponent<OnlinePlayerController>().id == transformData.playerID)
                        {
                            Vector3 tempPos = new Vector3(transformData.pos._posX, transformData.pos._posY,
                                transformData.pos._posZ);
                            o.transform.position = tempPos;
                        }
                    }
                    
                    foreach (Player player in client.playersConnected)
                    {
                        if (player.PlayerID == transformData.playerID)
                        {
                            Debug.Log($"{player.playerName} : {content}");
                        }
                    }
                
                    break;
            }

            client.receivedDataToHandle.Remove(client.receivedDataToHandle[0]);
        }
        
        // if (player.receivedData)
        {
            // switch (client.ServerPlayer.dataUpdateType)
            // {
            //     case DataUpdateType.OwnedObject:
            //         
            //         OwnedObject ownedObject = (OwnedObject)client.ServerPlayer.ByteArrayToObject(client.localPlayer.dataRecd);
            //
            //         switch ((ObjectType)ownedObject.objectType)
            //         {
            //             case ObjectType.Player:
            //
            //                 Vector3 tempPos = new Vector3(ownedObject.startPos._posX, ownedObject.startPos._posY,
            //                     ownedObject.startPos._posZ);
            //                 
            //                 GameObject temp = Instantiate(playerPrefab, tempPos, Quaternion.identity);
            //                 client.objectsInScene.Add(temp);
            //
            //                         
            //                 temp.GetComponent<OnlinePlayerController>().id = ownedObject.playerID;
            //             
            //                 break;
            //         
            //             // case ObjectType.Bullet
            //         }
            //         
            //         break;
            //     
            //     case DataUpdateType.Transform:
            //         
            //         TransformData transformData = (TransformData)client.ServerPlayer.ByteArrayToObject(client.localPlayer.dataRecd);
            //
            //         foreach (GameObject o in client.objectsInScene)
            //         {
            //             if (o.GetComponent<OnlinePlayerController>().id == transformData.playerID)
            //             {
            //                 Vector3 tempPos = new Vector3(transformData.pos._posX, transformData.pos._posY,
            //                     transformData.pos._posZ);
            //                 o.transform.position = tempPos;
            //             }
            //         }
            //         
            //         break;
            // }
            //
            // client.localPlayer.dataRecd = new byte[4];
            // player.receivedData = false;
        }
    }
}
