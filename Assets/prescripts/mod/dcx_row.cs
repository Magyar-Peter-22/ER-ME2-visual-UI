using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static fileDisplayerBase;

public class dcx_row : displayBase
{
    public static displayBase CreateGO(entryData ed, folder_display parentFolder)
    {
        //finding the parent where the folder will be displayed
        //if it's a root entry, the parentFolder is null, so select the parent transform of the root folders from filehandler
        Transform parentTransform;
        if (parentFolder != null)
        {
            parentTransform = parentFolder.modsParent;
            //the mod displayer is disabled by default, it must be enabled to make the mod visible
            parentFolder.modDisplayer.gameObject.SetActive(true);
        }
        else
            parentTransform = DefaultParentFolder;


        //creating the gameobject in it's parent transform
        GameObject instance = Instantiate(G.g.modDisplayPref, parentTransform);

        //getting the displayer script and displaying this entry on it
        dcx_row mod = instance.GetComponent<dcx_row>();
        mod.SetupBasics(ed);

        return mod;
    }

}
