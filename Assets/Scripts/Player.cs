using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// C1: Send Connection Request with data
/// S: Read player socket and add to connected clients list
///     store socket in player object
///     Check for size of data
///     Read the data with size
///     Update player Object with Name, 
/// </summary>


[Serializable]
public class Player
{
    // Client  socket.
    public Socket workSocket = null;

    public string playerName;
    // public float health;
    // Size of receive buffer.
    // public static int BufferSize = sizeof(int);
    // Receive buffer.
    public byte[] dataRecd = new byte[sizeof(int)];

    // public int updateVal = -1;
    public DataUpdateType dataUpdateType;
    
    // Received data string.
    // public StringBuilder sb;
    // public Data data;
    
    // DataType must be created before sending
    public object returnDataStruct => ReturnDataStruct();
    
    private object ReturnDataStruct()
    {
        switch (dataUpdateType)
        {
            case DataUpdateType.Joining:

                // Debug.Log("JOINING");
                JoiningData jd = new JoiningData();
                return jd;
                
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
            // Data data = (Data) binaryFormatter.Deserialize(memoryStream);
            switch (dataUpdateType)
            {
                case DataUpdateType.Joining:
                    
                    JoiningData joiningData = (JoiningData) binaryFormatter.Deserialize(memoryStream);
                    return joiningData;
                    
                    break;
                
                case DataUpdateType.Transform:

                    TransformData transformData = (TransformData) binaryFormatter.Deserialize(memoryStream);
                    return transformData;
                    
                    break;
            }
            // return data;
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
public class JoiningData
{
    public string playerName;
}

[Serializable]
public class TransformData
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

// [Serializable]
// public class JoiningData
// {
//     public string playerName;
// }

[Serializable]
public enum DataUpdateType
{
    Joining,
    Transform,
    Heath
}

[Serializable]
public class Data
{
    // public DataUpdateType _dataUpdateType;
    public string playerName;

    
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

    // public float health;
}
