using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public UIManager UIManager;
    public SceneLoader sceneLoader;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void LoadSceneBefore()
    {
        sceneLoader.LoadScene("LagoonWater");
    }

    public void LoadSceneAfter()
    {
        sceneLoader.LoadScene("QuarryScene");
    }


}
