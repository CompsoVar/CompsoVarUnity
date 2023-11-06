using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    public void Show()
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }
}
