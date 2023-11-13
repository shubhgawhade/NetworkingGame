using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class ClientOld : MonoBehaviour
{
    // [SerializeField] private GameObject spawnPrefab;
    private GameObject localPlayer;
    
    //static string message = "hello!";
    static Int32 port = 7777;

    // Prefer a using declaration to ensure the instance is Disposed later.
    static TcpClient client;

    // Translate the passed message into ASCII and store it as a Byte array.
    private Byte[] bytes = new Byte[256];
    private string data = "hello";

    // Get a client stream for reading and writing.
    NetworkStream stream;

    public float time;
    public float Readtime;

    public Player player;
    
    // Start is called before the first frame update
    async void Start()
    {
        await StartClient();
    }

    // private bool timeout;
    // private float time;
    private async void Update()
    {
        // time += Time.deltaTime;
        //
        // if (time > 0.5f)
        // {
        //     timeout = !timeout;
        // }
        
        if (Input.inputString != "" && Input.inputString != "\r")// && timeout)
        {
            // time = 0;
            // timeout = true;
            // if (data.Length > 10)
            {
                // data = "";
            }
            data += Input.inputString;
            // if (data != "")
        }
        
        if(Input.GetKeyDown(KeyCode.Return))
        {
            await SendMessage();
            // await ReceiveMessage();
        }


        float horizontalAxis = Input.GetAxis("Horizontal");
        if (horizontalAxis != 0)
        {
            transform.position += new Vector3(horizontalAxis, 0, 0);

            if (time > 0.3f)
            {
                SendMessage();
                // ReceiveMessage();
                time = 0;
            }
        }
        
        time += Time.deltaTime;
        Readtime += Time.deltaTime;
        if (Readtime > 0.3f)
        {
            // SendMessage();
            // await ReceiveMessage();
            // await ReadMsg();
            
            Readtime = 0;
        }
    }
    
    
    // [Serializable]
    // public class Data
    // {
    //     public enum DataUpdate
    //     {
    //         Transform,
    //         Heath
    //     }
    //
    //     public DataUpdate _dataUpdate = DataUpdate.Transform;
    //     public float _posX;
    // }

    private async Task SendMessage()
    {
        if (player.playerName == "")
        {
            Debug.Log("NEW PLAYER CREATED");
            player = new Player
            {
                data = new Data
                {
                    _dataUpdateType  = DataUpdateType.Joining,
                    playerName = Random.Range(0,100).ToString()
                    
                    // pos = new Data.Pos
                    // {
                    //     _posX = transform.position.x,
                    //     _posY = transform.position.y,
                    //     _posZ = transform.position.z
                    // }
                }
            };
            player.playerName = player.data.playerName;
        }
        else
        {

            Debug.Log($"OLD PLAYER INFO {player.playerName}");
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
        }

        // Data sendData = new Data()
        // {
        //     _dataUpdate = Data.DataUpdate.Transform,
        //     _posX = transform.position.x
        // };

        bytes = ObjectToByteArray(player.data);
        byte[] sizeOfMsg = new byte[sizeof(int)];
        sizeOfMsg = System.Text.Encoding.ASCII.GetBytes(bytes.Length.ToString());
        print(bytes.Length.ToString());

        try
        {
            await stream.WriteAsync(sizeOfMsg, 0, sizeOfMsg.Length);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        // string bytesToSend = Marshal.SizeOf(sendData) + "|";
        // data = data.Insert(0, bytesToSend);
        // bytes = new Byte[data.Length];
        // bytes = System.Text.Encoding.ASCII.GetBytes(data);
        // bytes = ObjectToByteArray(data);
        try
        {
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        // print($"Sent: {data}");
        // data = "";
        
        // string bytesToSend = data.Length + "|";
        // data = data.Insert(0, bytesToSend);
        // bytes = new Byte[data.Length];
        // bytes = System.Text.Encoding.ASCII.GetBytes(data);
        // await stream.WriteAsync(bytes, 0, bytes.Length);
        // print($"Sent: {data}");
        // data = "";
    }

    public byte[] ObjectToByteArray(Data obj)
    {
        // string bytesToSend = Marshal.SizeOf(obj) + "|";
        // print(bytesToSend);
        // data = data.Insert(0, bytesToSend);
        // bytes = new Byte[data.Length];


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

    private async Task ReadMsg()
    {
        // player.dataRecd = new byte[s];
        bytes = new byte[sizeof(int)];
        int i = await stream.ReadAsync(player.dataRecd, 0, sizeof(int));
        if (i == 0)
        {
            print($"DISCONNECTED");
            return;
        }
        int s = int.Parse(System.Text.Encoding.ASCII.GetString(player.dataRecd));
        Debug.Log(s);

        // await client.Client.ReceiveAsync(player.dataRecd, 0);
        i = await stream.ReadAsync(player.dataRecd, 0, s);
        
        if (i == 0)
        {
            print($"DISCONNECTED");
            return;
        }
        
        if (i > 0)
        {
            Data data = ByteArrayToObject(player.dataRecd);
            print($"{data._dataUpdateType} : {data.pos._posX}");
        }
    }
    
    private async Task ReceiveMessage()
    {
        // print("RECVD");
        // bytes = new byte[sizeof(int)];
        // int i = await stream.ReadAsync(player.dataRecd, 0, sizeof(int));
        // if (i == 0)
        // {
        //     print($"DISCONNECTED");
        //     return;
        // }
        //
        // int s = int.Parse(System.Text.Encoding.ASCII.GetString(player.dataRecd));
        // Debug.Log(s);

        // await ReadMsg(s);

        // bytes = new Byte[1];
        // string len = "";
        // int i;
        // for (int j = 0; j < 4; j++)
        // {
        //     i = await stream.ReadAsync(bytes, 0, 1);
        //     
        //     if (i == 0)
        //     {
        //         print($"DISCONNECTED");
        //         break;
        //     }
        //
        //     if (i > 0)
        //     {
        //         string currentByte = System.Text.Encoding.ASCII.GetString(bytes, 0, 1);
        //         if (currentByte == "|")
        //         {
        //             break;
        //         }
        //         
        //         len += currentByte;
        //     }
        //
        // }
        //
        //
        // print(len);
        //
        // bytes = new Byte[int.Parse(len)];
        // i = await stream.ReadAsync(bytes, 0, bytes.Length);
        // print(i);
        //
        // // Loop to receive all the data sent by the client.
        // // while ((i = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
        // if (i > 0)
        // {
        //     // Translate data bytes to a ASCII string.
        //     data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
        //     print($"Received: {data}");
        // }
        //
        // //  int bytesRecd = await stream.ReadAsync(bytes, 0, bytes.Length);
        // //
        // // // Translate data bytes to a ASCII string.
        // // data = System.Text.Encoding.ASCII.GetString(bytes, 0, bytesRecd);
        // // print($"Received: {data}");
        // data = "";
        //
        // // int bytesRecd = await stream.ReadAsync(bytes, 0, bytes.Length);
        // //         
        // // // Translate data bytes to a ASCII string.
        // // data = System.Text.Encoding.ASCII.GetString(bytes, 0, bytesRecd);
        // // print($"Received: {data}");
    }
    
    private async Task StartClient()
    {
        try
        {
            // Create a TcpClient.
            // Note, for this client to work you need to have a TcpServer
            // connected to the same address as specified by the server, port
            // combination.

            client = new TcpClient("192.168.0.118", port);
            
            stream = client.GetStream();
            await SendMessage();
            // await ReceiveMessage();
            
            // bytes = System.Text.Encoding.ASCII.GetBytes(data);
            //
            // await stream.WriteAsync(bytes, 0, bytes.Length);
            // print($"Sent: {data}");

            //int i;
            //while((i = await stream.ReadAsync(bytes, 0, bytes.Length))!=0) 
            {
                // int bytesRecd = await stream.ReadAsync(bytes, 0, bytes.Length);
                //
                // // Translate data bytes to a ASCII string.
                // data = System.Text.Encoding.ASCII.GetString(bytes, 0, bytesRecd);
                // print($"Received: {data}");
                
                // localPlayer = Instantiate(spawnPrefab);
            }
            
            // Send the message to the connected TcpServer.
            //this await stream.WriteAsync(data, 0, data.Length);

            //this print($"Sent: {message}");

            //this // Receive the server response.

            //this // Buffer to store the response bytes.
            //this data = new Byte[256];

            //this // String to store the response ASCII representation.
            //this String responseData = String.Empty;

            //this // Read the first batch of the TcpServer response bytes.
            //this Int32 bytes = await stream.ReadAsync(data, 0, data.Length);
            //this responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            //this print($"Received: {responseData}");

            //this localPlayer = Instantiate(spawnPrefab);

            // Explicit close is not necessary since TcpClient.Dispose() will be
            // called automatically.
            // stream.Close();
            // client.Close();
        }
        catch (ArgumentNullException e)
        {
            print($"ArgumentNullException: {e}");
        }
        catch (SocketException e)
        {
            print($"SocketException: {e}");
        }
    }

    private void OnApplicationQuit()
    {
        client.Close();
        Destroy(localPlayer);
    }
}
