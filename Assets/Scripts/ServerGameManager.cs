using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class ServerGameManager : MonoBehaviour
{
    public List<GameObject> ObjectsInScene = new List<GameObject>();
    [SerializeField] private GameObject playerPrefab;
    
    private HandleDataServer _handleDataServer;
    
    private void Awake()
    {
        _handleDataServer = GetComponent<HandleDataServer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            GameObject temp = Instantiate(playerPrefab, new Vector3(5, 0, 0), Quaternion.identity);
            RegisterObject(ExternServer.ConnectedPlayers[0], temp);
        }
        
        if (_handleDataServer.gameState.stateChanged)
        {
            _handleDataServer.gameState.stateChanged = false;
            
            switch (_handleDataServer.gameState.gameState)
            {
                case GameState.gameStateEnum.Lobby:
                    
                    break;
                
                case GameState.gameStateEnum.Game:

                    SceneManager.LoadScene(1);
                    // SpawnPlayers();
                    
                    break;
            }
        }
    }
    
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
}