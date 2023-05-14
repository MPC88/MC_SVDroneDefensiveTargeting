using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using BepInEx.Logging;

namespace MC_SVDroneDefensiveTargeting
{
	[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
	public class Main : BaseUnityPlugin
	{
		public const string pluginGuid = "mc.starvalor.dronedefensivetargeting";
		public const string pluginName = "SV Drone Defensive Targeting";
		public const string pluginVersion = "1.0.2";

		private static ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource(pluginName);
		public void Awake()
		{
			Harmony.CreateAndPatchAll(typeof(Main));
		}

        [HarmonyPatch(typeof(Drone), "FindNewTarget")]
        [HarmonyPostfix]
		private static void Drone_FNTPost(Drone __instance, int mode, ref Entity ___targetEntity)
        {
			if (__instance == null ||
				__instance.owner == null ||
				!__instance.owner.gameObject.CompareTag("Player"))
				return;

			if (__instance.ownerSS == null)
				__instance.ownerSS = __instance.owner.GetComponent<SpaceShip>();
			if (__instance.ownerSS == null)
				return;

			Vector3 ownerPos = __instance.owner.position;
			Collider[] array = Physics.OverlapSphere(ownerPos, 200f, 512);
			Transform x = null;
			if (array.Length > 0)
			{
				array = array.OrderBy(c => (ownerPos - c.transform.position).sqrMagnitude).ToArray();				
				float num = 9999f;
				for (int i = 0; i < array.Length; i++)
				{
					Transform transform = array[i].transform;

					SpaceShip spaceShip = null;
					if (transform != null)
					{
						if (transform.CompareTag("Collider"))
						{
							ColliderControl cc = transform.GetComponent<ColliderControl>();
							if (cc != null)
								spaceShip = (cc.ownerEntity as SpaceShip);
						}
						else
							spaceShip = transform.GetComponent<SpaceShip>();
					}

					if (spaceShip != null)
					{
						transform = spaceShip.transform;

						if (transform != null)
						{
							if ((mode == 1 && spaceShip != __instance.ownerSS && __instance.ownerSS.ffSys != null && __instance.ownerSS.ffSys.TargetIsEnemy(spaceShip.ffSys) && !spaceShip.IsCloaked))
							{
								float num2 = Vector3.Distance(__instance.gameObject.transform.position, transform.position);
								if (num2 < num)
								{
									num = num2;
									x = transform;
								}
							}
						}
					}
				}
			}

			if (x != null)
			{
				__instance.target = x;
				___targetEntity = __instance.target.GetComponent<Entity>();
				AccessTools.Method(typeof(Drone), "GetDesiredDistance").Invoke(__instance, null);
			}
			else
            {
				__instance.target = null;
				___targetEntity = null;
			}
		}
	}
}
