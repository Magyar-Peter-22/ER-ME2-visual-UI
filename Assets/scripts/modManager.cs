using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;

public class modManager : fileDisplayerBase
{
    //if a folder has one of these folders in it, the it will be marked as toml folder and will be added to eldenring_config.toml when selected in the mod manager
    private static List<string> tomlSubFolders = new List<string>() { "parts", "msg", "script", "chr" };
    public List<string> tomlEntries;
    public class profile
    {
        [System.NonSerialized]
        public int id;
        public string name;
        public List<string> enabled;
    }
    public profile selectedProfile;
    public bool allEnabledSelected{
        get{
            return GetSelectedProfileId()==0;
        }
    }
    public profile[] profiles;
    private List<string> selected;
    private const string profilePrefix = "profile";
    private const string selectedProfilePP = "selectedProfile";
    public Dropdown profileDropdown;
    private const int profileCount = 10;
    public InputField rename;
    public Button reset;
    public Button save;
    public Button load;
    public GameObject notifyAllEnabled;
    public TextAsset defaultToml;
    private int GetSelectedProfileId()
    {
        return profileDropdown.value;
    }
    public override void OnEnable()
    {
        base.OnEnable();

        //if modlaoder not found, error
        if (!G.g.mlf.ok)
        {
            G.g.m.DisplayErrorMessage("modloader2 must be located in the 'find modloader2' tab before viewing the installed mods");
            return;
        }

        //find all direct child of the mod folder
        #region find paths
        FileSystemEntry[] found = FileBrowserHelpers.GetEntriesInDirectory(G.g.mlf.modFolderPath, false);
        string[] paths = new string[found.Length];
        for (int n = 0; n < found.Length; n++)
            paths[n] = found[n].Path;
        #endregion

        CleanUp();
        PathsToEntryDatas(paths);
        WriteLocalPathAll();
        WriteTomlDataAll();
        DisplayEntryDatas();
        LoadProfiles();

        //load the selected profile from playerprefs
        profileDropdown.value = PlayerPrefs.GetInt(selectedProfilePP);
        profileDropdown.RefreshShownValue();

        SelectProfile();
    }
    //add the names to the profile dropdown. if save found, add the saved name, if not, add the default
    void LoadProfiles()
    {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        profiles = new profile[profileCount];

        options.Add(new Dropdown.OptionData { text = "All Enabled" });

        for (int n = 0; n < profileCount; n++)
        {
            string loaded = PlayerPrefs.GetString(profilePrefix + n);
            profile loadedProfile;
            try
            {
                if (loaded == "")
                    throw new System.Exception("empty playerpref");
                loadedProfile = (profile)JsonUtility.FromJson(loaded, typeof(profile));
                loadedProfile.id = n;
            }
            catch
            {
                loadedProfile = GetDefaultProfile(n);
            }
            profiles[n] = loadedProfile;
            options.Add(new Dropdown.OptionData { text = loadedProfile.name });
        }

        profileDropdown.options = options;
        profileDropdown.RefreshShownValue();
    }
    private profile GetDefaultProfile(int id)
    {
        profile result = new profile
        {
            name = "Profile " + id,
            id = id
        };
        return result;
    }
    public void ResetProfile()
    {
        G.g.m.Confirm("do you want to reset all values in the selected profile to default? the current values cannot be recovered after this", ResetConfirmed);
    }
    public void ResetConfirmed()
    {
        selectedProfile = GetDefaultProfile(selectedProfile.id);
        rename.text = selectedProfile.name;
        SetAllEntry(false);
    }
    //select a profile and set the enabled status of the entries 
    public void SelectProfile()
    {
        int id = profileDropdown.value;

        //the 0 id marks the all enabled profile, a special profile that do no exist in the profile array
        if (id == 0)
        {
            selectedProfile = null;
            SetAllEntry(true, false);
            rename.text = profileDropdown.options[0].text;
            AllEnabled(true);
        }
        else
        {
            selectedProfile = profiles[id - 1];//the id needs to be offsetted because of the all enabled profile
            if (selectedProfile.enabled != null)
            {
                foreach (var ed in rootEntryDatas)
                    SetStatusFromProfile(ed);
            }
            else
                SetAllEntry(false);
            rename.text = selectedProfile.name;
            AllEnabled(false);
        }
    }
    //sets the interactable status of buttons if the all enabled option is selected
    void AllEnabled(bool b)
    {
        save.interactable = !b;
        load.interactable = !b;
        rename.interactable = !b;
        reset.interactable = !b;
        notifyAllEnabled.SetActive(b);
    }
    void SetStatusFromProfile(entryData ed)
    {
        //if the enabled entries of the selected profile contains this, set the toggle
        bool enable = selectedProfile.enabled.Contains(ed.localPath) || (ed.type == entryData.entryType.mod && selectedProfile.enabled.Contains(ed.getAlternativeExtension(ed.localPath)));
        //if mod, search for the disabled or not disabled alternative name

        //the enabled status of the mods is inverted when loading from profile, so they are enabled by default
        if (ed.type == entryData.entryType.mod)
            enable = !enable;
        SetEntry(ed, enable, false);

        //process childs
        if (ed.children != null)
            foreach (var child in ed.children)
                SetStatusFromProfile(child);
    }
    //set the enabled status of all entries
    public void SetAllEntry(bool enabled, bool interactable = true)
    {
        foreach (var ed in rootEntryDatas)
            SetEntry(ed, enabled, true, interactable);
    }

    //return true if the toggle of this entry cannot be edited
    bool CannotSetToggle(entryData ed)
    {
        return ed.display == null || ed.type == entryData.entryType.failed || ed.display.toggle.interactable == false;
    }

    //sets the enabled status of entries recursively
    void SetEntry(entryData ed, bool enabled, bool recursive, bool interactable = true)
    {
        if (ed.display == null)
            return;

        ed.display.ResetInteractable();

        if (!CannotSetToggle(ed))
        {
            //set the toggle
            ed.display.toggle.isOn = enabled;
            ed.display.toggle.interactable = interactable;
        }

        //process childs
        if (recursive && ed.children != null)
            foreach (var child in ed.children)
                SetEntry(child, enabled, recursive, interactable);
    }
    void WriteTomlDataAll()
    {
        foreach (var ed in rootEntryDatas)
            WriteTomlData(ed, false);
    }
    //recursively mark the toml folders and their descendants
    void WriteTomlData(entryData ed, bool descendantOfTomlFolder)
    {
        //if any parent folder is toml folder, mark this folder
        ed.descendantOfToml = descendantOfTomlFolder;

        //searching for toml subfolders, find find one, mark this folder as toml folder
        if (ed.children != null)
            foreach (var child in ed.children)
            {
                if (child.type == entryData.entryType.folder && tomlSubFolders.Contains(child.name))
                {
                    ed.subType = entryData.SubType.toml_folder;
                    break;
                }
            }

        //process the subfolders
        if (ed.subType == entryData.SubType.toml_folder)
            descendantOfTomlFolder = true;
        if (ed.children != null)
            foreach (var child in ed.children)
            {
                WriteTomlData(child, descendantOfTomlFolder);
            }
    }
    //save the enabled status of the entries and the name to the profile
    public void Save()
    {
        G.g.m.Confirm("do you want to overwrite this this profile?", SaveConfirmed);
    }
    public void SaveConfirmed()
    {
        if (!allEnabledSelected)
        {
            //listing the enabled entries 
            selected = new List<string>();
            foreach (var ed in rootEntryDatas)
                SaveEntry(ed);

            selectedProfile.enabled = selected;
            selectedProfile.name = rename.text;
            profiles[selectedProfile.id] = selectedProfile;//because the reset button creates a new selectedprofile
            profileDropdown.options[selectedProfile.id + 1].text = selectedProfile.name;
            profileDropdown.RefreshShownValue();
            PlayerPrefs.SetString(profilePrefix + selectedProfile.id, JsonUtility.ToJson(selectedProfile));
        }
    }
    //recursively add the localpath of the enabled entries to the selected array
    void SaveEntry(entryData ed)
    {
        //the save of the mods is inverted
        if (ed.display != null && (ed.display.IsEnabled() && ed.type != entryData.entryType.mod || !ed.display.IsEnabled() && ed.type == entryData.entryType.mod))
        {
            selected.Add(ed.localPath);
            if (ed.children != null)
                foreach (var child in ed.children)
                    SaveEntry(child);
        }
    }

    //the activate profile button is pressed
    //update eldenring_config.toml based on the seelcted profile
    public void WriteToml()
    {
        try
        {
            //requies modloader
            if (!G.g.mlf.ok)
            {
                G.g.m.DisplayErrorMessage("modloader2 must be located in the 'find modloader2' tab before you can use this");
                return;
            }

            SaveConfirmed();

            #region toml file
            //write array of toml rows
            tomlEntries = new List<string>();
            foreach (var ed in rootEntryDatas)
                EntryDataToToml(ed);

            //read the toml file
            string tomlFile = FileBrowserHelpers.ReadTextFromFile(G.g.mlf.tomlFilePath);

            string startString = "\nmods = [";
            int start = tomlFile.IndexOf(startString);
            start += startString.Length;

            string endString = "\n]";
            int end = tomlFile.IndexOf(endString);

            if (start == -1 || end == -1 || end < start)
                throw new Exception("the \"mods\" section cannot be found. reset the toml file with the ...");

            string modsString = "\n";
            for (int n = 0; n < tomlEntries.Count; n++)
            {
                modsString += tomlEntries[n];
                if (n < tomlEntries.Count - 1)
                    modsString += ",\n";
            }

            string newTomlFile = tomlFile.Substring(0, start) + modsString + tomlFile.Substring(end, tomlFile.Length - end);
            FileBrowserHelpers.WriteTextToFile(G.g.mlf.tomlFilePath, newTomlFile);
            #endregion

            #region mod disabler extension
            foreach (var ed in rootEntryDatas)
                ApplyModDisablerExtensions(ed);
            #endregion

            PlayerPrefs.SetInt(selectedProfilePP, GetSelectedProfileId());
            G.g.m.DisplayOkMessage("profile activated successfully");
        }
        catch (Exception e)
        {
            G.g.m.DisplayErrorMessage(e.Message);
            Debug.LogException(e);
        }
    }

    //recursively convert the enabled toml folders into toml data rows
    void EntryDataToToml(entryData ed)
    {
        //if this entry is disabled or deleted or does not contains mods, exit
        if (ed.display == null || !ed.display.IsEnabled())
            return;

        //if this is a toml folder, create a data row 
        if (ed.subType == entryData.SubType.toml_folder)
        {
            string path = Path.Combine(modloaderFinder.modFolderName, ed.localPath);
            path = path.Replace('\\', '/');
            tomlEntries.Add("{ enabled = true, name = \"" + ed.name + "\", path = \"" + path + "\" }");
        }

        //processing the subfolders
        if (ed.children != null)
            foreach (var child in ed.children)
                EntryDataToToml(child);
    }

    //adds or removes the .disabled extension from the mod files depending on their checkbox
    void ApplyModDisablerExtensions(entryData ed)
    {
        if (ed.display == null)
            return;

        if (ed.type == entryData.entryType.mod)
            ed.modSetActive(ed.display.IsEnabled());

        //processing the subfolders
        if (ed.children != null && ed.display.IsEnabled())
            foreach (var child in ed.children)
                ApplyModDisablerExtensions(child);
    }

    public void ResetModengineConfig()
    {
        G.g.m.Confirm("do you want to reset \"config_eldenring.toml\" in modengine2 to it's starting value?", ResetModengineConfigConfirmed);
    }
    public void ResetModengineConfigConfirmed()
    {
        try
        {
            FileBrowserHelpers.WriteTextToFile(G.g.mlf.tomlFilePath, defaultToml.text);
            G.g.m.DisplayOkMessage("modloader2 config file resetted successfully");
        }
        catch (Exception e)
        {
                        Debug.LogException(e);
            G.g.m.DisplayErrorMessage(e.Message);
        }
    }
}
