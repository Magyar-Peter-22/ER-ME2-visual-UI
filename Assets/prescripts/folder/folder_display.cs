using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static fileDisplayerBase;


public class folder_display : displayBase
{
    //displays the type variable of the entrydata
    public Text type;
    //displays the path variable of the entrydata
    public Text path;
    //the subfolder displayers are stored here
    public Transform subfoldersParent;
    //subfolder displays
    public folder_display[] subfolders;
    //the mod file displayers are stored here
    //if there are no mods, disabled
    public Transform modsParent;
    //contains the whole mod displayer, not the same as modsparent, disabled by default, enabled by mod on creation
    public GameObject modDisplayer;
    //mod displayers
    public dcx_row[] mods;

    #region collapsing
    private Coroutine collapsing = null;
    private bool open = false;
    public Transform collapse;
    public Transform collapseButtonIcon;
    private float animTime = 0.5f;
    //public RectTransform updateWhenAnim;

    public void ToggleCollapse()
    {
        if (collapsing != null)
            StopCoroutine(collapsing);
        collapsing = StartCoroutine(toggleCollapse());
    }
    IEnumerator toggleCollapse()
    {
                collapse.gameObject.SetActive(true);
        open = !open;
        collapseButtonIcon.localScale = new Vector2(collapseButtonIcon.localScale.x, open ? -1f : 1f);
        float from = collapse.localScale.y;
        float targetSize = open ? 1f : 0f;
        float p = 0;
        while (p != 1)
        {
            p = Mathf.MoveTowards(p, 1f, Time.deltaTime / animTime);
            collapse.localScale = new Vector2(collapse.localScale.x, Mathf.SmoothStep(from, targetSize, p));
            //LayoutRebuilder.MarkLayoutForRebuild(updateWhenAnim);
            yield return new WaitForEndOfFrame();
        }
        if(!open)
        collapse.gameObject.SetActive(false);
    }
    #endregion

    public static displayBase CreateGO(entryData ed, folder_display parentFolder)
    {
        //finding the parent where the folder will be displayed
        //if it's a root entry, the parentFolder is null, so select the parent transform of the root folders from filehandler
        Transform parentTransform;
        if (parentFolder != null)
            parentTransform = parentFolder.subfoldersParent;
        else
            parentTransform = DefaultParentFolder;

        //selecting the gameobject to clone
        GameObject chosen;
        if (ed.subType == entryData.SubType.added_parts_folder)
            chosen = G.g.addedPartsFolderDisplayPref;
        else if (ed.subType == entryData.SubType.parts_folder)
            chosen = G.g.partsFolderDisplayPref;
        else
            chosen = G.g.folderDisplayPref;

        //creating the gameobject in it's parent transform
        GameObject instance = Instantiate(chosen, parentTransform);

        //getting the displayer script and displaying this entry on it
        folder_display fd = instance.GetComponent<folder_display>();
        fd.SetupBasics(ed);

        //filling the data fields on the gameobject
        fd.type.text = "<b>Type: </b>" + ed.type.ToString();
        if (ed.subType != entryData.SubType.none)
            fd.type.text += ", " + ed.subType.ToString();
        fd.path.text = "<b>Path: </b>" + ed.path;


        //displaying the child entries in this folder
        if (ed.children != null)
            foreach (entryData child in ed.children)
                child.createDisplayer(fd);

        return fd;
    }

}
