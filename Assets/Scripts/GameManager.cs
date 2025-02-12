using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    public UIManager UIManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void LoadSceneBefore()
    {
        SceneManager.LoadScene("LagoonWater");
    }

    public void LoadSceneAfter()
    {
        SceneManager.LoadScene("QuarryScene");
    }


}
