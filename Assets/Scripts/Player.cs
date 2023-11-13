using System;
using System.Net.Sockets;
using System.Text;
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
    // Size of receive buffer.
    // public static int BufferSize = sizeof(int);
    // Receive buffer.
    public byte[] dataRecd = new byte[sizeof(int)];
    // Received data string.
    // public StringBuilder sb;
    public Data data;
}

// [Serializable]
// public class JoiningData
// {
//     public string playerName;
// }
//
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
    public DataUpdateType _dataUpdateType;
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
}
