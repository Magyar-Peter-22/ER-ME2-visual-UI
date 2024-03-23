using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using System;
using System.IO;
using System.Linq;

public class fileDisplayerBase : MonoBehaviour
{
    public List<entryData> rootEntryDatas;
    public Transform rootEntryParent;
    public bool allowDelete;
    public bool tomlDescentantsEnabled;
    public static fileDisplayerBase activeDisplay;
    public static string modDisablerExtension = "disabled";


    //class that contains additional data of an entry
    [System.Serializable]
    public class entryData
    {
        public string name;
        public string path;
        public FileSystemEntry entry;
        public bool hasModChild;
        //when setting the type of this entry, also set the display creator that belongs to the type
        public entryType type
        {
            get
            {
                return savedType;
            }
            set
            {
                savedType = value;
                switch (type)
                {
                    case entryType.folder:
                        creator = folder_display.CreateGO;
                        break;
                    case entryType.mod:
                        creator = dcx_row.CreateGO;
                        break;
                    case entryType.failed:
                        creator = failed_display.CreateGO;
                        break;
                }
            }
        }
        private entryType savedType;
        public enum entryType
        {
            folder,
            mod,
            failed
        }
        public List<entryData> children;
        //creates a gameobject for this entry when it needs to be displayed
        public delegate displayBase displayCreator(entryData ed, folder_display parentFolder);
        private displayCreator creator;
        public void createDisplayer(folder_display parentFolder)
        {
            display = creator(this, parentFolder);
        }

        //the displayed gameobject
        public displayBase display;
        //there are partsbnd.dcx mods directly in this folder, so this folder must be a parts folder
        public bool hasPartsModsDirectly;

        public enum SubType
        {
            none,
            parts_folder,
            added_parts_folder,
            toml_folder
        }
        public SubType subType;
        public bool descendantOfToml;
        //the path compared to the mod folder, includes and added parts folders
        public string localPath;

        //return true this mod is disabled by extension
        public bool hasDisabledExtension
        {
            get
            {
                return name.EndsWith("." + modDisablerExtension);
            }
        }
        //add or remove the disabled extension
        public void modSetActive(bool enable)
        {
            bool isEnabled = !hasDisabledExtension;
            //if the enabled status changed, write the extensions
            if (isEnabled != enable)
            {
                string newName;
                if (enable)
                    newName = getActivatedExtension(name);
                else
                    newName = getDeActivatedExtension(name);
                name = newName;
                FileBrowserHelpers.RenameFile(path, newName);
            }
        }
        //if disabled by extension, returns the not disabled name. if not, then returns the disabled name
        public string getAlternativeExtension(string value)
        {
                if (hasDisabledExtension)
                    return getActivatedExtension(value);
                else
                    return getDeActivatedExtension(value);
        }
        private string getActivatedExtension(string value)//returns correct value only when not activated
        {
            return value.Substring(0, value.Length - (modDisablerExtension.Length + 1));//reducing the length of the filename to remove the disabled extension and the dot
        }
        private string getDeActivatedExtension(string value)//returns correct value only when activated
        {
            return value + "." + modDisablerExtension;
        }
    }


    public virtual void OnEnable()
    {
        //the gameobject of the displayed entries will be moved to the active display
        activeDisplay = this;
    }

    public void CleanUp()//cleaning up the results of the previous browsing
    {
        //deleting the gameobjects
        foreach (var red in rootEntryDatas)
        {
            if (red.display != null)
                DestroyImmediate(red.display.gameObject);
        }
        //clearing the entrydata list
        rootEntryDatas = new List<entryData>();
    }

    public void PathsToEntryDatas(string[] paths)//converting the selected paths into entry datas
    {
        try
        {
            foreach (var path in paths)
            {
                entryData rootData = SearchFolder(path);
                if (rootData != null)
                {
                    //if the root entry has no mods inside it, the it will be displayed as failed folder and will not be installed
                    if (!rootData.hasModChild)
                    {
                        rootData.type = entryData.entryType.failed;
                    }
                    rootEntryDatas.Add(rootData);
                }
            }
        }
        catch (Exception e)
        {
            G.g.m.DisplayErrorMessage(e.Message);
            Debug.LogException(e);
        }
    }

    public void DisplayEntryDatas()//creating gameobject to display the entry datas on the UI 
    {
        foreach (entryData ed in rootEntryDatas)
            ed.createDisplayer(null);
    }

    public entryData SearchFolder(string path)//recursively convert a folder and it's entries into entry datas if they contain mods directly or indirectly
    {
        //if this is not a directory, exit
        if (!FileBrowserHelpers.DirectoryExists(path))
            return null;

        //searches for .dcx files. if finds a folder, searches in the folder
        //adds the found dcx file entries to a list 

        //the entry data if this entry
        entryData myData = new entryData
        {
            type = entryData.entryType.folder,
            name = FileBrowserHelpers.GetFilename(path),
            path = path
        };

        FileSystemEntry[] entries = FileBrowserHelpers.GetEntriesInDirectory(path, false);
        //if has child entries, create a list for them
        if (entries.Length > 0)
            myData.children = new List<entryData>();
        foreach (FileSystemEntry entry in entries)
        {

            //the entry data of this entry
            entryData childData = null;

            //creating the recently defined childdata
            if (entry.IsDirectory)
            {
                childData = SearchFolder(entry.Path);
                if (childData.hasModChild)
                    myData.hasModChild = true;
            }
            else
            {
                //if this is any kind of mod, create a mod displayer and set the "has mod child" variable
                //example extension: .partsbnd.dcx.disabled
                //the extensions array contains an unintentional empty string because of the first dot
                string[] extensions = GetExtensions(entry);
                if (extensions.Contains("dcx"))
                {
                    //creating a new entry data based on this mod file
                    childData = new entryData
                    {
                        name = entry.Name,
                        type = entryData.entryType.mod,
                        path = entry.Path
                    };

                    myData.hasModChild = true;
                    //if this is a parts mod, mark this folder as direct parent of part mods
                    //this bool decides if this folder must be named "parts" or not
                    if (extensions.Contains("partsbnd"))
                        myData.hasPartsModsDirectly = true;
                }
            }

            //if the entry is a mod, or contains mod, it will be added to the entry datas, otherwise not
            if (childData != null && (childData.hasModChild || childData.type == entryData.entryType.mod))
                myData.children.Add(childData);
        }

        //if this folder has mods directly inside it, and its name is "parts" then this is a parts folder
        if (myData.hasPartsModsDirectly)
        {
            if (myData.name == "parts")
                myData.subType = entryData.SubType.parts_folder;
        }

        return myData;
    }

    public void WriteLocalPathAll()
    {
        foreach (var ed in rootEntryDatas)
            WriteLocalPath(ed, "");
    }
    //filling the localpath variable
    public void WriteLocalPath(entryData ed, string localPath)
    {
        ed.localPath = Path.Combine(localPath, ed.name);
        if (ed.type == entryData.entryType.folder)
        {
            if (ed.children != null)
                foreach (var child in ed.children)
                    WriteLocalPath(child, ed.localPath);
        }
    }

    public static string[] GetExtensions(FileSystemEntry entry)
    {
        return entry.Extension.Split('.');
    }
}
