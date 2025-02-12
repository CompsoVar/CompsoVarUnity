using System;
using System.IO.Ports;
using UnityEngine;
using System.Threading;
using UnityEngine.SceneManagement;

public class RFIDReader : MonoBehaviour
{
    private SerialPort serialPort;
    public int baudRate = 115200;
    public string port = "COM9";
    private bool isReading = false;
    private Thread readThread;

    private string receivedUID = "";
    private bool sceneLoadRequested = false;
    private bool messageRequested = false;
    private string message;
    private int targetSceneIndex = 0;
    private int prehistoricEraScene = 0; // TODO : A modifier
    private int modernEraScene = 1; // TODO : A modifier

    void Start()
    {
        Debug.Log("Tentative de connexion au port " + port);
        serialPort = new SerialPort(port, baudRate);
        serialPort.ReadTimeout = 100;

        try
        {
            serialPort.Open();
            isReading = true;
            readThread = new Thread(ReadDataFromSerialPort);
            readThread.Start();
            Debug.Log("Port série ouvert avec succès");
        }
        catch (Exception ex)
        {
            Debug.LogError("Erreur port série: " + ex.Message);
        }
    }

    void Update()
    {
        if (sceneLoadRequested)
        {
            if (SceneManager.GetActiveScene().buildIndex != targetSceneIndex)
            {
                SceneManager.LoadScene(targetSceneIndex);
                sceneLoadRequested = false;
            }
        }
        if (messageRequested)
        {
            Debug.Log(message);
            messageRequested = false;
        }
    }

    private void ReadDataFromSerialPort()
    {
        while (isReading)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    string prehistoricEra = serialPort.ReadTo("\n");
                    if (!string.IsNullOrEmpty(prehistoricEra))
                    {
                        int newSceneIndex = (prehistoricEra == "0") ? modernEraScene : prehistoricEraScene;
                        targetSceneIndex = newSceneIndex;
                        sceneLoadRequested = true;
                    }
                }
                catch (TimeoutException)
                {

                }
                catch (Exception ex)
                {
                    message = "Erreur lecture: " + ex.Message;
                    messageRequested = true;
                }
            }
            else
            {
                Debug.LogError("Port série fermé ou non initialisé");
                isReading = false;
            }

            Thread.Sleep(100);
        }
    }

    private void StopReading()
    {
        isReading = false;
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join(500); // Attendre que le thread se termine
        }
    }

    void OnDestroy()
    {
        StopReading();
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }
}
