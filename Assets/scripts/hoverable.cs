using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class hoverable : MonoBehaviour,  IPointerEnterHandler,IPointerExitHandler
{
    public string message;
    public void OnPointerEnter(PointerEventData eventData)
    {
       ToolTipHover.me.Enter(this);
    }
     public void OnPointerExit(PointerEventData eventData)
    {
         ToolTipHover.me.Exit(this);
    }
}
