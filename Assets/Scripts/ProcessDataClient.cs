using System;
using UnityEngine;

public class ProcessDataClient : MonoBehaviour
{
    private ClientNew player;
    
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        player = GetComponent<ClientNew>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player.receivedData)
        {
            switch (player.ServerPlayer.dataUpdateType)
            {
                case DataUpdateType.OwnedObject:
                    
                    OwnedObject ownedObject = (OwnedObject)player.ServerPlayer.ByteArrayToObject(player.localPlayer.dataRecd);

                    switch ((ObjectType)ownedObject.objectType)
                    {
                        case ObjectType.Player:

                            Vector3 tempPos = new Vector3(ownedObject.startPos._posX, ownedObject.startPos._posY,
                                ownedObject.startPos._posZ);
                            
                            GameObject temp = Instantiate(playerPrefab, tempPos, Quaternion.identity);
                            player.objectsInScene.Add(temp);

                                    
                            temp.GetComponent<OnlinePlayerController>().id = ownedObject.playerID;
                        
                            break;
                    
                        // case ObjectType.Bullet
                    }
                    
                    break;
                
                case DataUpdateType.Transform:
                    
                    TransformData transformData = (TransformData)player.ServerPlayer.ByteArrayToObject(player.localPlayer.dataRecd);

                    foreach (GameObject o in player.objectsInScene)
                    {
                        if (o.GetComponent<OnlinePlayerController>().id == transformData.playerID)
                        {
                            Vector3 tempPos = new Vector3(transformData.pos._posX, transformData.pos._posY,
                                transformData.pos._posZ);
                            o.transform.position = tempPos;
                        }
                    }
                    
                    break;
            }
            
            player.localPlayer.dataRecd = new byte[4];
            player.receivedData = false;
        }
    }
}
