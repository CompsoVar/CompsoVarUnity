using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemClick : MonoBehaviour //, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Sprite infoImage;
    private Vector3 defaultScale;
    [SerializeField] Material outline;
    [SerializeField] bool grow = true;

    private void Start()
    {
        defaultScale = transform.localScale;
    }

    //private void OnMouseDown()
    //{
    //    ActionClick();
    //}

    //private void OnMouseEnter()
    //{
    //    ActionEnter();
    //}

    //private void OnMouseExit()
    //{
    //    ActionExit();
    //}

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    ActionClick();
    //}

    public void ActionClick()
    {
        GameManager.Instance.UIManager.ShowInfoImage(infoImage);
    }

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    ActionEnter();
    //}

    public void ActionEnter()
    {
        if(grow)
            transform.localScale = defaultScale * 1.2f;
        SetLayerRecursively(this.gameObject, LayerMask.NameToLayer("Outline"));
 
    }

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    ActionExit();
    //}

    public void ActionExit()
    {
        transform.localScale = defaultScale;
        SetLayerRecursively(this.gameObject, LayerMask.NameToLayer("Default"));
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        if (!LayerMask.LayerToName(obj.layer).Equals("NoOutline"))
        {
            obj.layer = layer;
        }
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }


}
