using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class openlink : hoverable
{
    public void OpenLink()
    {
        Application.OpenURL(message);
    }
}
