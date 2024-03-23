using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using System;
using System.IO;

public class filehandler : fileDisplayerBase
{
    public FolderInput modFolderInput;

    void Start()
    {
        modFolderInput.onSuccess = OnFilesSelected;
    }

    //searching for installable mods in the folders selected by the user
    public void OnFilesSelected(string[] paths)
    {
        CleanUp();
        PathsToEntryDatas(paths);

        //adding parts folders if necessary
        #region adding parts folders
        foreach (entryData ed in rootEntryDatas)
            add_parts_folders(ed);
        #endregion

                WriteLocalPathAll();
        DisplayEntryDatas();
    }

    //recursively add parts folders above the mods if missing
    void add_parts_folders(entryData ed)
    {
        //if the folder has mods in it directly , then it must be a parts folder
        if (ed.hasPartsModsDirectly && ed.type == entryData.entryType.folder)
        {
            if (ed.name != "parts")
            {
                //separating the childs of the folder to 2 groups: mods and not mods 
                //the mods will go to the new parts folder that will be added to the remaining childs of this folder
                List<entryData> mod_childs = new List<entryData>();
                List<entryData> remaining_childs = new List<entryData>();
                if(ed.children!=null)
                foreach (var child in ed.children)
                {
                    if (child.type == entryData.entryType.mod)
                        mod_childs.Add(child);
                    else
                        remaining_childs.Add(child);
                }

                //creating the new parts folder
                entryData added_parts_folder = new entryData
                {
                    name = "parts",
                    children = mod_childs,
                    type = entryData.entryType.folder,
                    subType = entryData.SubType.added_parts_folder,
                    hasModChild = true,
                    hasPartsModsDirectly = true,
                    path = "-"
                };

                //adding the parts folder to the child list and updating tis folder
                remaining_childs.Add(added_parts_folder);
                ed.hasPartsModsDirectly = false;
                ed.children = remaining_childs;
            }
        }

        //processing the childs too
        if (ed.children != null)
            foreach (var child in ed.children)
                add_parts_folders(child);

    }


    //copying the enabled entries to the mod folder of the modloader
    public void InstallSelected()
    {
        //if modloader no found, error
        if (!G.g.mlf.ok)
        {
            G.g.m.DisplayErrorMessage("modloader2 must be located in the 'find modloader2' tab before installing mods");
            return;
        }
        //if 0 root folders, error
        if (rootEntryDatas == null || rootEntryDatas.Count == 0)
        {
            G.g.m.DisplayErrorMessage("no folders selected");
            return;
        }

        //install the entries
        int successfullyInstalled = 0;
        string modFolderPath = G.g.mlf.modFolderPath;
        foreach (entryData ed in rootEntryDatas)
            if (InstallEntry(ed, modFolderPath))
                successfullyInstalled++;

        //if 0 successfull install, error
        if (successfullyInstalled == 0)
        {
            G.g.m.DisplayErrorMessage("no valid mods found, nothing was installed");
            return;
        }

        //send message on success
        G.g.m.DisplayOkMessage("successfully installed '" + successfullyInstalled + "' mods\nselect the 'manage mods' tab to see them");

        CleanUp();
    }
    //recussively copy the files of a selected entry to the mod folder, returns bool that marks if any files were copied successfully
    //this function uses a separate localpath variable, because it can change the name of the folders to avoid duplications
    bool InstallEntry(entryData ed, string localPath)
    {
        if (ed.display.IsEnabled())
        {
            if (ed.type == entryData.entryType.folder)
            {
                //creating unique name
                string originalName = ed.name;
                for (int n = 1; n < 100; n++)
                {
                    if (FileBrowserHelpers.DirectoryExists(Path.Combine(localPath, ed.name)))
                        ed.name = originalName + " (" + n + ")";
                    else
                        break;
                }
                if (originalName != ed.name)
                    Debug.Log("the name of folder '" + originalName + "' was changed to '" + ed.name + "' becasue this name was already taken in the mod folder");

                Debug.Log("copying folder from '" + ed.path + "' to '" + localPath + "'");
                FileBrowserHelpers.CreateFolderInDirectory(localPath, ed.name);

                //updating the localpath for the children
                localPath = Path.Combine(localPath, ed.name);

if(ed.children!=null)
                foreach (var child in ed.children)
                    InstallEntry(child, localPath);

                return true;
            }
            else if (ed.type == entryData.entryType.mod)
            {
                Debug.Log("copying file from '" + ed.path + "' to '" + localPath + "'");
                FileBrowserHelpers.CopyFile(ed.path, Path.Combine(localPath, ed.name));
                return true;
            }
        }
        return false;
    }
}
