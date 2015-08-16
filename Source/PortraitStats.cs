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
		private bool careerMode;
		private int index;
		private Vector2 screenPos = new Vector2();
		private KerbalGUIManager manager;
		private static Texture2D atlas;

		private Rect tooltipPosition;
		private double toolTipTime;

		private int currentToolTip = -1;

		private static bool loaded = false;
		private static GUIStyle tipStyle;
		private static ConfigNode settingsFile;
		private static bool traitTooltip;
		private static bool expTooltip;

		private void Start()
		{
			GameEvents.onVesselWasModified.Add(vesselCheck);
			GameEvents.onVesselChange.Add(vesselCheck);

			manager = findKerbalGUIManager();

			careerMode = HighLogic.CurrentGame.Mode == Game.Modes.CAREER;

			if (manager == null)
				Destroy(this);

			reload = true;

			if (!loaded)
			{
				loaded = true;

				atlas = GameDatabase.Instance.GetTexture("PortraitStats/Icons/Atlas", false);

				settingsFile = GameDatabase.Instance.GetConfigNode("PortraitStats/Settings/Portrait_Stats_Config");
				if (settingsFile != null)
				{
					if (settingsFile.HasValue("traitToolTip"))
					{
						bool.TryParse(settingsFile.GetValue("traitToolTip"), out traitTooltip);
					}
					if (settingsFile.HasValue("expToolTip"))
					{
						bool.TryParse(settingsFile.GetValue("expToolTip"), out expTooltip);
					}
				}
			}

			RenderingManager.AddToPostDrawQueue(5, drawLabels);
		}

		private void OnDestroy()
		{
			GameEvents.onVesselWasModified.Remove(vesselCheck);
			GameEvents.onVesselChange.Remove(vesselCheck);

			RenderingManager.RemoveFromPostDrawQueue(5, drawLabels);
		}

		private void Update()
		{
			if ((int)Time.time % 10 == 0)
				reload = true;
		}

		private void LateUpdate()
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

				float button = KerbalGUIManager.ActiveCrew.Count > 3 ? 28 : 0;

				screenPos.x = Screen.width - manager.AvatarSpacing - manager.AvatarSize - button;
				screenPos.y = Screen.height - manager.AvatarSpacing - manager.AvatarTextSize - 26;

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
			int crewCount = KerbalGUIManager.ActiveCrew.Count;

			if (crewCount <= 0)
				return;

			switch (CameraManager.Instance.currentCameraMode)
			{
				case CameraManager.CameraMode.Map:
				case CameraManager.CameraMode.Internal:
				case CameraManager.CameraMode.IVA:
					return;
			}

			Color old = GUI.color;
			bool drawTooltip = false;
			bool drawTraitTooltip = false;

			for(int i = 0; i < activeCrew.Count; i++)
			{
				float leftOffset;

				/* This lovely bit of nonsense is due to the fact that KSP orders the crew portraits
				 * differently based on how many Kerbals are present. Crews with 2 or 3 Kerbals require
				 * special cases...
				 */
				switch (crewCount)
				{
					case 2:
						leftOffset = screenPos.x - ((i == 0 ? 1 : 0) * (manager.AvatarSpacing + manager.AvatarSize));
						break;
					case 3:
						int j = 0;
						switch (i)
						{
							case 1:
								j = 2;
								break;
							case 2:
								j = 1;
								break;
							default:
								j = i;
								break;
						}
						leftOffset = screenPos.x - (j * (manager.AvatarSpacing + manager.AvatarSize));
						break;
					default:
						leftOffset = screenPos.x - (i * (manager.AvatarSpacing + manager.AvatarSize));
						break;
				}

				Rect r = new Rect(leftOffset, screenPos.y, 26, 26);

				GUI.color = activeCrew[i].IconColor;

				GUI.DrawTextureWithTexCoords(r, atlas, activeCrew[i].TraitPos);

				GUI.color = old;

				if (traitTooltip)
				{
					if (r.Contains(Event.current.mousePosition))
					{
						if (currentToolTip != i)
						{
							currentToolTip = i;
							toolTipTime = Time.fixedTime;
						}
						drawTooltip = true;
						drawTraitTooltip = true;
					}
				}

				if (careerMode || expTooltip)
				{
					r.x += manager.AvatarSize - 18;
					r.y -= 48;
					r.height = 72;
					r.width = 16;

					GUI.DrawTextureWithTexCoords(r, atlas, activeCrew[i].LevelPos);

					if (!drawTooltip && expTooltip)
					{
						if (r.Contains(Event.current.mousePosition))
						{
							if (currentToolTip != i)
							{
								currentToolTip = i;
								toolTipTime = Time.fixedTime;
							}
							drawTooltip = true;
							drawTraitTooltip = false;
						}
					}
				}
			}

			// Tooltip drawing - do this after the loop to make sure it gets drawn on top
			if (drawTooltip)
			{
				DrawToolTip(currentToolTip, drawTraitTooltip);
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

		private void DrawToolTip(int index, bool drawTraitTip)
		{
			if (tipStyle == null)
			{
				tipStyle = new GUIStyle(GUI.skin.box);
				tipStyle.wordWrap = true;
				tipStyle.stretchHeight = true;
				tipStyle.normal.textColor = Color.white;
				tipStyle.richText = true;
				tipStyle.alignment = TextAnchor.UpperLeft;
			}

			GUI.depth = 0;
			if (Time.fixedTime > toolTipTime + 0.4)
			{
				ProtoCrewMember pcm = activeCrew[index].ProtoCrew;

				if (pcm == null)
					return;

				string text = "";

				if (drawTraitTip)
				{
					text = "<b>" + pcm.name + "</b>";
					if (pcm.isBadass)
						text += " - Badass";
					text += "\nCourage: " + pcm.courage.ToString("P0") + " Stupidity: " + pcm.stupidity.ToString("P0");
					text += "\n<b>" + pcm.experienceTrait.Title + "</b>";
					if (!string.IsNullOrEmpty(pcm.experienceTrait.Description))
					{
						text += "\n" + pcm.experienceTrait.Description;
					}
					if (!string.IsNullOrEmpty(pcm.experienceTrait.DescriptionEffects))
					{
						text += "\n<b>Effects</b>\n";
						text += pcm.experienceTrait.DescriptionEffects;
					}
				}
				else
				{
					text = "<b>" + pcm.name + "</b>";
					if (careerMode)
						text += "\n<b>Experience:</b> " + pcm.experience.ToString("F0") + "/" + KerbalRoster.GetExperienceLevelRequirement(pcm.experienceLevel);
					string log = KerbalRoster.GenerateExperienceLog(pcm.careerLog);
					if (!string.IsNullOrEmpty(log))
					{
						text += "\n<b>Career Log:</b>\n";
						text += log;
					}
					log = KerbalRoster.GenerateExperienceLog(pcm.flightLog);
					if (!string.IsNullOrEmpty(log))
					{
						text += "\n<b>Current Flight:</b>\n";
						text += log;
					}
				}

				GUIContent tip = new GUIContent(text);

				Vector2 textDimensions = tipStyle.CalcSize(tip);
				if (textDimensions.x > 320)
				{
					textDimensions.x = 320;
					textDimensions.y = tipStyle.CalcHeight(tip, 320);
				}
				tooltipPosition.width = textDimensions.x;
				tooltipPosition.height = textDimensions.y;
				tooltipPosition.x = Event.current.mousePosition.x + tooltipPosition.width > Screen.width ?
					Screen.width - tooltipPosition.width : Event.current.mousePosition.x;
				tooltipPosition.y = Event.current.mousePosition.y - tooltipPosition.height - 8;

				GUI.Label(tooltipPosition, tip, tipStyle);
			}
		}

	}
}
