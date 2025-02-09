using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemClick : MonoBehaviour
{
    [SerializeField] Sprite infoImage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseDown()
    {
        print("mousedown");
        GameManager.Instance.UIManager.ShowInfoImage(infoImage);
    }
}
