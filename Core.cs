// These references are "must haves" for any project using LMUI.
using LMUI;
using LMUI.API;
using MelonLoader;
using UnityEngine;
// I'm referencing the LMUI wrapper to get
// access to the game's existing UI objects
// such as the menu layout GameObjects.
using static LMUI.CoreShell;

[assembly: MelonInfo(typeof(LMDModMenu.Core), "LMD Mod Menu", "1.0.1", "Jcau8", null)]
[assembly: MelonGame("Megagon Industries", "Lonely Mountains: Downhill")]

namespace LMDModMenu
{
    public class Core : MelonMod
    {
        // Initialising the classes I'm using
        Button _button = new();
        Menu.GameMenu _gameMenu = new();
        Layout _layout = new();
        LMUI.API.AssetBundle _assetBundle = new();
        Prefab _prefab = new();

        // Bools to store whether or not menus have been created already.
        // Need these otherwise I'd be creating lots of duplicates.
        bool _allModMenuBtnsCreated;
        bool _modMenuScreenCreated;

        // Variables to store the various game objects.
        GameObject _modMenuBtn;
        GameObject _testButtonPrefab;
        GameObject _modMenuLayout;
        Dictionary<string, GameObject> _modInfoLayouts = new();
        Dictionary<string, GameObject> _modButtons = new();

        // Stores the active menu screen.
        CoreShell.MenuScreen _activeScreen;
        // There are 3 menu screens in the enum:
        //     MainMenu
        //     PauseMenu
        //     SettingsMenu

        // Grabs all the mods in the mods folder (unused).
        public List<string> GetAllModsViaDir()
        {
            string[] files = Directory.GetFiles(@"C:\Program Files (x86)\Steam\steamapps\common\Lonely Mountains - Downhill\Mods\");
            List<string> cleanedFiles = new();

            foreach (string file in files)
            {
                cleanedFiles.Add(file.Replace("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Lonely Mountains - Downhill\\Mods\\", "").Replace(".dll", ""));
            }

            return cleanedFiles;
        }

        // Gets the active mods directly from melon loader.
        public List<MelonMod> GetModsViaML()
        {
            List<MelonMod> mods = new();
            foreach (var mod in MelonMod.RegisteredMelons)
            {
                mods.Add(mod);
            }
            return mods;
        }

        public void OnModMenuClick()
        {
            // Sets the active screen to current game menu then disables it.
            // Need this for returning to the previous menu.
            _activeScreen = _gameMenu.GetActiveMenu();
            _gameMenu.Disable(_activeScreen);

            // Checks whether the mod menu has been created.
            if (!_modMenuScreenCreated)
            {
                // If the layout which I'm using for the
                // mod menu doesn't exist then create it.
                if (_modMenuLayout == null)
                {
                    // Duplicates the main menu's layout and
                    // instantiates it under a new name and parent.
                    _modMenuLayout = _layout.CreateDuplicate(CoreShell.MenuScreen.MainMenu, "ActiveModMenuListLayout");
                }

                // Gets the active mods and creates things for each
                foreach (MelonMod mod in GetModsViaML())
                {
                    // Stores the name of the mod and the test button
                    // prefab with the game's settings image applied.
                    // Just to make the code slightly cleaner.
                    string modName = mod.Info.Name;
                    GameObject modifiedTestPrefab = Instance.StealGameSettingImage(_testButtonPrefab);

                    // Create a new button in the mod menu that will
                    // open information about the mod when clicked.
                    // This happens to all buttons and they get added to a dictionary.
                    // It's not necessary but is useful for keeping track of them.
                    _modButtons.Add(modName, _button.Create(_testButtonPrefab, _modMenuLayout, () => OpenModInfo(modName), optionalText: modName, useGameMenuStyle: true));
                    // Create a new layout, here I'm using the CoreShell instance
                    // to get access to the settings layout game object. Not the 
                    // one which contains the buttons for the different settings menus,
                    // but the one that actually contains the individual settings inside of it.

                    // I'm then adding it to a dictionary, so I can keep track of it
                    // and enable/disable it later on.
                    _modInfoLayouts.Add(modName, _layout.CreateDuplicate(Instance.SettingsEnumLayout, modName));

                    // Creates 3 buttons to display the information about the mod.
                    // These don't need to be buttons, but this was the easiest way.
                    _button.Create(modifiedTestPrefab, _modInfoLayouts[modName], VoidOnClick, optionalText: "Version: " + mod.Info.Version);
                    _button.Create(modifiedTestPrefab, _modInfoLayouts[modName], VoidOnClick, optionalText: "Creator: " + mod.Info.Author);
                    _button.Create(modifiedTestPrefab, _modInfoLayouts[modName], VoidOnClick, optionalText: "Download Link: " + mod.Info.DownloadLink);

                    // Disbale the information layout so we don't have multiple layered over each other.
                    _modInfoLayouts[modName].SetActive(false);
                }
                // Create the back button.
                _button.Create(_testButtonPrefab, _modMenuLayout, BackToStoredScreen, optionalText: "Back", useGameMenuStyle: true);

                // Make sure the mod menu doesn't get recreated.
                _modMenuScreenCreated = true;
            }
            // Enable the mod menu.
            _modMenuLayout.SetActive(true);
        }

        public void VoidOnClick()
        {
            // Nothing to see here.
        }

        // Enables the corresponding layout of information
        // depending on which mod was clicked.
        public void OpenModInfo(string modName)
        {
            // Goes through all the layouts and compares
            // the key to the mod that was clicked.
            foreach (var layout in _modInfoLayouts)
            {
                //If they are the same, then enable that layout.
                if (modName == layout.Key)
                {
                    layout.Value.SetActive(true);
                }
                else // If they aren't, then disable it.
                {
                    layout.Value.SetActive(false);
                }
            }
        }

        public void BackToStoredScreen()
        {
            // Checks which game menu was last active
            switch (_activeScreen)
            {
                case CoreShell.MenuScreen.MainMenu:
                    // Disables any custom menu that may be active
                    _modMenuLayout.SetActive(false);
                    foreach (var layout in _modInfoLayouts)
                    {
                        layout.Value.SetActive(false);
                    }
                    // Enables the last active game menu
                    _gameMenu.Enable(CoreShell.MenuScreen.MainMenu);
                    break;

                case CoreShell.MenuScreen.PauseMenu:
                    _modMenuLayout.SetActive(false);
                    foreach (var layout in _modInfoLayouts)
                    {
                        layout.Value.SetActive(false);
                    }
                    _gameMenu.Enable(CoreShell.MenuScreen.PauseMenu);
                    break;

                case CoreShell.MenuScreen.SettingsMenu:
                    _modMenuLayout.SetActive(false);
                    foreach (var layout in _modInfoLayouts)
                    {
                        layout.Value.SetActive(false);
                    }
                    _gameMenu.Enable(CoreShell.MenuScreen.SettingsMenu);
                    break;

                default:
                    break;
            }
        }

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialising...");

            // Load the asset bundle which contains my test button prefab
            UnityEngine.AssetBundle testbundle = _assetBundle.Load("testbundle", "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Lonely Mountains - Downhill\\AssetBundles\\");
            // Load the prefab into a variable
            _testButtonPrefab = _prefab.Load(testbundle, "assets/prefabs/testbutton.prefab");

            _allModMenuBtnsCreated = false;

            // Initialise the CoreShell wrapper. This creates
            // a CoreShell instance which allows us to access
            // the game's UI objects for duplication/manipulation.
            CoreShell.Init();
            // Most use cases of LMUI shouldn't require this. I
            // plan on adding more and easier accessibility to these
            // properties in the future. If I return to LMUI, that is.
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            // Checks if everything has been created.
            if (!_allModMenuBtnsCreated)
            {
                foreach (CoreShell.MenuScreen menu in Enum.GetValues(typeof(CoreShell.MenuScreen)))
                {
                    // For every game menu, check if it exists.
                    if (_gameMenu.Exists(menu))
                    {
                        // If it exists, then it is safe to create
                        // a mod menu button inside that menu.
                        _modMenuBtn = _button.Create(_testButtonPrefab, menu, OnModMenuClick, optionalText: "Mod Menu", useLayout: true, useGameMenuStyle: true);
                    }
                }

                if (_modMenuBtn != null)
                {
                    // Make sure the UI is only created once
                    _allModMenuBtnsCreated = true;
                }
            }
        }
    }
}