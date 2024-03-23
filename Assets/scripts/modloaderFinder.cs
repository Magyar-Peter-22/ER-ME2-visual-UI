using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;

public class modloaderFinder : MonoBehaviour
{
    public FolderInput folderInput;
    public Text statusText;
    private const string lastSuccessfulPathPP = "lastModloaderPath";
    //the image of this button will be colored depending on the status of the found modloader
    public Image colorButton;
    public const string modFolderName = "mod";
    public const string tomlFolderName = "config_eldenring.toml";
    public string path{
        get{
            return path_;
        }
        set{
            path_=value;
           modFolderPath= Path.Combine(path_, modFolderName);
                      tomlFilePath= Path.Combine(path_, tomlFolderName);
        }
    }
    private string path_;
    public string modFolderPath;
    public string tomlFilePath;
    public bool ok;

    public void Start()
    {
        folderInput.onSuccess = OnFileSelected;
        //try to load the modloader from the last successful path
        OnFileSelected(new string[1] { PlayerPrefs.GetString(lastSuccessfulPathPP) });
    }

    public void OnFileSelected(string[] paths)
    {
        //these entry names must be present directly in the selected folder
        List<string> searchingFor = new List<string> { modFolderName, tomlFolderName };
        //these names were found
        List<string> found = new List<string>();
        //only one path can be selected
     path = paths[0];
        try
        {
            //getting the entries of the selected folder
            FileSystemEntry[] entries = FileBrowserHelpers.GetEntriesInDirectory(path, false);
            foreach (var entry in entries)
            {
                //if searching for this name, and still not found it, add it to the found list
                if (searchingFor.Contains(entry.Name) && !found.Contains(entry.Name))
                    found.Add(entry.Name);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);//if the loaded path is invalid
        }

        //write infos
        string status = "-path: '" + path + "'\n";

        //write the found status of all searched items
        foreach (var searched in searchingFor)
        {
            status += found.Contains(searched) ?
            "<color="+G.CtRT(G.g.ok)+">-'" + searched + "' found\n</color>" :
            "<color="+G.CtRT(G.g.error)+">-'" + searched + "' not found\n</color>";
        }

        //if all searched names were found, write ok message and save the path
        //color the tab button depending on the result
        if (found.Count == searchingFor.Count)
        {
            status += "<color="+G.CtRT(G.g.ok)+">-ready to install mods</color>";
            PlayerPrefs.SetString(lastSuccessfulPathPP, path);
            colorButton.color=G.g.ok;
            ok=true;
        }
        else
        {
                    colorButton.color=G.g.error;
                    ok=false;
        }

        statusText.text = status;
    }
}
