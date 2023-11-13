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
    
    public float writeTimer;
    public float readTimer;
    
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
                Console.WriteLine(e);
                throw;
            }
            
            // try
            // {
            //     byte[] aa = new byte[sizeof(int)];
            //     await _clientSocket.ReceiveAsync(aa, 0);
            //     
            //     // if (player.dataRecd.Length > 0)
            //     {
            //         string sizeOfMsgS = System.Text.Encoding.ASCII.GetString(aa);
            //         print(sizeOfMsgS);
            //     
            //         int size = int.Parse(System.Text.Encoding.ASCII.GetString(aa));
            //         player.dataRecd = new byte[size];
            //     
            //         await _clientSocket.ReceiveAsync(player.dataRecd, 0);
            //         player.data = ByteArrayToObject(player.dataRecd);
            //         print(player.data.playerName);
            //     }
            // }
            // catch (Exception e)
            // {
            //     Console.WriteLine(e);
            //     throw;
            // }

            ServerPlayer = new Player();
            _clientSocket.BeginReceive( ServerPlayer.dataRecd, 0, sizeof(int), 0, 
                new AsyncCallback(CheckForDataLength), ServerPlayer);
            
            // _clientSocket.BeginSend(player.dataRecd, 0, sizeof(int), 0, 
            //     SendJoiningData, player);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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

            if (writeTimer > 0.3f)
            {
                // _clientSocket.BeginSend()
                writeTimer = 0;
            }
        }
        
        writeTimer += Time.deltaTime;
        readTimer += Time.deltaTime;
        if (readTimer > 0.3f)
        {
            // try
            // {
            //     await _clientSocket.ReceiveAsync(player.dataRecd, 0);
            //
            //     if (player.dataRecd.Length > 0)
            //     {
            //         string sizeOfMsg = System.Text.Encoding.ASCII.GetString(player.dataRecd);
            //         print(sizeOfMsg);
            //     
            //         int size = int.Parse(System.Text.Encoding.ASCII.GetString(player.dataRecd));
            //         player.dataRecd = new byte[size];
            //
            //         await _clientSocket.ReceiveAsync(player.dataRecd, 0);
            //         player.data = ByteArrayToObject(player.dataRecd);
            //         print(player.data.playerName);
            //     }
            // }
            // catch (Exception e)
            // {
            //     Console.WriteLine(e);
            //     throw;
            // }
            
            // _clientSocket.BeginReceive( player.dataRecd, 0, sizeof(int), 0,
            //     new AsyncCallback(CheckForDataLength), player);
            
            readTimer = 0;
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

        if (bytesRead > 0)
        {
            int size = int.Parse(System.Text.Encoding.ASCII.GetString(serverPlayer.dataRecd));
            serverPlayer.dataRecd = new byte[size];
            print(size);
            _clientSocket.BeginReceive( serverPlayer.dataRecd, 0, size, 0, 
                new AsyncCallback(ReceiveData), serverPlayer);
        }
        
    }

    private void ReceiveData(IAsyncResult ar)
    {
        Player serverPlayer = (Player) ar.AsyncState;
        Data data = ByteArrayToObject(serverPlayer.dataRecd);
        serverPlayer.data = data;
        // print(data._dataUpdateType);
        
        int bytesRead = _clientSocket.EndReceive(ar);
        
        // print(bytesRead);
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
            ServerPlayer.dataRecd = new byte[sizeof(int)];
            _clientSocket.BeginReceive( ServerPlayer.dataRecd, 0, sizeof(int), 0, 
                new AsyncCallback(CheckForDataLength), ServerPlayer);
        }

        // player.dataRecd = new byte[sizeof(int)];
    }

    private async Task ReadMSg()
    {
        // _clientSocket.BeginReceive()
        
        await _clientSocket.ReceiveAsync(bytes, 0);
        
        int s = int.Parse(System.Text.Encoding.ASCII.GetString(bytes));
        print(s);
        player.dataRecd = new byte[s];
        
        // await _clientSocket.ReceiveAsync(player.dataRecd, 0);
    }

    public byte[] ObjectToByteArray(Data obj)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (var ms = new MemoryStream())
        {
            binaryFormatter.Serialize(ms, obj);
            return ms.ToArray();
        }
    }
    
    private Data ByteArrayToObject(byte[] arrBytes)
    {
        MemoryStream memStream = new MemoryStream();
        BinaryFormatter binForm = new BinaryFormatter();
        memStream.Write(arrBytes, 0, arrBytes.Length);
        memStream.Seek(0, SeekOrigin.Begin);
        Data obj = (Data) binForm.Deserialize(memStream);
        return obj;
    }
    
    private void SendJoiningData(IAsyncResult ar)
    {
        
    }
    
    private void OnApplicationQuit()
    {
        _clientSocket.Close();
    }
}