using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemClick : MonoBehaviour, IPointerClickHandler
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

    private void OnMouseDown()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        print("mousedown");
        GameManager.Instance.UIManager.ShowInfoImage(infoImage);
    }

}
