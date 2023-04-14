using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;

namespace MC_SVDroneDefensiveTargeting
{
	[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
	public class Main : BaseUnityPlugin
	{
		public const string pluginGuid = "mc.starvalor.dronedefensivetargeting";
		public const string pluginName = "SV Drone Defensive Targeting";
		public const string pluginVersion = "1.0.1";

		public void Awake()
		{
			Harmony.CreateAndPatchAll(typeof(Main));
		}

        [HarmonyPatch(typeof(Drone), "FindNewTarget")]
        [HarmonyPostfix]
		private static void Drone_FNTPost(Drone __instance, int mode, ref Entity ___targetEntity)
        {
			if (__instance.owner == null)
				return;

			if (!__instance.owner.gameObject.CompareTag("Player"))
				return;

			if (__instance.ownerSS == null)
				__instance.ownerSS = __instance.owner.GetComponent<SpaceShip>();

            Collider[] array = Physics.OverlapSphere(__instance.owner.position, 200f, 512);
			array = array.OrderBy(c => (__instance.owner.position - c.transform.position).sqrMagnitude).ToArray();
			Transform x = null;
			float num = 9999f;
			for (int i = 0; i < array.Length; i++)
			{
				Transform transform = array[i].transform;
				SpaceShip spaceShip;
				if (transform.CompareTag("Collider"))
					spaceShip = (transform.GetComponent<ColliderControl>().ownerEntity as SpaceShip);
				else
					spaceShip = transform.GetComponent<SpaceShip>();

				transform = spaceShip.transform;
				if ((mode == 1 && spaceShip != __instance.ownerSS && __instance.ownerSS.ffSys.TargetIsEnemy(spaceShip.ffSys) && !spaceShip.IsCloaked))
				{
					float num2 = Vector3.Distance(__instance.gameObject.transform.position, transform.position);
					if (num2 < num)
					{
						num = num2;
						x = transform;
					}
				}
			}

			if (x != null)
			{
				__instance.target = x;
				___targetEntity = __instance.target.GetComponent<Entity>();
				AccessTools.Method(typeof(Drone), "GetDesiredDistance").Invoke(__instance, null);
			}
		}
	}
}
