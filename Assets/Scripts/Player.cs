using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// C1: Send Connection Request with data
/// S: Read player socket and add to connected clients list
///     store socket in player object
///     Check for size of data
///     Read the data with size
///     Update player Object with Name
///     Player	                Server
//      Request Join    ->  Creates Player
// 	                    <- Players Connected to Lobby
//      Last Player Ready -> Start Game
//      Creates local player <- Player Creation info
/// </summary>

[Serializable]
public enum DataUpdateType
{
    Joining,
    JoiningDataReply,
    Ready,
    Transform,
    Heath
}

[Serializable]
public class Player
{
    // Client  socket.
    public Socket workSocket = null;

    public string playerName;
    public int PlayerID;

    public bool ready;
    
    // Receive and Send buffers
    public byte[] dataRecd = new byte[sizeof(int)];
    public byte[] dataToSend = new byte[sizeof(int)];
    
    public DataUpdateType dataUpdateType;
    
    // DataType must be created before sending
    public object returnDataStruct => ReturnDataStruct();
    
    private object ReturnDataStruct()
    {
        switch (dataUpdateType)
        {
            case DataUpdateType.Joining:

                JoiningData jd = new JoiningData();
                return jd;
                
                break;
            
            case DataUpdateType.JoiningDataReply:

                JoinLeaveData jdr = new JoinLeaveData();
                return jdr;
                
                break;
            
            case DataUpdateType.Ready:

                ReadyStatus rs = new ReadyStatus();
                return rs;
                
                break;
            
            case DataUpdateType.Transform:

                TransformData td = new TransformData();
                return td;
                
                break;
        }

        return null;
    }

    // public byte[] DataToSend => ObjectToByteArray(returnDataStruct);
    public byte[] ObjectToByteArray(object data)
    {
        try
        {
            // Create new BinaryFormatter
            BinaryFormatter binaryFormatter = new BinaryFormatter(); 
            
            // Create target memory stream
            using MemoryStream memoryStream = new MemoryStream();             
            
            // Serialize object to stream
            binaryFormatter.Serialize(memoryStream, data);       
            
            // Return stream as byte array
            return memoryStream.ToArray();                              
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    // public object DataReceived => ByteArrayToObject(DataToSend);
    public object ByteArrayToObject(byte[] data)
    {
        try
        {
            // Create new BinaryFormatter
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            
            // Convert buffer to memorystream
            using MemoryStream memoryStream = new MemoryStream(data);
            memoryStream.Seek(0, SeekOrigin.Begin);
            
            // Deserialize stream to an object
            switch (dataUpdateType)
            {
                case DataUpdateType.Joining:
                    
                    JoiningData joiningData = (JoiningData) binaryFormatter.Deserialize(memoryStream);
                    return joiningData;
                    
                    break;
                
                case DataUpdateType.JoiningDataReply:

                    JoinLeaveData joinLeaveData = (JoinLeaveData) binaryFormatter.Deserialize(memoryStream);
                    return joinLeaveData;
                    
                    break;
                
                case DataUpdateType.Ready:

                    ReadyStatus readyStatus = (ReadyStatus) binaryFormatter.Deserialize(memoryStream);
                    return readyStatus;
                    
                    break;
                
                case DataUpdateType.Transform:

                    TransformData transformData = (TransformData) binaryFormatter.Deserialize(memoryStream);
                    return transformData;
                    
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }

        return null;
    }
}

[Serializable]
public class PlayerID
{
    public int playerID;
}

[Serializable]
public class JoiningData : PlayerID
{
    public string playerName;
}

[Serializable]
public class JoinLeaveData
{
    public int errorCode;
    // CODES
    // -1 : MAX PLAYERS
    // 0 : DISCONNECTED NORMALLY
    // 1 : SERVER - PLAYER CLOSED CONNECTION
    // 2 : PLAYER - SERVER CLOSED CONNECTION
    
    
    public string[] playersConnected;
    public int[] playerIDs;
    public bool[] ready;
}

[Serializable]
public class ReadyStatus : PlayerID
{
    public bool ready;
}

[Serializable]
public class TransformData : PlayerID
{
    [Serializable]
    public class Pos
    {
        public float _posX;
        public float _posY;
        public float _posZ;
    }

    public Pos pos;
    
    [Serializable]
    public class Rot
    {
        public float _rotX;
        public float _rotY;
        public float _rotZ;
    }

    public Rot rot;
}