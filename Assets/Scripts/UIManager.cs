using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Image infoImage;
    public Image fadeImage;

    [SerializeField] GameObject LagoonView;
    [SerializeField] GameObject QuarryView;

    [SerializeField] AudioClip showClick;
    [SerializeField] AudioClip hideClick;
    // Start is called before the first frame update
    void Start()
    {
        HideImage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsShowingInfoImage { private set; get; } = false;
    public void ShowInfoImage(Sprite sprite)
    {
        if (!IsShowingInfoImage)
        {
            infoImage.sprite = sprite;
            infoImage.gameObject.SetActive(true);
            IsShowingInfoImage = true;
            AudioManager.Instance.PlayInterfaceSound(showClick);
        }
    }

    public void HideImage()
    {
        IsShowingInfoImage = false;
        infoImage.gameObject.SetActive(false);
        AudioManager.Instance.PlayInterfaceSound(hideClick);

    }


    public IEnumerator Fade(float startAlpha, float endAlpha, float fadeDuration)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);

            if (fadeImage != null)
                fadeImage.color = new Color(0f, 0f, 0f, alpha);

            yield return null;
        }
    }

    public void ShowLagoonView()
    {
        LagoonView.SetActive(true);
        QuarryView.SetActive(false);
    }

    public void ShowQuarryView()
    {
        LagoonView.SetActive(false);
        QuarryView.SetActive(true);
    }
}
