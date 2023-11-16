using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ClientNew : MonoBehaviour
{
    private Socket _clientSocket;
    static readonly Int32 Port = 7777;
    private static readonly IPAddress ServerAddress = IPAddress.Parse("192.168.0.118");
    
    // is NULL till the client joins the server(client -> player)
    public Player player;

    public ClientStatus clientStatus;
    
    public enum ClientStatus
    {
        Connecting,
        Connected,
        Disconnected
    }
    
    public float timeSinceLastSend;
    
    // Start is called before the first frame update
    void Start()
    {
        // Create TCP Socket
        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        
        // Call from a connect button
        ConnectToServer();
    }
    
    private Byte[] bytes = new Byte[4];
    public Player ServerPlayer;

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
            player = new Player
            {
                data = new Data
                {
                    _dataUpdateType = DataUpdateType.Joining,
                    playerName = Random.Range(0, 100).ToString()
                }
            };
                
            player.playerName = player.data.playerName;

            bytes = ObjectToByteArray(player.data);
            byte[] sizeOfMsg = new byte[sizeof(int)];
            sizeOfMsg = System.Text.Encoding.ASCII.GetBytes(bytes.Length.ToString());
            print(bytes.Length.ToString());

            try
            {
                await _clientSocket.SendAsync(sizeOfMsg, 0);
                await _clientSocket.SendAsync(bytes, 0);
            }
            catch (Exception e)
            {
                print(e);
                throw;
            }

            ServerPlayer = new Player();
            _clientSocket.BeginReceive( ServerPlayer.dataRecd, 0, sizeof(int), 0, 
                new AsyncCallback(CheckForDataLength), ServerPlayer);
        }
        catch (Exception e)
        {
            print(e);
            throw;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        float horizontalAxis = Input.GetAxis("Horizontal");
        if (horizontalAxis != 0)
        {
            transform.position += new Vector3(horizontalAxis, 0, 0);

            if (timeSinceLastSend > 0.3f)
            {
                player.data = new Data
                {
                    _dataUpdateType = DataUpdateType.Transform,
                    pos = new Data.Pos
                    {
                        _posX = transform.position.x,
                        _posY = transform.position.y,
                        _posZ = transform.position.z
                    }
                };
                
                bytes = ObjectToByteArray(player.data);
                byte[] sizeOfMsg = new byte[sizeof(int)];
                sizeOfMsg = System.Text.Encoding.ASCII.GetBytes(bytes.Length.ToString());
                print(bytes.Length.ToString());

                Send(sizeOfMsg, bytes);
                // _clientSocket.BeginSend()
                timeSinceLastSend = 0;
            }
        }
        
        timeSinceLastSend += Time.deltaTime;
    }

    private async void Send(byte[] sizeOfMsg, byte[] bytesToSend)
    {
        await _clientSocket.SendAsync(sizeOfMsg, 0);
        await _clientSocket.SendAsync(bytesToSend, 0);
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

        // if (bytesRead > 0)
        {
            int size = int.Parse(System.Text.Encoding.ASCII.GetString(serverPlayer.dataRecd));
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

    private void ReceiveData(IAsyncResult ar)
    {
        Player serverPlayer = (Player) ar.AsyncState;
        print(serverPlayer.dataRecd.Length);
        
        // READS SOMETIMES AND FAILS OTHER TIMES
        Data data = ByteArrayToObject(serverPlayer.dataRecd);
        serverPlayer.data = data;
        print(data._dataUpdateType);
        
        int bytesRead = _clientSocket.EndReceive(ar);
        
        print($"{bytesRead} converted");
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
            print(serverPlayer.data._dataUpdateType);
            
            // serverPlayer.dataRecd = new byte[sizeof(int)];
            // _clientSocket.BeginReceive( serverPlayer.dataRecd, 0, sizeof(int), 0,
            //     new AsyncCallback(CheckForDataLength), serverPlayer);
            
            ServerPlayer = new Player();
            // ServerPlayer.dataRecd = new byte[sizeof(int)];
            _clientSocket.BeginReceive( ServerPlayer.dataRecd, 0, sizeof(int), 0, 
                new AsyncCallback(CheckForDataLength), ServerPlayer);
        }

        // player.dataRecd = new byte[sizeof(int)];
    }

    public byte[] ObjectToByteArray(Data obj)
    {
        // BinaryFormatter binaryFormatter = new BinaryFormatter();
        // using (var ms = new MemoryStream())
        // {
        //     binaryFormatter.Serialize(ms, obj);
        //     return ms.ToArray();
        // }

        try
        {
            // Create new BinaryFormatter
            BinaryFormatter binaryFormatter = new BinaryFormatter();    
            // Create target memory stream
            using MemoryStream memoryStream = new MemoryStream();             
            // Serialize object to stream
            binaryFormatter.Serialize(memoryStream, obj);       
            // Return stream as byte array
            return memoryStream.ToArray();                              
        }
        catch (Exception e)
        {
            print(e);
            throw;
        }
    }
    
    private Data ByteArrayToObject(byte[] arrBytes)
    {
        // MemoryStream memStream = new MemoryStream();
        // BinaryFormatter binForm = new BinaryFormatter();
        // memStream.Write(arrBytes, 0, arrBytes.Length);
        // memStream.Seek(0, SeekOrigin.Begin);
        // Data obj = (Data) binForm.Deserialize(memStream);
        // return obj;

        // try
        // {
        //     BinaryFormatter bf = new BinaryFormatter();
        //     using(MemoryStream ms = new MemoryStream(arrBytes))
        //     {
        //         Data obj = (Data) bf.Deserialize(ms);
        //         return obj;
        //     }
        // }
        // catch (Exception e)
        // {
        //     Console.WriteLine(e);
        //     throw;
        // }

        try
        {
            // Create new BinaryFormatter
            BinaryFormatter binaryFormatter = new BinaryFormatter(); 
            // Convert buffer to memorystream
            using MemoryStream memoryStream = new MemoryStream(arrBytes);
            memoryStream.Seek(0, SeekOrigin.Begin);
            // Deserialize stream to an object
            Data data = (Data) binaryFormatter.Deserialize(memoryStream);
            return data;
        }
        catch (Exception e)
        {
            print(e);
            throw;
        }
    }
    
    private void SendJoiningData(IAsyncResult ar)
    {
        
    }
    
    private void OnApplicationQuit()
    {
        _clientSocket.Close();
    }
}