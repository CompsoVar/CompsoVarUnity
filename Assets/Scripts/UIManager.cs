using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Image infoImage;
    // Start is called before the first frame update
    void Start()
    {
        HideImage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool isShowingInfoImage = false;
    public void ShowInfoImage(Sprite sprite)
    {
        if (!isShowingInfoImage)
        {
            infoImage.sprite = sprite;
            infoImage.gameObject.SetActive(true);
            isShowingInfoImage = true;
        }
    }

    public void HideImage()
    {
        isShowingInfoImage = false;
        infoImage.gameObject.SetActive(false);
    }
}
