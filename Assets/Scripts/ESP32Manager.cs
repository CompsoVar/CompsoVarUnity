using System;
using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.SceneManagement;

public class ESP32Manager : MonoBehaviour
{
    [SerializeField] private int port = 4210;
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool threadRunning = false;
    private UnityMainThreadDispatcher mainThreadDispatcher;
    private int targetSceneIndex = 0;
    private int prehistoricEraScene = 0; // TODO : A modifier
    private int modernEraScene = 1; // TODO : A modifier

    void Start()
    {
        // Ensure UnityMainThreadDispatcher is initialized on the main thread
        mainThreadDispatcher = UnityMainThreadDispatcher.Instance();

        // Initialize UDP client and start the receive thread
        udpClient = new UdpClient(port);
        threadRunning = true;
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.Start();

        Debug.Log($"UDP Receiver started on port {port}");
    }

    private void ReceiveData()
    {
        while (threadRunning)
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(data);

                // Use the dispatcher to queue the processing of the message on the main thread
                mainThreadDispatcher.Enqueue(() => ProcessReceivedMessage(message));
            }
            catch (SocketException e)
            {
                Debug.LogError($"SocketException: {e}");
            }
        }
    }

    private void ProcessReceivedMessage(string prehistoricEra)
    {
        Debug.Log($"Message received from ESP32: {prehistoricEra}");
        int newSceneIndex = (prehistoricEra == "0") ? modernEraScene : prehistoricEraScene;
        targetSceneIndex = newSceneIndex;
        if (SceneManager.GetActiveScene().buildIndex != targetSceneIndex)
        {
            SceneManager.LoadScene(targetSceneIndex);
        }

    }


    void OnDestroy()
    {
        threadRunning = false;
        if (receiveThread != null)
        {
            receiveThread.Abort();
        }
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}

// Cette classe est n cessaire pour ex cuter du code sur le thread principal de Unity
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance = null;
    private readonly Queue<Action> _executionQueue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (!_instance)
        {
            _instance = FindObjectOfType(typeof(UnityMainThreadDispatcher)) as UnityMainThreadDispatcher;
            if (!_instance)
            {
                var obj = new GameObject("MainThreadDispatcher");
                _instance = obj.AddComponent<UnityMainThreadDispatcher>();
            }
        }
        return _instance;
    }

    void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }
}