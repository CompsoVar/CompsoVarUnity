using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using UnityEngine;

public class ESPManager : MonoBehaviour
{

    private SerialPort serialPort;
    public int COM = 0;
    public int readTimeoutMilliseconds = 1; // Temps d'attente en millisecondes
    private Manager managerScript;
    void Start()
    {
        string portName = "COM" + COM.ToString();
        serialPort = new SerialPort(portName, 115200);
        serialPort.Open();
        serialPort.ReadTimeout = readTimeoutMilliseconds; // Définir le temps d'attente
        managerScript = GetComponent<Manager>();
    }

    void FixedUpdate()
    {
        try
        {
            if (serialPort.BytesToRead > 0)
            {
                string receivedData = serialPort.ReadLine();
                UnityEngine.Debug.Log(receivedData);

                if (receivedData == "1")
                {
                    managerScript.LoadPeriod(0);
                }
                else if (receivedData == "2")
                {
                    managerScript.LoadPeriod(1);
                }
            }
        }
        catch (TimeoutException e)
        {
            // Ce bloc sera exécuté si le délai d'attente est dépassé sans recevoir de données
           // UnityEngine.Debug.LogError("Timeout Exception: " + e.Message);
        }
        catch (IOException e)
        {
           // UnityEngine.Debug.LogError("IO Exception: " + e.Message);
        }
        catch (InvalidOperationException e)
        {
            //UnityEngine.Debug.LogError("Invalid Operation Exception: " + e.Message);
        }
        catch (IndexOutOfRangeException e)
        {
            //UnityEngine.Debug.LogError("Index Out of Range Exception: " + e.Message);
        }
    }
}