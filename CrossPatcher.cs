using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using Portningsbolaget.Platforms;
using Portningsbolaget.Utilities;
using Unity.Mathematics;
using UnityEngine;
using Zorro.Settings;

// ExampleCWPlugin is mostly an example of how to use the modding API, not an actual serious mod.
// It adds a setting to the Mods settings page, which is a slider from 0 to 100.
// It then edits the Flashlight.Update method to prevent the battery of the flashlight
// from falling below that setting value.

namespace CrossPatcher;

// The first argument is the GUID for this mod. This must be globally unique across all mods.
// Consider prefixing your name/etc. to the GUID. (or generate an actual GUID)

// vanillaCompatible: False means that this mod affects anything related to the multiplayer aspect of the game - for
// example, adjusting sprint regen, battery life, changing monster behavior, or anything similar or more major. Most
// mods leave this as false. 
// True means that this mod only affects this client. For example, a mod that changes the folder your clips are saved at.
// A good rule of thumb is: If someone else can tell you're using this mod, you must set vanillaCompatible to false.
// If you set this to true, and it should be false, your mod may be removed/banned from the workshop.
[ContentWarningPlugin("CrossPatcher", "0.1", vanillaCompatible: true )]
public class CrossPatcher
{
    static CrossPatcher()
    {
        // Static constructors of types marked with ContentWarningPluginAttribute are automatically invoked on load.
        // Register callbacks, construct stuff, etc. here.
        Debug.Log("Hello from CrossPatcher! This is called on plugin load");
        // Adding the [ContentWarningSetting] attribute to a setting class is basically the same as:
        // GameHandler.Instance.SettingsHandler.AddSetting(new ExampleSetting());
    }
}

[HarmonyPatch(typeof(MainMenuHandler))]
public class MainMenuHandlerPatches
{
    [HarmonyPatch(nameof(MainMenuHandler.OnLobbyHosted))]
    [HarmonyPrefix]
    private static bool OnLobbyHostedPrefix(MainMenuHandler __instance, ulong obj, bool privateMatch)
    {
        string localizedString = LocalizationKeys.GetLocalizedString(LocalizationKeys.Keys.Error_CreateRoom);
        if (obj == 0L)
        {
            Debug.LogError("Something went wrong hosting the Lobby, aborting Photon Create Room");
            Modal.ShowError(localizedString, "");
            RetrievableSingleton<ConnectionStateHandler>.Instance.Disconnect();
            return false;
        }
        Debug.Log($"Lobby Hosted: {obj} Hosting PhotonRoom Now");
        LanguageMatchmakingSetting.MatchmakingLanguage value = (LanguageMatchmakingSetting.MatchmakingLanguage)GameHandler.Instance.SettingsHandler.GetSetting<PreferredLanguageMatchmakingSetting>().Value;
        PlatformUtility.PlatformFamily platformFamily = (MainMenuHandler.CrossPlatform ? ((PlatformUtility.PlatformFamily)(-1)) : PlatformUtility.CurrentPlatform);
        Debug.Log(string.Format("Targeted Platform: {0}", (platformFamily < PlatformUtility.PlatformFamily.None) ? "All" : ((object)platformFamily)));
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = false;
        roomOptions.MaxPlayers = 4;
        roomOptions.Plugins = new string[1] { "ContentWarningPlugin" };
        //roomOptions.Plugins = new string[0] { };
        roomOptions.CustomRoomPropertiesForLobby = new string[4] { "P", "L", "PL", "M" };
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
        {
            {
                "C",
                obj.ToString()
            },
            {
                "P",
                privateMatch ? 1 : 0
            },
            { "L", value },
            {
                "PL",
                (int)platformFamily
            },
            {
                "M",
                "none"//GameHandler.GetPluginHash()
            }
        };
        __instance.m_Hosting = false;
        if (!PhotonNetwork.CreateRoom(Utilities.CreateRandomName(PhotonNetwork.CloudRegion, usingMods: false), roomOptions, TypedLobby.Default))
        {
            Debug.LogError("Error While Hosting Photon Room...");
            Modal.ShowError(localizedString, "");
            RetrievableSingleton<ConnectionStateHandler>.Instance.Disconnect();
            return false;
        }
        Debug.Log("Created Photon Room... ");
        if (!SaveSystem.HaveCurrentSave && SaveSystem.USING_SAVE)
        {
            SaveSystem.MakeNewSave();
        }

        return false;
    }
}