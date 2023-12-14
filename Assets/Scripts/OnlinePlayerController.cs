using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class OnlinePlayerController : MonoBehaviour
{
    public int id;
    public float timeSinceLastSend;
    public bool isOwner;
    private Rigidbody rb;

    private float moveForce = 30;
    private float moveSpeed = 2;

    private InputData inputData;

    private Queue<InputData> sendInputQueue = new Queue<InputData>();
    
    public InputData[] inputBuffer = new InputData[2048];
    public Vector3[] positionBuffer = new Vector3[2048];

    public bool shouldReconcile;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        rb = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (ClientGameManager.client != null && ClientGameManager.client.localPlayer.PlayerID == id)
        {
            isOwner = true;
            
            ClientGameManager.client.localPlayer.dataUpdateType = DataUpdateType.Input;
            inputData = (InputData)ClientGameManager.client.localPlayer.returnDataStruct;
            inputData.playerID = id;
        }
    }

    private void FixedUpdate()
    {
        if(!isOwner) return;
        
        if (ClientGameManager.client.networkTimer != null)
        {
            // networkTimer.Update(Time.deltaTime);
            
            while (ClientGameManager.client.networkTimer.ShouldTick()) 
            {
                // print(networkTimer.CurrentTick + $" - {DateTime.UtcNow.Second}:{DateTime.UtcNow.Millisecond}");
                
                Inputs();

                if (shouldReconcile)
                {
                    Reconcile();
                }
            }
        }
    }

    public void Inputs()
    {
        if (Input.GetAxis("Horizontal") != 0)
        {
            if (Input.GetAxis("Horizontal") > 0)
            {
                // if (!inputData.right)
                if(inputData.right != 1)
                {
                    inputData.right = 1;
                    // inputData.tick = ClientGameManager.client.networkTimer.CurrentTick;
                    // inputData.left = false;
                    
                    // ClientGameManager.client.localPlayer.dataToSend = ClientGameManager.client.localPlayer.ObjectToByteArray(inputData);
                    // SendData.Send(ClientGameManager.client.localPlayer, ClientGameManager.client.localPlayer.dataToSend, SendData.SendType.ReplyOne);

                    sendInputQueue.Enqueue(inputData);
                    
                }
                    // Move(1, ClientGameManager.client.networkTimer.MinTimeBetweenTicks);
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                // if (!inputData.left)
                if(inputData.right != -1)
                {    
                    // inputData.left = true;
                    inputData.right = -1;
                    // inputData.tick = ClientGameManager.client.networkTimer.CurrentTick;
                    
                    // ClientGameManager.client.localPlayer.dataToSend = ClientGameManager.client.localPlayer.ObjectToByteArray(inputData);
                    // SendData.Send(ClientGameManager.client.localPlayer, ClientGameManager.client.localPlayer.dataToSend, SendData.SendType.ReplyOne);
                    
                    sendInputQueue.Enqueue(inputData);

                }
                    // Move(-1, ClientGameManager.client.networkTimer.MinTimeBetweenTicks);
            }
        }
        else
        {
            // if (inputData.right || inputData.left)
            if(inputData.right != 0)
            {
                // inputData.left = false;
                inputData.right = 0;
                // inputData.tick = ClientGameManager.client.networkTimer.CurrentTick;
                
                // ClientGameManager.client.localPlayer.dataToSend = ClientGameManager.client.localPlayer.ObjectToByteArray(inputData);
                // SendData.Send(ClientGameManager.client.localPlayer, ClientGameManager.client.localPlayer.dataToSend, SendData.SendType.ReplyOne);
                
                sendInputQueue.Enqueue(inputData);
                
            }
                // Move(0, ClientGameManager.client.networkTimer.MinTimeBetweenTicks);
        }
        
        positionBuffer[ClientGameManager.client.networkTimer.CurrentTick] = transform.position;
        Move(inputData.right, ClientGameManager.client.networkTimer.MinTimeBetweenTicks);
    }
    
    // Update is called once per frame
    async void Update()
    {
        if(!isOwner) return;

        await HandeSendInputQueue();
        
        // RAW POSITION UPDATE (OLD TEST)
        // float horizontalAxis = Input.GetAxis("Horizontal") * Time.deltaTime;
        // if (horizontalAxis != 0)
        // {
        //     transform.position += new Vector3(horizontalAxis, 0, 0);
        //
        //     if (timeSinceLastSend > 0.5f)
        //     {
        //         ClientGameManager.client.localPlayer.dataUpdateType = DataUpdateType.Transform;
        //         
        //         TransformData transformData = (TransformData)ClientGameManager.client.localPlayer.returnDataStruct;
        //         transformData.pos = new Pos
        //         {
        //             _posX = transform.position.x,
        //             _posY = transform.position.y,
        //             _posZ = transform.position.z
        //         };
        //
        //         
        //         ClientGameManager.client.localPlayer.dataToSend = ClientGameManager.client.localPlayer.ObjectToByteArray(transformData); 
        //         print($"{ClientGameManager.client.localPlayer.dataToSend.Length} {ClientGameManager.client.localPlayer.dataUpdateType}");
        //         // print(bytes.Length);
        //
        //         // Send(sizeOfMsg, bytes);
        //         SendData.Send(ClientGameManager.client.localPlayer, ClientGameManager.client.localPlayer.dataToSend, SendData.SendType.ReplyOne);
        //         // _clientSocket.BeginSend()
        //         timeSinceLastSend = 0;
        //     }
        // }
        //
        // timeSinceLastSend += Time.deltaTime;
    }

    private async Task HandeSendInputQueue()
    {
        while (sendInputQueue.Count > 0)
        {
            InputData inputs = sendInputQueue.Dequeue();
            inputBuffer[ClientGameManager.client.networkTimer.CurrentTick] = inputs;

            ClientGameManager.client.localPlayer.dataUpdateType = DataUpdateType.Input;
            inputData.tick = ClientGameManager.client.networkTimer.CurrentTick;
            ClientGameManager.client.localPlayer.dataToSend =
                ClientGameManager.client.localPlayer.ObjectToByteArray(inputs);
            await SendData.Send(ClientGameManager.client.localPlayer, ClientGameManager.client.localPlayer.dataToSend,
                SendData.SendType.ReplyOne);
        }
    }

    public void Move(float horizontalInput, float timeBetweenTicks)
    {
        // print(horizontalInput);
        Vector3 moveDir = new Vector3(horizontalInput, 0, 0);
        transform.position += moveDir * (moveSpeed * timeBetweenTicks);
        // rb.AddForce(moveDir * (moveForce * timeBetweenTIcks), ForceMode.VelocityChange);
    }

    private Vector3 tempPos;
    private Vector3 predictedPlayerPos;
    public bool ShouldReconcile(TransformData transformData)
    {
        if (!isOwner) return false;
        
        int dataTick = transformData.tick;
        tempPos = new Vector3(transformData.pos._posX, transformData.pos._posY,
            transformData.pos._posZ);
                
        // Simulate from the new position to current tick and then check if reconciliation needed
        while (dataTick <= ClientGameManager.client.networkTimer.CurrentTick)
        {
            Vector3 moveDir = new Vector3(inputBuffer[dataTick].right, 0, 0);
            predictedPlayerPos = tempPos +
                           moveDir * (moveSpeed * ClientGameManager.client.networkTimer.MinTimeBetweenTicks);

            if (dataTick == ClientGameManager.client.networkTimer.CurrentTick && (positionBuffer[ClientGameManager.client.networkTimer.CurrentTick] - predictedPlayerPos).magnitude > 0.5f)
            {
                shouldReconcile = true;
                // transform.position = Vector3.Lerp(transform.position,
                //     tempPos, ClientGameManager.client.networkTimer.MinTimeBetweenTicks);

                // if (inputData.tick <= ClientGameManager.client.networkTimer.CurrentTick)
                // {
                //             
                //             Move(inputBuffer[transformData.tick].right, ClientGameManager.client.networkTimer.MinTimeBetweenTicks);
                //             inputData.tick++;
                // }
            }
            else
            {
                shouldReconcile = false;
            }

            tempPos = predictedPlayerPos;
            dataTick++;
        }

        return true;
    }
    
    private void Reconcile()
    {
        transform.position = Vector3.Lerp(transform.position,
                predictedPlayerPos, ClientGameManager.client.networkTimer.MinTimeBetweenTicks);
    }
}