using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public Image fadeImage => GameManager.Instance.UIManager.fadeImage;
    public float fadeDuration = 1f;
    public float minimumBlackTime = 1f;

    private bool isTransitioning = false;

    public void LoadScene(string sceneName)
    {
        if (!isTransitioning)
            StartCoroutine(FadeInOut(sceneName));
    }

    private IEnumerator FadeInOut(string sceneName)
    {
        isTransitioning = true;

        // 1. Fade In (écran devient noir)
        yield return Fade(0f, 1f,fadeDuration);

        // 2. Attendre un minimum de temps noir
        yield return new WaitForSeconds(minimumBlackTime);

        if (sceneName.Contains("Lagoon"))
        {
            GameManager.Instance.UIManager.ShowLagoonView();
        }
        else
        {
            GameManager.Instance.UIManager.ShowQuarryView();
        }

        // 3. Charger la scène
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 4. Fade Out (réapparaître)
        yield return Fade(1f, 0f,fadeDuration);

        isTransitioning = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float fadeDuration)
    {
        // Ici tu appelles directement la coroutine de UIManager
        yield return StartCoroutine(GameManager.Instance.UIManager.Fade(startAlpha, endAlpha, fadeDuration));
    }



}
