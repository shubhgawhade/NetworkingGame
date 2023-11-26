using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class ClientNew : MonoBehaviour
{
    private Socket _clientSocket;
    static readonly Int32 Port = 7777;
    private static readonly IPAddress ServerAddress = IPAddress.Parse("192.168.0.118");
    // private static readonly IPAddress ServerAddress = IPAddress.Parse("127.0.0.1");
    
    // is NULL till the client joins the server(client -> player)
    public Player localPlayer;

    public ClientStatus clientStatus;
    
    public List<Player> playersConnected = new List<Player>();
    
    public enum ClientStatus
    {
        Connecting,
        Connected,
        Disconnected
    }
    
    public float timeSinceLastSend;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Create TCP Socket
        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        
        // Call from a connect button
        ConnectToServer();
    }
    
    // private Byte[] bytes = new Byte[4];
    public Player ServerPlayer;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            localPlayer.dataUpdateType = DataUpdateType.Ready;
            ReadyStatus readyStatus = (ReadyStatus)localPlayer.returnDataStruct;
            readyStatus.ready = true;
            localPlayer.dataToSend = localPlayer.ObjectToByteArray(readyStatus);
            SendData.Send(localPlayer, localPlayer.dataToSend, SendData.SendType.ReplyOne);
        }
        
        float horizontalAxis = Input.GetAxis("Horizontal");
        if (horizontalAxis != 0)
        {
            transform.position += new Vector3(horizontalAxis, 0, 0);

            if (timeSinceLastSend > 0.5f)
            {
                localPlayer.dataUpdateType = DataUpdateType.Transform;
                
                TransformData transformData = (TransformData)localPlayer.returnDataStruct;
                transformData.pos = new TransformData.Pos
                {
                    _posX = transform.position.x,
                    _posY = transform.position.y,
                    _posZ = transform.position.z
                };

                
                localPlayer.dataToSend = localPlayer.ObjectToByteArray(transformData); 
                print($"{localPlayer.dataToSend.Length} {localPlayer.dataUpdateType}");
                // print(bytes.Length);

                // Send(sizeOfMsg, bytes);
                SendData.Send(localPlayer, localPlayer.dataToSend, SendData.SendType.ReplyOne);
                // _clientSocket.BeginSend()
                timeSinceLastSend = 0;
            }
        }
        
        timeSinceLastSend += Time.deltaTime;
    }
    
    private async void ConnectToServer()
    {
        // IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        // IPAddress ipAddress = ipHostInfo.AddressList[1];
        // Debug.Log(IPAddress.Parse(ipAddress.ToString()));
        
        // Create the connection endpoint and connect
        try
        {
            IPEndPoint localEndPoint = new IPEndPoint(ServerAddress, Port);
            await _clientSocket.ConnectAsync(localEndPoint);
            
            // Set the client status to be connected
            clientStatus = ClientStatus.Connecting;
            
            Debug.Log("NEW PLAYER CREATED");
            localPlayer = new Player
            {
                dataUpdateType = DataUpdateType.Joining,
                // data = new Data
                // {
                //     playerName = Random.Range(0, 100).ToString()
                // }
            };
                
            // player.playerName = player.data.playerName;
            JoiningData joiningData = (JoiningData)localPlayer.returnDataStruct;
            joiningData.playerName = Random.Range(0, 100).ToString();
            int randomID = Random.Range(1000, 9999);
            joiningData.playerID = randomID;
            // gameObject.name = joiningData.playerName;
            localPlayer.playerName = joiningData.playerName;
            // ServerPlayer.playerName = joiningData.playerName;
            // playersConnected.Add(localPlayer);
            localPlayer.workSocket = _clientSocket;
            // print(joiningData.playerName);

            ServerPlayer.dataToSend = localPlayer.ObjectToByteArray(joiningData);
            // bytes = ObjectToByteArray(player.data);
            byte[] sizeOfMsg = new byte[sizeof(int)];
            sizeOfMsg = System.Text.Encoding.ASCII.GetBytes(ServerPlayer.dataToSend.Length.ToString() + (int)localPlayer.dataUpdateType);
            print($"{ServerPlayer.dataToSend.Length} {localPlayer.dataUpdateType}");

            // Send(sizeOfMsg, bytes);
            await SendData.Send(localPlayer, ServerPlayer.dataToSend, SendData.SendType.ReplyOne);
            ServerPlayer.dataToSend = new byte[sizeof(int)];

            // try
            // {
            //     await _clientSocket.SendAsync(sizeOfMsg, 0);
            //     await _clientSocket.SendAsync(bytes, 0);
            // }
            // catch (Exception e)
            // {
            //     print(e);
            //     throw;
            // }

            // ServerPlayer = new Player();
            _clientSocket.BeginReceive( ServerPlayer.dataRecd, 0, 4, 0, 
                new AsyncCallback(CheckForDataLength), ServerPlayer);
        }
        catch (Exception e)
        {
            print(e);
            throw;
        }
    }

    private void CheckForDataLength(IAsyncResult ar)
    {
        Player serverPlayer = (Player) ar.AsyncState;
        
        int bytesRead = _clientSocket.EndReceive(ar);

        if (bytesRead == 0)
        {
            print("DISCONNECTED");
            _clientSocket.Close();
            // QuitClient(handler, state);
            return;
        }

        Debug.Log($"{bytesRead} Bytes Received");
        byte[] sizeVal = new byte[bytesRead - 1];
        byte[] updateType = new byte[1];
        updateType[0] = serverPlayer.dataRecd[bytesRead - 1];
        // state.updateVal = ;
        serverPlayer.dataUpdateType = (DataUpdateType)int.Parse(System.Text.Encoding.ASCII.GetString(updateType));
        for (int i = 0; i < bytesRead - 1; i++)
        {
            sizeVal[i] = serverPlayer.dataRecd[i];
        }
        
        // if (bytesRead > 0)
        {
            int size = int.Parse(System.Text.Encoding.ASCII.GetString(sizeVal));
            serverPlayer.dataRecd = new byte[size];
            // print(size);
            
            try
            {
                _clientSocket.BeginReceive( serverPlayer.dataRecd, 0, size, 0, 
                    new AsyncCallback(ReceiveData), serverPlayer);
            }
            catch (Exception e)
            {
                print(e);
                throw;
            }
        }
        
    }

    // public static Action<Player, byte[]> HandleData;

    private void ReceiveData(IAsyncResult ar)
    {
        Player serverPlayer = (Player) ar.AsyncState;
        print(serverPlayer.dataRecd.Length);
        
        int bytesRead = _clientSocket.EndReceive(ar);
        
        // print($"{bytesRead} converted");
        // print(player.data.playerName);

        // Read data from the client socket. 

        if (bytesRead == 0)
        {
            print("DISCONNECTED");
            _clientSocket.Close();
            // QuitClient(handler, state);
            return;
        }

        if (bytesRead > 0)
        { 
            // GetComponent<HandleData>().Handle(serverPlayer, serverPlayer.dataRecd);
            HandleDataPlayer(serverPlayer, serverPlayer.dataRecd);
            // HandleData(serverPlayer, serverPlayer.dataRecd);
            
            // serverPlayer = new Player();
            serverPlayer.dataRecd = new byte[sizeof(int)];
            _clientSocket.BeginReceive( serverPlayer.dataRecd, 0, 4, 0, 
                new AsyncCallback(CheckForDataLength), serverPlayer);
        }

        // player.dataRecd = new byte[sizeof(int)];
    }
    
    private void HandleDataPlayer(Player state, byte[] data)
    {
        switch (state.dataUpdateType)
        {
            case DataUpdateType.Ready:

                ReadyStatus readyStatus = (ReadyStatus)state.ByteArrayToObject(data);
                foreach (Player player in playersConnected)
                {
                    print($"{player.PlayerID} {readyStatus.ready}");
                    if (player.PlayerID == readyStatus.playerID)
                    {
                        player.ready = readyStatus.ready;
                    }
                }
                
                break;
                
            case DataUpdateType.JoiningDataReply:

                JoinLeaveData joinLeaveData = (JoinLeaveData)state.ByteArrayToObject(data);
                // print(joiningDataReply.playersConnected);
                
                // playersConnected = new List<Player>();

                // New player joined Server
                if (playersConnected.Count <= joinLeaveData.playersConnected.Length)
                {
                    for (int i = 0; i < joinLeaveData.playersConnected.Length; i++)
                    {
                        if (i == 0)
                        {
                            localPlayer.PlayerID = joinLeaveData.playerIDs[i];
                        }
                        
                        if (i < playersConnected.Count)
                        {
                            playersConnected[i].playerName = joinLeaveData.playersConnected[i];
                            playersConnected[i].PlayerID = joinLeaveData.playerIDs[i];
                            playersConnected[i].ready = joinLeaveData.ready[i];
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
                            playersConnected.Add(player);
                            Debug.Log($"{player.playerName} CONNECTED!");
                        }
                    }
                }
                // Player left Server
                else if (playersConnected.Count > joinLeaveData.playersConnected.Length)
                {
                    foreach (Player player in playersConnected)
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
                            playersConnected.Remove(player);
                            break;
                        }
                    }
                }
                
                break;
            
            case DataUpdateType.Transform:

                TransformData transformData = (TransformData)state.ByteArrayToObject(data);
                string content = $"{transformData.pos._posX}, {transformData.pos._posY}, {transformData.pos._posZ}";

                foreach (Player player in playersConnected)
                {
                    if (player.PlayerID == transformData.playerID)
                    {
                        Debug.Log($"{player.playerName} : {content}");
                    }
                }
                
                break;
        }
    }
    
    private void OnApplicationQuit()
    {
        try
        {
            _clientSocket.Shutdown(SocketShutdown.Both);
        }
        finally
        {
            _clientSocket.Close();
        }
    }
}