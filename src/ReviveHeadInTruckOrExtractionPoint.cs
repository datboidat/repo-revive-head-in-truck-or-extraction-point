using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Hypn.Patches;
using BepInEx.Configuration;

namespace Hypn;

[BepInPlugin("Hypn.ReviveHeadInTruckOrExtractionPoint", "ReviveHeadInTruckOrExtractionPoint", "0.0.1")]
public class Plugin : BaseUnityPlugin
{

    public const string modGUID = "Hypn.ReviveHeadInTruckOrExtractionPoint";
    public const string modName = "ReviveHeadInTruckOrExtractionPoint";
    public const string modVersion = "0.0.1";
    private readonly Harmony harmony = new Harmony(modGUID);

    internal static Plugin Instance { get; private set; } = null!;

    private void Awake()
    {
        Instance = this;

        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        harmony.PatchAll();
    }

    internal void Unpatch()
    {
        harmony?.UnpatchSelf();
    }
}