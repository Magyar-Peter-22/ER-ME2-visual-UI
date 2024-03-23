using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static fileDisplayerBase;


public class failed_display : displayBase
{
    //displays the path variable of the entrydata
    public Text path;


    public static displayBase CreateGO(entryData ed, folder_display parentFolder)
    {
        //finding the parent where the folder will be displayed
        //if it's a root entry, the parentFolder is null, so select the parent transform of the root folders from filehandler
        Transform parentTransform;
        if (parentFolder != null)
            parentTransform = parentFolder.subfoldersParent;
        else
            parentTransform = DefaultParentFolder;

        //creating the gameobject in it's parent transform
        GameObject instance = Instantiate(G.g.failedDisplayPref, parentTransform);

        //getting the displayer script and displaying this entry on it
        failed_display fd = instance.GetComponent<failed_display>();
        fd.SetupBasics(ed);

        //filling the data fields on the gameobject
        fd.entryName.text = "<b>Error: </b>The folder '"+ed.name+"' contains no mods, it will not be installed!";
        fd.path.text = "<b>Path: </b>"+ed.path;

        return fd;
    }

}
