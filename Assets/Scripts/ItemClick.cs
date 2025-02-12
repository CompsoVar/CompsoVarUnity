using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Sprite infoImage;
    private Vector3 defaultScale;
    private void Start()
    {
        defaultScale = transform.localScale;
    }

    private void OnMouseDown()
    {
        ActionClick();
    }

    private void OnMouseEnter()
    {
        ActionEnter();
    }

    private void OnMouseExit()
    {
        ActionExit();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ActionClick();
    }

    private void ActionClick()
    {
        GameManager.Instance.UIManager.ShowInfoImage(infoImage);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ActionEnter();
    }

    private void ActionEnter()
    {
        transform.localScale = defaultScale * 1.2f;
        //transform.localScale += Vector3.one/2;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ActionExit();
    }

    private void ActionExit()
    {
        transform.localScale = defaultScale;
    }
}
