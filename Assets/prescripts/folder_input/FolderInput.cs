using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System;
using System.IO;

public class FolderInput : MonoBehaviour
{
    public InputField defaultFolder;
    public string defaultFolderPP = "defaultFolder";
    //the void that will be called when files are selected
    //filled by other scripts
    public FileBrowser.OnSuccess onSuccess;
    public bool allowMultiSelection;

    //loading the saved values on start
     void Start()
    {
        LoadDefaultFolderInput();
    }
    //open the file displayer window
    public void DisplayFiles()
    {
        //getting the starting folder
        string startingFolder;
        if (defaultFolder.text != "")
            startingFolder = defaultFolder.text;
        else
            startingFolder = GetDefaultDownloadsFolder();

        FileBrowser.ShowLoadDialog(onSuccess, null, FileBrowser.PickMode.Folders, allowMultiSelection, startingFolder);
    }

    //returns the downloads folder of the current user on windows only
    public string GetDefaultDownloadsFolder()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    }

    //when the default fodler selector changes, save it
    public void DefaultFolderInputChanged()
    {
        PlayerPrefs.SetString(defaultFolderPP, defaultFolder.text);
    }
    public void ResetDefaultFolder()
    {
        defaultFolder.text=GetDefaultDownloadsFolder();
        DefaultFolderInputChanged();
    }

    //writes text into the default folder selector
    //if saved value exists, load it, if not, use the default
    private void LoadDefaultFolderInput()
    {
        string found = PlayerPrefs.GetString(defaultFolderPP);
        if (found == "")
            found = GetDefaultDownloadsFolder();
        defaultFolder.text = found;
    }


}
