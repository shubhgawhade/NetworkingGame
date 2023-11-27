using TMPro;
using UnityEngine;
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
        ClientNew.ClientStatusAction += UIStatus;
        
        nameInput.text = Random.Range(1000, 9999).ToString();
        _playerName = nameInput.text;
        _ipAddress = ipAddressInput.text;
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

    void UIStatus(int clientStatus)
    {
        print($"CLIENT STATUS : {clientStatus}");
        switch (clientStatus)
        {
            case 0:

                joinLobbyCanvas.SetActive(true);
                lobbyCanvas.SetActive(false);
                
                break;
            
            case 1:
                
                lobbyCanvas.SetActive(true);
                joinLobbyCanvas.SetActive(false);
                
                break;
            
            case 3:
                
                print("33333");
                joinLobbyCanvas.SetActive(true);
                lobbyCanvas.SetActive(false);
                
                break;
        }
    }

    private void OnApplicationQuit()
    {
        ClientNew.ClientStatusAction -= UIStatus;
    }
}
