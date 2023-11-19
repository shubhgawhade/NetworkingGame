// Asynchronous Server Socket Example
// http://msdn.microsoft.com/en-us/library/fx6588te.aspx

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using UnityEngine;

public class AsynchronousSocketListener
{
    private Socket listener;

    private List<Player> playersConnected;
    
    // Thread signal.
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    private void StartListening() 
    {
        
        playersConnected = new();

        // Establish the local endpoint for the socket.
        // The DNS name of the computer
        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[1];
        Debug.Log(IPAddress.Parse(ipAddress.ToString()));
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 7777);

        // Create a TCP/IP socket.
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

        // Bind the socket to the local endpoint and listen for incoming connections.
        try 
        {
            listener.Bind(localEndPoint);
            listener.Listen(-1);

            while (true) 
            {
                // Set the event to nonsignaled state.
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.
                Debug.Log("Waiting for a connection...");
                listener.BeginAccept(new AsyncCallback(AddConnection), listener);

                // Wait until a connection is made before continuing.
                allDone.WaitOne();
            }

        }
        catch (Exception e) 
        {
            Debug.Log(e.ToString());
        }

        Debug.Log("END");
        
    }
    
    private void AddConnection(IAsyncResult ar)
    {
        // Signal the main thread to continue.
        allDone.Set();
        // Get the socket that handles the client request.
        Socket listener = (Socket) ar.AsyncState;
        Socket handler = listener.EndAccept(ar);
        Player state = new Player();

        state.workSocket = handler;
        // Debug.Log(IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).AddressFamily.ToString()));
        playersConnected.Add(state);
        ExternServer.ConnectedPlayers = playersConnected;

        handler.BeginReceive( state.dataRecd, 0, 0, 0,
            new AsyncCallback(CheckForDataLength), state);
    }

    string sizeRecd = "";
    private void CheckForDataLength(IAsyncResult ar) 
    {
        // Retrieve the state object and the handler socket
        // from the asynchronous state object.
        // StateObject state = (StateObject) ar.AsyncState;

        Player state = (Player) ar.AsyncState;
        Socket handler = state.workSocket;
        
        int bytesRead = handler.EndReceive(ar);

        // if (bytesRead == 0)
        // {
        //     QuitClient(handler, state);
        //     return;
        // }

        // Debug.Log(System.Text.Encoding.ASCII.GetString(state.dataRecd));
        // if (System.Text.Encoding.ASCII.GetString(state.dataRecd) != "|")
        // {
        //     sizeRecd += System.Text.Encoding.ASCII.GetString(state.dataRecd);
        // }
        // else
        // {
        //     int size = int.Parse(sizeRecd);
        //     state.dataRecd = new byte[size];
        //     Debug.Log(size);
        //     
        //     handler.BeginReceive( state.dataRecd, 0, size, 0, 
        //         new AsyncCallback(ReceiveData), state);
        // }

        // for (int i = 0; i < 4; i++)
        {
            bytesRead = handler.Receive(state.dataRecd, 0);
            if (bytesRead == 0)
            {
                QuitClient(handler, state);
                return;
            }
            
        }
        
        Debug.Log(bytesRead);
        byte[] sizeVal = new byte[bytesRead - 1];
        byte[] updateType = new byte[1];
        updateType[0] = state.dataRecd[bytesRead - 1];
        // state.updateVal = ;
        state.dataUpdateType = (DataUpdateType)int.Parse(System.Text.Encoding.ASCII.GetString(updateType));
        for (int i = 0; i < bytesRead - 1; i++)
        {
           sizeVal[i] = state.dataRecd[i];
        }
        int size = int.Parse(System.Text.Encoding.ASCII.GetString(sizeVal));
        state.dataRecd = new byte[size];
        // Debug.Log(state.updateVal);
        Debug.Log(size);
        
        handler.BeginReceive( state.dataRecd, 0, size, 0, 
            new AsyncCallback(ReceiveData), state);
        
    }

    private void ReceiveData(IAsyncResult ar)
    {
        string content;
        
        Player state = (Player) ar.AsyncState;
        Socket handler = state.workSocket;
        byte[] aa = state.dataRecd;
        // Data data = ByteArrayToObject(state.dataRecd);
        // state.data = data;
        ExternServer.ConnectedPlayers = playersConnected;
        
        // Read data from the client socket. 
        int bytesRead = handler.EndReceive(ar);
        

        if (bytesRead == 0)
        {
            QuitClient(handler, state);
            return;
        }
        
        if (bytesRead > 0) 
        {
            // HandleData.Handle(state, data, bytesRead);
            switch (state.dataUpdateType)
            {
                case DataUpdateType.Joining:
                    
                    // state.playerName = data.playerName;
                    // Debug.Log($"{state.playerName} CONNECTED!");
                    JoiningData joiningData = (JoiningData)state.ByteArrayToObject(aa);
                    if (state.playerName == null)
                    {
                        // Debug.Log("HAS JOINING DATA");
                        state.playerName = joiningData.playerName;
                        Debug.Log($"{state.playerName} CONNECTED!");
                        // playersConnected.Add(state);
                    }
                    ReplyAll(state, state.dataRecd);
            
                    break;
                
                case DataUpdateType.Transform:

                    TransformData transformData = (TransformData)state.ByteArrayToObject(aa);
                    content = $"{transformData.pos._posX}, {transformData.pos._posY}, {transformData.pos._posZ}";
                    Debug.Log($"{state.playerName} : {content}");
                    ReplyAll(state, state.dataRecd);
                    
                    break;
            }
            
            // if (state.playerName == null)
            // {
            //     // Debug.Log("HAS JOINING DATA");
            //     state.playerName = data.playerName;
            //     Debug.Log($"{state.playerName} CONNECTED!");
            //     // playersConnected.Add(state);
            // }
            
            
            // state.sb = new StringBuilder();
            // There  might be more data, so store the data received so far.
            // state.sb.Append(Encoding.ASCII.GetString(state.dataRecd, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read 
            // more data.
            
            // content = $"{state.data.pos._posX}, {state.data.pos._posY}, {state.data.pos._posZ}";
            // Debug.Log($"{state.playerName} : {content}");
            state.dataRecd = new byte[sizeof(int)];
            handler.BeginReceive(state.dataRecd, 0, 0, 0, 
                new AsyncCallback(CheckForDataLength), state);
            // state.sb = new StringBuilder();
            // Send(handler, content);
            // if (content.IndexOf("<EOF>") > -1) {
            //     // All the data has been read from the 
            //     // client. Display it on the console.
            //     Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
            //         content.Length, content );
            //     // Echo the data back to the client.
            //     Send(handler, content);
            // } else {
            //     // Not all data received. Get more.
            //     handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            //     new AsyncCallback(ReadCallback), state);
            // }
        }
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
            memoryStream.Close();
            // Return stream as byte array
            return memoryStream.ToArray();                              
        }
        catch (Exception e)
        {
            Debug.Log(e);
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
        
        // BinaryFormatter bf = new BinaryFormatter();
        // using(MemoryStream ms = new MemoryStream(arrBytes))
        // {
        //     Data obj = (Data) bf.Deserialize(ms);
        //     return obj;
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
            Debug.Log(e);
            throw;
        }
    }

    // public static Action ReplyAction;
    
    public async void ReplyAll(Player state, byte[] dataRecd)
    {
        foreach (Player player in playersConnected)
        {
            if (player.playerName != state.playerName)
            {
                // Debug.Log($"Sending data to {player.playerName}");
                Socket handler = player.workSocket;
                byte[] byteData = dataRecd; //ObjectToByteArray(state.data);
                // byte[] sizeOfMsg = new byte[sizeof(int)];
                byte[] sizeOfMsg = System.Text.Encoding.ASCII.GetBytes(byteData.Length.ToString() + (int)state.dataUpdateType);
                // Debug.Log(System.Text.Encoding.ASCII.GetString(sizeOfMsg));

                try
                {
                    await handler.SendAsync(sizeOfMsg, 0);
                    await handler.SendAsync(byteData, 0);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    throw;
                }

                Debug.Log($"Sent {player.playerName} {System.Text.Encoding.ASCII.GetString(sizeOfMsg)} bytes");
                    
                // handler.BeginSend(sizeOfMsg, 0, sizeOfMsg.Length, 0,
                //     new AsyncCallback(SendDataInfo), player);
                // try
                // {
                //     // handler.SendAsync(sizeOfMsg, 0);
                // }
                // catch (Exception e)
                // {
                //     Console.WriteLine(e);
                //     throw;
                // }
                
                // return;
                // handler.BeginSend(byteData, 0, byteData.Length, 0,
                //     new AsyncCallback(SendCallback), player);
            }
        }
    }
    
    private void Send(Socket handler, String data) {
        // Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.
        // handler.BeginSend(byteData, 0, byteData.Length, 0,
        //     new AsyncCallback(SendCallback), handler);
    }

    // private void SendDataInfo(IAsyncResult ar)
    // {
    //     try {
    //         Player state = (Player) ar.AsyncState;
    //         // Retrieve the socket from the state object.
    //         Socket handler = state.workSocket;
    //
    //         // Complete sending the data to the remote device.
    //         int bytesSent = handler.EndSend(ar);
    //         Debug.Log($"Sent {bytesSent} bytes to {state.playerName}.");
    //
    //         byte[] byteData = ObjectToByteArray(state.data);
    //         handler.BeginSend(byteData, 0, byteData.Length, 0,
    //             new AsyncCallback(SendCallback), state);
    //
    //     } catch (Exception e) {
    //         Debug.Log(e.ToString());
    //     }
    // }
    
    private void SendCallback(IAsyncResult ar) {
        try {
            Player state = (Player) ar.AsyncState;
            // Retrieve the socket from the state object.
            Socket handler = state.workSocket;

            // Complete sending the data to the remote device.
            int bytesSent = handler.EndSend(ar);
            Debug.Log($"Sent {bytesSent} bytes to {state.playerName}.");

            // handler.Shutdown(SocketShutdown.Both);
            // handler.Close();

        } catch (Exception e) {
            Debug.Log(e.ToString());
        }
    }

    private void QuitClient(Socket handler, Player state)
    {
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
        Debug.Log($"{state.playerName} DISCONNECTED!");
        playersConnected.Remove(state);
    }

    private void ShutDownServer()
    {
        foreach (Player player in playersConnected)
        {
            try
            {
                player.workSocket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                player.workSocket.Close();
            }
        }
        
        listener.Close();
        
        // try
        // {
        //     listener.Shutdown(SocketShutdown.Send);
        // }
        // finally
        // {
        //     asynchronousSocketListener = null;
        // }
    }
    
    static AsynchronousSocketListener asynchronousSocketListener = new AsynchronousSocketListener();

    public static void SD()
    {
        if (asynchronousSocketListener != null)
        {
            asynchronousSocketListener.ShutDownServer();
        }
    }
    
    public static void Main()
    {
        if (asynchronousSocketListener != null)
        {
            asynchronousSocketListener.StartListening();
        }
        
        // StartListening();
        // return 0;
    }
}