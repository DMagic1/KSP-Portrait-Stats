#region license
/*The MIT License (MIT)
PortraitStats - Control drawing trait and experience level icons

Copyright (c) 2015 DMagic

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PortraitStats
{

	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class PortraitStats : MonoBehaviour
	{
		private Dictionary<string, KerbalTrait> currentCrew = new Dictionary<string, KerbalTrait>();
		private List<KerbalTrait> activeCrew = new List<KerbalTrait>();
		private bool reload;
		private int index;
		private Vector2 screenPos = new Vector2();
		private KerbalGUIManager manager;

		private void Start()
		{
			GameEvents.onVesselWasModified.Add(vesselCheck);

			manager = findKerbalGUIManager();

			if (manager == null)
				Destroy(this);

			reload = true;

			RenderingManager.AddToPostDrawQueue(5, drawLabels);
		}

		private void OnDestroy()
		{
			GameEvents.onVesselWasModified.Remove(vesselCheck);

			RenderingManager.RemoveFromPostDrawQueue(5, drawLabels);
		}

		private void Update()
		{
			if (!FlightGlobals.ready)
				return;

			if (KerbalGUIManager.ActiveCrew.Count <= 0)
				return;

			if (reload)
			{
				foreach (Kerbal k in KerbalGUIManager.ActiveCrew)
				{
					if (currentCrew.ContainsKey(k.name))
						continue;

					currentCrew.Add(k.name, new KerbalTrait(k));
				}

				float button = KerbalGUIManager.ActiveCrew.Count > 3 ? 30 : 2;

				screenPos.x = Screen.width - manager.AvatarSpacing - manager.AvatarSize - button;
				screenPos.y = Screen.height - manager.AvatarSpacing - manager.AvatarSize - manager.AvatarTextSize + 68;

				index = int.MaxValue;
				reload = false;
			}

			if (index != manager.startIndex)
			{
				index = manager.startIndex;

				activeCrew.Clear();

				for (int i = index + 2; i >= index; i--)
				{
					if (i >= KerbalGUIManager.ActiveCrew.Count)
						continue;

					string name = KerbalGUIManager.ActiveCrew[i].name;

					if (currentCrew.ContainsKey(name))
					{
						activeCrew.Add(currentCrew[name]);
					}
				}
			}
		}

		private void drawLabels()
		{
			if (KerbalGUIManager.ActiveCrew.Count <= 0)
				return;

			switch (CameraManager.Instance.currentCameraMode)
			{
				case CameraManager.CameraMode.Map:
				case CameraManager.CameraMode.Internal:
				case CameraManager.CameraMode.IVA:
					return;
			}

			Color old = GUI.color;

			for(int i = 0; i < activeCrew.Count; i++)
			{
				float leftOffset = screenPos.x - (i * (manager.AvatarSpacing + manager.AvatarSize));
				Rect r = new Rect(leftOffset, screenPos.y, 24, 24);

				GUI.color = activeCrew[i].IconColor;

				GUI.DrawTextureWithTexCoords(r, PortraitStatsSetup.atlas, activeCrew[i].TraitPos);

				GUI.color = old;

				r.x += 24;
				r.y += 9;
				r.height = 14;
				r.width = 68;

				GUI.DrawTextureWithTexCoords(r, PortraitStatsSetup.atlas, activeCrew[i].LevelPos);
			}
		}

		private void vesselCheck(Vessel v)
		{
			reload = true;
		}

		private KerbalGUIManager findKerbalGUIManager()
		{
			FieldInfo[] fields = typeof(KerbalGUIManager).GetFields(BindingFlags.NonPublic | BindingFlags.Static);

			if (fields == null)
				return null;

			if (fields[0].GetValue(null).GetType() != typeof(KerbalGUIManager))
				return null;

			return (KerbalGUIManager)fields[0].GetValue(null);
		}

	}
}
