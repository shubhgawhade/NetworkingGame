using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class ServerGameManager : MonoBehaviour
{
    public List<GameObject> ObjectsInScene = new List<GameObject>();
    [SerializeField] private GameObject playerPrefab;
    
    private HandleDataServer _handleDataServer;

    private GameObject serverCam;
    private Vector3 serverCamPos;
    
    private void Awake()
    {
        serverCamPos = new Vector3(0, 2.25f, -10);
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
                    StartCoroutine(CreateServerCamera());
                    
                    break;
            }
        }
    }

    IEnumerator CreateServerCamera()
    {
        yield return new WaitForSeconds(0.001f);
        serverCam = new GameObject
        {
            name = "SererCamera",
            transform = { position = serverCamPos}
        };
        serverCam.AddComponent<Camera>();
    }
}