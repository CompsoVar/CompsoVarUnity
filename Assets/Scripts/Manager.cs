using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    [SerializeField]
    private List<string> scenesPeriod;

    public static Manager Instance { get; private set; }

    private void Awake()
    {
        Debug.Log("1: "+ Instance);
        if (Instance != null & Instance != this){
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        Debug.Log("2: "+ Instance);
    }

    public void LoadPeriod(int period)
    {
        SceneManager.LoadScene(scenesPeriod[period]);
    }
}
