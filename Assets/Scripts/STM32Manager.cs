using System;
using System.IO.Ports;
using UnityEngine;
using System.Threading;

public class RFIDReader : MonoBehaviour
{
    private SerialPort serialPort;
    public int baudRate = 115200; // Vitesse de communication
    public string port = "COM9";  // Stockera le port détecté
    private bool isReading = false;
    private Thread readThread;

    void Start()
    {
        string[] ports = SerialPort.GetPortNames();

        // Afficher les ports COM dans la console
        foreach (string port in ports)
        {
            Debug.Log("Port COM disponible : " + port);
        }
        // Initialiser la connexion série
        serialPort = new SerialPort(port, baudRate);
        serialPort.Open();
        serialPort.ReadTimeout = 100;    // Timeout pour les lectures
        isReading = true;

        // Démarrer le thread de lecture
        readThread = new Thread(ReadDataFromSerialPort);
        readThread.Start();
    }

    private void ReadDataFromSerialPort()
    {
        while (isReading)
        {
            print("cac");
            try
            {
                // Lire une ligne de données du STM32
                string line = serialPort.ReadLine(); // Lecture d'une ligne complète
                print(line);
                // Traitement des UID
                if (line != "")
                {
                    Debug.Log("UID de la carte : " + line);
                    if (line == "0")
                    {
                        Debug.Log("charger prehistoire");
                    }
                    else
                    {
                        Debug.Log("charger present");
                    }
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception("A timeout occurred.", ex);
            }


            // Attendre un peu avant de lire à nouveau
            Thread.Sleep(1000);
        }
    }

    void OnDestroy()
    {
        isReading = false;
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join();
        }
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }

    void OnApplicationQuit()
    {
        isReading = false;
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join();
        }
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }
}
