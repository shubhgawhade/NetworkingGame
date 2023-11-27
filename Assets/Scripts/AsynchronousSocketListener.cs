﻿// Asynchronous Server Socket Example
// http://msdn.microsoft.com/en-us/library/fx6588te.aspx

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class AsynchronousSocketListener
{
    private Player serverPlayer;
    private Socket listener;

    private List<Player> playersConnected = new List<Player>();
    
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
        Player state = (Player) ar.AsyncState;
        Socket handler = state.workSocket;
        
        int bytesRead = handler.EndReceive(ar);
        
        bytesRead = handler.Receive(state.dataRecd, 0);
        if (bytesRead == 0)
        {
            QuitClient(handler, state, 1);
            return;
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

    // public static Action<Player, byte[]> HandleData;
    private void ReceiveData(IAsyncResult ar)
    {
        string content;
        
        Player state = (Player) ar.AsyncState;
        Socket handler = state.workSocket;
        // Data data = ByteArrayToObject(state.dataRecd);
        // state.data = data;
        ExternServer.ConnectedPlayers = playersConnected;
        
        // Read data from the client socket. 
        int bytesRead = handler.EndReceive(ar);
        

        if (bytesRead == 0)
        {
            QuitClient(handler, state, 1);
            return;
        }
        
        if (bytesRead > 0)
        {
            HandleDataServer(state, state.dataRecd);
            
            state.dataRecd = new byte[sizeof(int)];
            handler.BeginReceive(state.dataRecd, 0, 0, 0, 
                new AsyncCallback(CheckForDataLength), state);
        }
    }

    public static Action<Player, object, DataUpdateType> ProcessDataServer;
    
    private void HandleDataServer(Player state, byte[] data)
    {
        switch (state.dataUpdateType)
        {
            case DataUpdateType.Joining:
                    
                JoiningData joiningData = (JoiningData)state.ByteArrayToObject(data);
                if (state.playerName == null)
                {
                    // Debug.Log("HAS JOINING DATA");
                    // state.playerName = joiningData.playerName;
                    // state.PlayerID = joiningData.playerID;
                    // Debug.Log($"{state.playerName} CONNECTED!");

                    for (int i = 0; i < playersConnected.Count; i++)
                    {
                        // int randomID = Random.Range(1000, 9999);
                        // if (randomID != playersConnected[i].PlayerID)
                        {
                            // state.PlayerID = 1;
                        }
                    }
                    ProcessDataServer(state, joiningData, state.dataUpdateType);
                    
                    // SendData.Send(state, state.dataRecd, SendData.SendType.ReplyAllButSender);
                }
            
                break;
            
            case DataUpdateType.Ready:

                ReadyStatus readyStatus = (ReadyStatus)state.ByteArrayToObject(data);
                // readyStatus.playerID = 1;
                readyStatus.playerID = state.PlayerID;
                state.ready = readyStatus.ready;

                state.dataToSend = state.ObjectToByteArray(readyStatus);
                SendData.Send(state, state.dataToSend, SendData.SendType.ReplyAll);

                if (playersConnected.Count > 1)
                {
                    foreach (Player player in playersConnected)
                    {
                        if(!player.ready) return;
                    }
                    
                    ProcessDataServer(state, readyStatus, DataUpdateType.Ready);
                }
                
                break;
                
            case DataUpdateType.Transform:

                TransformData transformData = (TransformData)state.ByteArrayToObject(data);
                string content = $"{transformData.pos._posX}, {transformData.pos._posY}, {transformData.pos._posZ}";
                Debug.Log($"{state.playerName} : {content}");
                
                transformData.playerID = state.PlayerID;
                state.dataToSend = state.ObjectToByteArray(transformData);
                SendData.Send(state, state.dataToSend, SendData.SendType.ReplyAllButSender);
                    
                break;
        }
    }
    
    public void QuitClient(Socket handler, Player state, int errorCode)
    {
        state.dataUpdateType = DataUpdateType.JoiningDataReply;
        JoinLeaveData joinLeaveData = (JoinLeaveData) state.returnDataStruct;
        joinLeaveData.playersConnected = new string[playersConnected.Count];
        joinLeaveData.playerIDs = new int[playersConnected.Count];
        for (int i = 0; i < playersConnected.Count; i++)
        {
            joinLeaveData.playersConnected[i] = playersConnected[i].playerName;
            joinLeaveData.playerIDs[i] = playersConnected[i].PlayerID;
        }

        joinLeaveData.errorCode = errorCode;
        Debug.LogWarning($"ERROR CODE : {joinLeaveData.errorCode}");
        
        ProcessDataServer(state, joinLeaveData, DataUpdateType.JoiningDataReply);
        
        state.dataToSend = state.ObjectToByteArray(joinLeaveData);
        
        Debug.Log($"{state.dataToSend.Length} {state.dataUpdateType}");
        SendData.Send(state, state.dataToSend, SendData.SendType.ReplyAll);

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
    
    public static readonly AsynchronousSocketListener asynchronousSocketListener = new AsynchronousSocketListener();

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