using System;
using UnityEngine;

public class OnlinePlayerController : MonoBehaviour
{
    public int id;
    public float timeSinceLastSend;
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ClientGameManager.client.localPlayer.PlayerID == id)
        {
            float horizontalAxis = Input.GetAxis("Horizontal") * Time.deltaTime;
            if (horizontalAxis != 0)
            {
                transform.position += new Vector3(horizontalAxis, 0, 0);

                if (timeSinceLastSend > 0.5f)
                {
                    ClientGameManager.client.localPlayer.dataUpdateType = DataUpdateType.Transform;
                    
                    TransformData transformData = (TransformData)ClientGameManager.client.localPlayer.returnDataStruct;
                    transformData.pos = new Pos
                    {
                        _posX = transform.position.x,
                        _posY = transform.position.y,
                        _posZ = transform.position.z
                    };

                    
                    ClientGameManager.client.localPlayer.dataToSend = ClientGameManager.client.localPlayer.ObjectToByteArray(transformData); 
                    print($"{ClientGameManager.client.localPlayer.dataToSend.Length} {ClientGameManager.client.localPlayer.dataUpdateType}");
                    // print(bytes.Length);

                    // Send(sizeOfMsg, bytes);
                    SendData.Send(ClientGameManager.client.localPlayer, ClientGameManager.client.localPlayer.dataToSend, SendData.SendType.ReplyOne);
                    // _clientSocket.BeginSend()
                    timeSinceLastSend = 0;
                }
            }
            
            timeSinceLastSend += Time.deltaTime;
        }
    }
}
