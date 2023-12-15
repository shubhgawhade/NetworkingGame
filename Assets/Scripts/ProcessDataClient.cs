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

    private bool updateOtherPos;
    private TransformData otherPayersPos;
    
    // Update is called once per frame
    void Update()
    {
        while (client.receivedDataToHandle.Count > 0)
        {
            DataToHandle data = client.receivedDataToHandle.Dequeue();
            
            switch (data.dataUpdateType)
            {
                case DataUpdateType.Ready:

                    ReadyStatus readyStatus = (ReadyStatus) data.deserializedData;
                    foreach (Player player in client.playersConnected)
                    {
                        if (player.PlayerID == readyStatus.playerID)
                        {
                            player.ready = readyStatus.ready;
                        }
                        print($"{player.PlayerID} {player.ready}");
                    }
                
                    break;
                
                case DataUpdateType.JoiningDataReply:
    
                    JoinLeaveData joinLeaveData = (JoinLeaveData) data.deserializedData;

                    if (joinLeaveData.errorCode == -1)
                    {
                        print("SERVER HAS MAX PLAYERS");
                        break;
                    }

                    if (client.tickSynced)
                    {
                        // print("SETTING");
                        // float a = joinLeaveData.tick + client.pingTimer*15;

                        // if (client.pingTimer > 1.0f/30) client.pingTimer /= 2;

                        if (client.pingTimer >= 1.0f/30)
                        {
                            // float ping = client.pingTimer / 2;
                            // float setTimer = (ping + joinLeaveData.subTick) % 1.0f/30;
                            int a = (int)Mathf.Floor((client.pingTimer / 4 + joinLeaveData.subTick) / (1.0f/30));
                            client.networkTimer = new NetworkTimer(30, joinLeaveData.tick + a, 
                                client.pingTimer / 2 + joinLeaveData.subTick);
                        }
                        else
                        {
                            int a = (int)Mathf.Floor((client.pingTimer + joinLeaveData.subTick) / (1.0f/30));
                            client.networkTimer = new NetworkTimer(30, joinLeaveData.tick + a, 
                                joinLeaveData.subTick + client.pingTimer/2);
                        }
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
                                client.clientStatus = ClientNew.ClientStatus.Connected;
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
                                foreach (GameObject obj in client.objectsInScene)
                                {
                                    if (obj.GetComponent<OnlinePlayerController>().id == player.PlayerID)
                                    {
                                        Destroy(obj);
                                        client.objectsInScene.Remove(obj);
                                        break;
                                    }
                                }
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

                            bool exists = false;
                            foreach (OwnedObject localPlayerPlayerOwnedObject in client.localPlayer.playerOwnedObjects)
                            {
                                if (localPlayerPlayerOwnedObject.playerID == ownedObject.playerID)
                                {
                                    exists = true;
                                }
                            }

                            if (!exists)
                            {
                                Vector3 tempPos = new Vector3(ownedObject.startPos._posX, ownedObject.startPos._posY,
                                    ownedObject.startPos._posZ);
                                
                                GameObject temp = Instantiate(playerPrefab, tempPos, Quaternion.identity);
                                client.objectsInScene.Add(temp);

                                        
                                temp.GetComponent<OnlinePlayerController>().id = ownedObject.playerID;
                                
                                if (client.localPlayer.PlayerID == ownedObject.playerID)
                                {
                                    client.localPlayer.playerOwnedObjects.Add(ownedObject);
                                }
                            }
                        
                            break;
                    
                        // case ObjectType.Bullet
                    }

                    
                
                    break;
                
                case DataUpdateType.Transform:

                    TransformData transformData = (TransformData) data.deserializedData;
                    string content = $"{transformData.pos._posX}, {transformData.pos._posY}, {transformData.pos._posZ}";

                    foreach (GameObject o in client.objectsInScene)
                    {
                        if (o.GetComponent<OnlinePlayerController>().id == transformData.playerID)
                        {
                            if (!o.GetComponent<OnlinePlayerController>().ShouldReconcile(transformData))
                            {
                                updateOtherPos = true;
                                otherPayersPos = transformData;
                                // Vector3 tempPos = new Vector3(transformData.pos._posX, transformData.pos._posY,
                                //     transformData.pos._posZ);
                                //
                                // while ((tempPos - o.transform.position).magnitude > 0.3f)
                                // {
                                //     o.transform.position = Vector3.Lerp(o.transform.position,
                                //         tempPos, client.networkTimer.MinTimeBetweenTicks);
                                // }
                            }
                            else
                            {
                                updateOtherPos = true;
                            }
                            // o.transform.position = tempPos;
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
        }

        if (updateOtherPos && otherPayersPos != null)
        {
            foreach (GameObject o in client.objectsInScene)
            {
                if (o.GetComponent<OnlinePlayerController>().id == otherPayersPos.playerID)
                {
                    Vector3 tempPos = new Vector3(otherPayersPos.pos._posX, otherPayersPos.pos._posY,
                        otherPayersPos.pos._posZ);
                    // while ((tempPos - o.transform.position).magnitude > 0.3f)
                    {
                        o.transform.position = Vector3.Lerp(o.transform.position,
                            tempPos, ClientGameManager.client.networkTimer.MinTimeBetweenTicks);
                    }
                }
            }
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
