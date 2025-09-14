using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Hypn;
using Photon.Pun;

namespace Hypn.Patches
{
    [HarmonyPatch(typeof(PlayerDeathHead), "Update")]
    internal class HypnReviveInTruckAndExtractionPatch
    {
        private class ReviveInfo
        {
            public int Count;
            public DateTime LastReviveTime;
            public float OutsideLevelTimer;
        }

        private static readonly Dictionary<string, ReviveInfo> _reviveTracker = new();
        private static readonly object _reviveLock = new();
        private const int ReviveCooldownSeconds = 5;

        static void Postfix(PlayerDeathHead __instance)
        {
            // skip if not multiplayer host (likely doesn't work in singleplayer anyway)
            if (!SemiFunc.IsMasterClientOrSingleplayer())
            {
                // Plugin.Logger.LogInfo($"{Plugin.ModName}: not multiplayer host - not reviving!");
                return;
            }

            // skip if we don't have a usable instance/player
            var avatar = __instance?.playerAvatar;
            if (avatar == null)
            {
                // Plugin.Logger.LogInfo($"{Plugin.ModName}: no playerAvatar - not reviving!");
                return;
            }

            // track players by Steam Id
            string steamID = avatar.steamID;
            if (string.IsNullOrEmpty(steamID))
            {
                // Plugin.Logger.LogInfo($"{Plugin.ModName}: no steamID - not reviving!");
                return;
            }

            // skip if not dead
            bool triggered = __instance?.triggered ?? false;
            if (!triggered)
            {
                // Plugin.Logger.LogInfo($"{Plugin.ModName}: not triggered (dead?) - not reviving!");
                return;
            }

            // skip if not in a valid revive location
            bool inTruck = __instance?.roomVolumeCheck?.inTruck ?? false;
            if (!inTruck)
            {
                // Plugin.Logger.LogInfo($"{Plugin.ModName}: not in Truck - not reviving!");
                return;
            }

            // prevent race conditions (resulting in extra health being given)
            lock (_reviveLock)
            {
                _reviveTracker.TryGetValue(steamID, out var info);
                if (info != null)
                {
                    int revivedSecondsAgo = (int)(DateTime.UtcNow - info.LastReviveTime).TotalSeconds;
                    if (revivedSecondsAgo < ReviveCooldownSeconds)
                    {
                        // Plugin.Logger.LogInfo($"{Plugin.ModName}: revived too recently ({revivedSecondsAgo} seconds ago) - not reviving!");
                        return;
                    }
                }

                int reviveHealth = (int)(avatar.playerHealth.maxHealth * 0.20); // revive with 20% health
                Plugin.Logger.LogInfo($"{Plugin.ModName}: reviving player \"{avatar.playerName}\" (steamID={steamID}) with {reviveHealth}/{avatar.playerHealth.maxHealth} health (revives: {info?.Count ?? 0})");
                avatar.Revive(inTruck);
                avatar.playerHealth.HealOther(reviveHealth - 1, true);

                // update info of when the player was last revived
                if (info == null)
                {
                    info = new ReviveInfo
                    {
                        Count = 1,
                        LastReviveTime = DateTime.UtcNow,
                        OutsideLevelTimer = 0f
                    };
                    _reviveTracker[steamID] = info;
                    // Plugin.Logger.LogInfo($"{Plugin.ModName}: setting revive info for \"{avatar.playerName}\" (steamID={steamID}) for the first time (revives: {info.Count})");
                }
                else
                {
                    info.Count++;
                    info.LastReviveTime = DateTime.UtcNow;
                    // Plugin.Logger.LogInfo($"{Plugin.ModName}: updating revive info for \"{avatar.playerName}\" (steamID={steamID}) (revives: {info.Count})");
                }

                // prevent the player from getting stuck in the void when respawning
                if ((__instance?.roomVolumeCheck?.CurrentRooms?.Count ?? 1) <= 0)
                {
                    info.OutsideLevelTimer += Time.deltaTime;
                    if ((__instance?.physGrabObject != null) && info.OutsideLevelTimer >= 5f)
                    {
                        if (inTruck)
                        {
                            Plugin.Logger.LogInfo($"{Plugin.ModName}: teleporting player \"{avatar.playerName}\" (steamID={steamID}) out of the void, to truck");
                            __instance.physGrabObject.Teleport(TruckSafetySpawnPoint.instance.transform.position, TruckSafetySpawnPoint.instance.transform.rotation);
                        }
                        else
                        {
                            Plugin.Logger.LogInfo($"{Plugin.ModName}: teleporting player \"{avatar.playerName}\" (steamID={steamID}) out of the void, to extraction point");
                            __instance.physGrabObject.Teleport(RoundDirector.instance.extractionPointCurrent.safetySpawn.position, RoundDirector.instance.extractionPointCurrent.safetySpawn.rotation);
                        }
                    }
                    else
                    {
                        Plugin.Logger.LogInfo($"{Plugin.ModName}: wanted to teleport player \"{avatar.playerName}\" (steamID={steamID}) out of the void, but did not have a `physGrabObject` :/");
                    }
                }
                else
                {
                    info.OutsideLevelTimer = 0f;
                }
            }
        }
    }
}