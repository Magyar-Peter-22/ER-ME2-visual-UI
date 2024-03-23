using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipHover : MonoBehaviour
{

    RectTransform rect;
    RectTransform parent;
    Vector2 offset;

    public static ToolTipHover me;
    public List<hoverable> inside = new List<hoverable>();
    public Text content;
    float xMax;
    float xMin;
    float yMax;
    float yMin;

    // Start is called before the first frame update
    void Start()
    {
        me = this;
        rect = GetComponent<RectTransform>();
        parent = transform.parent.GetComponent<RectTransform>();
        offset = rect.anchoredPosition;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        xMax = parent.sizeDelta.x / 2f - (1f - rect.pivot.x) * rect.sizeDelta.x;
        xMin = -(parent.sizeDelta.x / 2f) + rect.pivot.x * rect.sizeDelta.x;
        yMax = parent.sizeDelta.y / 2f - (1f - rect.pivot.y) * rect.sizeDelta.y;
        yMin = -(parent.sizeDelta.y / 2f) + rect.pivot.y * rect.sizeDelta.y;

        transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        Vector2 p = rect.anchoredPosition;
        rect.anchoredPosition = new Vector2(Mathf.Clamp(p.x + offset.x, xMin, xMax), Mathf.Clamp(p.y + offset.y, yMin, yMax));
    }
    public void Enter(hoverable h)
    {
        inside.Add(h);
        UpdateContent();
    }
    public void Exit(hoverable h)
    {
        inside.Remove(h);
        UpdateContent();
    }
    private void UpdateContent()
    {
        if (inside.Count > 0)
        {
            gameObject.SetActive(true);
            content.text = inside[inside.Count - 1].message;
        }
        else
            gameObject.SetActive(false);
    }
}
