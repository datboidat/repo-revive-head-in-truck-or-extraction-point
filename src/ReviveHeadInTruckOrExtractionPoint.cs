using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Hypn.Patches;
using BepInEx.Configuration;

namespace Hypn;

[BepInPlugin("Hypn.ReviveHeadInTruckOrExtractionPoint", "ReviveHeadInTruckOrExtractionPoint", "1.2.0")]
public class Plugin : BaseUnityPlugin
{

    public const string ModGUID = "Hypn.ReviveHeadInTruckOrExtractionPoint";
    public const string ModName = "ReviveHeadInTruckOrExtractionPoint";
    public const string ModVersion = "1.2.0";
    private readonly Harmony harmony = new Harmony(ModGUID);

    internal static Plugin Instance { get; private set; } = null!;
    public static new ManualLogSource Logger { get; private set; } = null!;

    private void Awake()
    {
        Instance = this;

        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        Logger = BepInEx.Logging.Logger.CreateLogSource(ModGUID);
        Logger.LogInfo($"Plugin {ModName} v{ModVersion} is loaded!");

        harmony.PatchAll();
    }

    internal void Unpatch()
    {
        harmony?.UnpatchSelf();
    }
}
