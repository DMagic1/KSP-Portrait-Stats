using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens.Flight;

namespace PortraitStats
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class PortraitHook : MonoBehaviour
	{
		public static readonly List<KerbalPortrait> PortraitList = new List<KerbalPortrait>();

		class PortraitTracker : MonoBehaviour
		{
			private KerbalPortrait _portrait;

			private void Start() // Awake might be too early
			{
				if (transform.GetComponentCached(ref _portrait) == null)
					Destroy(this);
				else AddPortrait(_portrait);
			}

			private void OnDestroy()
			{
				if (_portrait == null) return;

				RemovePortrait(_portrait);
			}
		}


		private void Awake()
		{
			var kpg = KerbalPortraitGallery.Instance;

			AddTracker(kpg.portraitPrefab);

			// uncertain whether KSPAddons created before KerbalPortraits initialized
			// pretty sure they are but too lazy to check
			kpg.gameObject.GetComponentsInChildren<KerbalPortrait>()
				.ToList()
				.ForEach(AddTracker);

			Destroy(gameObject);
		}

		// Might only need to edit the prefab once. This will make sure we don't add duplicates
		private static void AddTracker(KerbalPortrait portrait)
		{
			if (portrait.GetComponent<PortraitTracker>() != null) return;

			portrait.gameObject.AddComponent<PortraitTracker>();
		}


		private static void AddPortrait(KerbalPortrait portrait)
		{
			if (portrait == null) return;

			PortraitList.AddUnique(portrait);
		}

		private static void RemovePortrait(KerbalPortrait portrait)
		{
			if (PortraitList.Contains(portrait)) PortraitList.Remove(portrait);
		}
	}
}
