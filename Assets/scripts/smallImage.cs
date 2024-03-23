using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class smallImage:MonoBehaviour
{
    private Image i;
    void Start()
    {
        i = GetComponent<Image>();
hoverable h = gameObject.AddComponent<hoverable>();
h.message="click to magnify";
    }
    public void Open()
    {
        G.g.m.DisplayImage(i.sprite);
    }
}
