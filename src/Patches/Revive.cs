using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Hypn;
using Photon.Pun;

namespace Hypn.Patches
{
	[HarmonyPatch(typeof(PlayerDeathHead), "Update")]
	class HypnReviveInTruckAndExtraction
	{
		static void Postfix(PlayerDeathHead __instance)
		{
			int reviveHealth = 20;

			// head is in extraction point
			if (__instance.triggered && __instance.roomVolumeCheck.inExtractionPoint)
			{
				__instance.playerAvatar.Revive(false);
				__instance.playerAvatar.playerHealth.HealOther(reviveHealth-1, true);
			}

			// head is in truck
			if (__instance.triggered && __instance.roomVolumeCheck.inTruck) {
				__instance.playerAvatar.Revive(true);
				__instance.playerAvatar.playerHealth.HealOther(reviveHealth-1, true);
			}
		}
	}
}
