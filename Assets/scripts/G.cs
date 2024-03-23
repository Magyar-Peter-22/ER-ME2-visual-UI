using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class G : MonoBehaviour
{
    #region static accessibility
    public static G g
    {
        get
        {
            if (g_ == null)
            {
                g_ = FindObjectOfType<G>();
                if (g_ == null)
                    Debug.LogError("no G in the scene");
            }
            return g_;
        }
    }
    private static G g_;
    #endregion

    #region colors
    public Color ok;
    public Color error;
    #endregion

    #region file displayer prefabs
    public GameObject folderDisplayPref;
    public GameObject partsFolderDisplayPref;
    public GameObject addedPartsFolderDisplayPref;
    public GameObject failedDisplayPref;
    public GameObject modDisplayPref;
    #endregion


    #region tabs
    //the name of the playerpref where the index of the last opened tab is saved
    private const string lastTabPP = "lastTab";

    //the parent of the scrollviews
    public Transform tabs;

    //closing all tabs then activating the selected
    public void SelectTab(GameObject selected)
    {
        int selectedIndex = 0;
        for (int n = 0; n < tabs.childCount; n++)
        {
            GameObject thisTab = tabs.GetChild(n).gameObject;
            thisTab.SetActive(false);
            //finding the index of the selected tab
            if (selected == thisTab)
                selectedIndex = n;
        }
        selected.SetActive(true);
        //save the last selected tab
        PlayerPrefs.SetInt(lastTabPP, selectedIndex);
    }
    public void LoadLastSelectedTab()
    {
        try
        {
            tabs.GetChild(PlayerPrefs.GetInt(lastTabPP)).gameObject.SetActive(true);
        }
        catch
        {
            Debug.LogWarning("failed to load the last selected tab");
        }
    }

    #endregion

    public modloaderFinder mlf;
    public modal m;

    void Start()
    {
        //setting the status of the modloader even if the tab is not open
        //calling the start twice causes no problems
        mlf.Start();
        LoadLastSelectedTab();
    }

    //convert color type variable to string rich text color
    public static string CtRT(Color c)
    {
        return "#" + ColorUtility.ToHtmlStringRGBA(c);
    }
}
