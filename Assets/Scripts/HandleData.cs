using System;
using UnityEngine;

public class HandleData : MonoBehaviour
{
    private static AsynchronousSocketListener asynchronousSocketListener;

    private void Awake()
    {
        // asynchronousSocketListener = GetComponent<AsynchronousSocketListener>();
    }

    // TODO: CHANGE TO ACTION
    public static void Handle(Player state, Data data, int bytesRead)
    {
        switch (data._dataUpdateType)
        {
            case DataUpdateType.Joining:
                    
                state.playerName = data.playerName;
                print($"{state.playerName} CONNECTED!");
                    
                break;
                
            case DataUpdateType.Transform:
                    
                string content = $"{state.data.pos._posX}, {state.data.pos._posY}, {state.data.pos._posZ}";
                print($"{state.playerName} : {content}");
                // asynchronousSocketListener.Reply(state, data);
                
                break;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if (a != null)
        // {
        //     print(a);
        // }
    }
}
