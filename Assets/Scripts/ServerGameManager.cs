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
}