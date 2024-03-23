using System;
using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static fileDisplayerBase;

public class displayBase : MonoBehaviour
{
    public Text entryName;
    public Toggle toggle;
    public GameObject deleteButton;
    public entryData myEntry;
    public void SetupBasics(entryData ed)
    {
        myEntry = ed;
        entryName.text = ed.name;
        if (!activeDisplay.allowDelete)
            DestroyImmediate(deleteButton);

        //the folders inside a toml folder cannot be disabled because only the path of the toml folders go into the config file, there is no way to adjust their subfolders
        //but unlike the folders, the mods can be disabled even if they are inside the toml folder by altering their extension
        ResetInteractable();
    }
    public void ResetInteractable()
    {
        if(activeDisplay.tomlDescentantsEnabled)
        {
        bool notInteractable = myEntry.descendantOfToml  && myEntry.type != entryData.entryType.mod;
            toggle.isOn = notInteractable;
        toggle.interactable = !notInteractable;
        }
    }
    public bool IsEnabled()
    {
        return toggle != null ? toggle.isOn : false;
    }


    //getting the parent transform where the display will go if the 'parentFolder' is null
    public static Transform DefaultParentFolder
    {
        get
        {
            return activeDisplay.rootEntryParent;
        }
    }

    public void Delete()
    {
        G.g.m.Confirm("do you want to delete this entry? the selected file or folder will be removed from the 'mod' folder and cannot be restored.", ConfirmedDelete);
    }
    public void ConfirmedDelete()
    {
        try
        {
            if (myEntry.type == entryData.entryType.folder || myEntry.type == entryData.entryType.failed)
                FileBrowserHelpers.DeleteDirectory(myEntry.path);
            else
                FileBrowserHelpers.DeleteFile(myEntry.path);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
        DestroyImmediate(gameObject);

    }
}
