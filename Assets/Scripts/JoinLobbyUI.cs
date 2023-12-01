using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class JoinLobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject joinLobbyCanvas;
    [SerializeField] private GameObject lobbyCanvas;
    
    [SerializeField] private ClientNew clientConnectionObject;

    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField ipAddressInput;
    
    private string _playerName;
    private string _ipAddress;

    private void Awake()
    {
        nameInput.text = Random.Range(1000, 9999).ToString();
        _playerName = nameInput.text;
        _ipAddress = ipAddressInput.text;

        // joinLobbyCanvas = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        // ClientNew.ClientStatusAction += UIStatus;
    }

    public void OnConnectButtonPressed()
    {
        clientConnectionObject.ConnectToServer(_ipAddress, _playerName);
    }

    public void OnPlayerNameEditEnd()
    {
        _playerName = nameInput.text;
    }

    public void OnIpAddressEditEnd()
    {
        _ipAddress = ipAddressInput.text;
    }
    
    private void Update()
    {
        if (clientConnectionObject.gameState.stateChanged)
        {
            clientConnectionObject.gameState.stateChanged = false;
            
            switch (clientConnectionObject.gameState.gameState)
            {
                case GameState.gameStateEnum.JoinScreen:
                    
                    joinLobbyCanvas.SetActive(true);
                    lobbyCanvas.SetActive(false);
                    
                    break;
                
                case GameState.gameStateEnum.Lobby:
                    
                    lobbyCanvas.SetActive(true);
                    joinLobbyCanvas.SetActive(false);
                    
                    break;
                
                case GameState.gameStateEnum.Game:

                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                    
                    break;
            }
        }
    }

    private void OnApplicationQuit()
    {
        // ClientNew.ClientStatusAction -= UIStatus;
    }
}
