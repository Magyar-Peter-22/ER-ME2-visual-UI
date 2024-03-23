using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class modal : MonoBehaviour
{
    public Text message;
    public Image img;
    public GameObject yesno;
    public Button yesButton;
    public Text yesnoText;
    public Transform contents;
    #region starting animation

    public void Confirm(string message, UnityEngine.Events.UnityAction onYes)
    {
        HideAll();
        yesnoText.text = message;
        yesButton.onClick.RemoveAllListeners();
                yesButton.onClick.AddListener(Confirmed);
        yesButton.onClick.AddListener(onYes);
        yesno.SetActive(true);
        gameObject.SetActive(true);
    }
    public void Confirmed()
    {
        gameObject.SetActive(false);
    }
    private const float animationTime = 0.5f;
    void OnEnable()
    {
        StartCoroutine(Grow());
    }
    IEnumerator Grow()
    {
        float p = 0;
        while (p != 1f)
        {
            p = Mathf.MoveTowards(p, 1f, Time.deltaTime / animationTime);
            contents.localScale = Vector2.one * Mathf.SmoothStep(0f, 1f, p);
            yield return new WaitForEndOfFrame();
        }
    }
    #endregion
    public void DisplayMessage(string m)
    {
        HideAll();
        message.text = m;
        message.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }
    public void DisplayErrorMessage(string m)
    {
        DisplayMessage("<color=" + G.CtRT(G.g.error) + ">" + m + "</color>");
    }
    public void DisplayOkMessage(string m)
    {
        DisplayMessage("<color=" + G.CtRT(G.g.ok) + ">" + m + "</color>");
    }

    public void DisplayImage(Sprite s)
    {
        HideAll();
        img.sprite=s;
        img.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }
    public void HideAll()
    {
        for (int n = 0; n < contents.childCount; n++)
        {
            contents.GetChild(n).gameObject.SetActive(false);
        }

    }
}
