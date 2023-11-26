using UnityEngine;

public class HandleDataServer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AsynchronousSocketListener.ProcessDataServer += ProcessDataServer;
    }

    void ProcessDataServer(object data, DataUpdateType dataTye)
    {
        switch (dataTye)
        {
            case DataUpdateType.Joining:

                JoiningData joiningData = (JoiningData) data;
                
                break;
            
            case DataUpdateType.Ready:

                ReadyStatus readyStatus = (ReadyStatus) data;
                if (readyStatus.ready)
                {
                    print("Start Game");
                }
                
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        AsynchronousSocketListener.ProcessDataServer -= ProcessDataServer;
    }
}