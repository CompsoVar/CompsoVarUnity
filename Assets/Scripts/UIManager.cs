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

    public bool IsShowingInfoImage { private set; get; } = false;
    public void ShowInfoImage(Sprite sprite)
    {
        if (!IsShowingInfoImage)
        {
            infoImage.sprite = sprite;
            infoImage.gameObject.SetActive(true);
            IsShowingInfoImage = true;
        }
    }

    public void HideImage()
    {
        IsShowingInfoImage = false;
        infoImage.gameObject.SetActive(false);
    }
}
