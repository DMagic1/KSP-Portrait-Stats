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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens.Flight;
using UnityEngine.UI;

namespace PortraitStats
{

	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class PortraitStats : MonoBehaviour
	{
		private Dictionary<string, KerbalTrait> currentCrew = new Dictionary<string, KerbalTrait>();
		private bool reload;
		public bool careerMode;
		private int crewCount;

		public static Texture2D atlas;

		private static bool loaded = false;
		private static ConfigNode settingsFile;
		public static bool traitTooltip = true;
		public static bool expTooltip;
		public static bool showAlways;
		public static bool useIcon;
		public static Color pilotColor = XKCDColors.PastelRed;
		public static Color engineerColor = XKCDColors.DarkYellow;
		public static Color scientistColor = XKCDColors.DirtyBlue;
		public static Color touristColor = XKCDColors.SapGreen;
		public static Color unknownColor = XKCDColors.White;

		private static PortraitStats instance;

		public static PortraitStats Instance
		{
			get { return instance; }
		}

		private void Start()
		{
			instance = this;

			GameEvents.onVesselWasModified.Add(vesselCheck);
			GameEvents.onVesselChange.Add(vesselCheck);

			careerMode = HighLogic.CurrentGame.Mode == Game.Modes.CAREER;

			reload = true;

			if (!loaded)
			{
				loaded = true;

				atlas = GameDatabase.Instance.GetTexture("PortraitStats/Icons/Atlas", false);

				settingsFile = GameDatabase.Instance.GetConfigNode("PortraitStats/Settings/Portrait_Stats_Config");
				if (settingsFile != null)
				{
					settingsFile.TryGetValue("traitToolTip", ref traitTooltip);
					settingsFile.TryGetValue("showAlways", ref showAlways);
					settingsFile.TryGetValue("expToolTip", ref expTooltip);
					settingsFile.TryGetValue("useIcon", ref useIcon);

					if (settingsFile.HasValue("pilotColor"))
						pilotColor = parseColor(settingsFile, "pilotColor", pilotColor);
					if (settingsFile.HasValue("engineerColor"))
						engineerColor = parseColor(settingsFile, "engineerColor", engineerColor);
					if (settingsFile.HasValue("scientistColor"))
						scientistColor = parseColor(settingsFile, "scientistColor", scientistColor);
					if (settingsFile.HasValue("touristColor"))
						touristColor = parseColor(settingsFile, "touristColor", touristColor);
					if (settingsFile.HasValue("unknownClassColor"))
						unknownColor = parseColor(settingsFile, "unknownClassColor", unknownColor);
				}
			}
		}

		private Color parseColor(ConfigNode node, string name, Color c)
		{
			try
			{
				return ConfigNode.ParseColor(node.GetValue(name));
			}
			catch
			{
				return c;
			}
		}

		private void OnDestroy()
		{
			GameEvents.onVesselWasModified.Remove(vesselCheck);
			GameEvents.onVesselChange.Remove(vesselCheck);
		}

		private void Update()
		{
			if (!traitTooltip && !expTooltip)
				return;

			foreach (KerbalTrait k in currentCrew.Values)
			{
				if (k == null)
					continue;

				if (k.Portrait.hoverArea.Hover)
				{
					if (traitTooltip)
					{
						k.CrewTip.textString = crewTooltip(k.ProtoCrew);
					}
					if (expTooltip)
					{
						k.LevelTip.textString = levelTooltip(k.ProtoCrew);
					}
				}
			}
		}

		private string levelTooltip(ProtoCrewMember c)
		{
			string text = "<b>" + c.name + "</b>";
			if (PortraitStats.Instance.careerMode)
				text += "\n<b>Experience:</b> " + c.experience.ToString("F2") + "/" + KerbalRoster.GetExperienceLevelRequirement(c.experienceLevel);
			string log = KerbalRoster.GenerateExperienceLog(c.careerLog);
			if (!string.IsNullOrEmpty(log))
			{
				text += "\n<b>Career Log:</b>\n";
				text += log;
			}
			log = KerbalRoster.GenerateExperienceLog(c.flightLog);
			if (!string.IsNullOrEmpty(log))
			{
				text += "\n<b>Current Flight:</b>\n";
				text += log;
			}
			return text;
		}

		private string crewTooltip(ProtoCrewMember c)
		{
			string text = "<b>" + c.name + "</b>";
			if (c.isBadass)
				text += " - Badass";
			text += "\nCourage: " + c.courage.ToString("P0") + " Stupidity: " + c.stupidity.ToString("P0");
			text += "\n<b>" + c.experienceTrait.Title + "</b>";
			if (!string.IsNullOrEmpty(c.experienceTrait.Description))
			{
				text += "\n" + c.experienceTrait.Description;
			}
			if (!string.IsNullOrEmpty(c.experienceTrait.DescriptionEffects))
			{
				text += "\n<b>Effects</b>\n";
				text += c.experienceTrait.DescriptionEffects;
			}
			return text;
		}

		private void LateUpdate()
		{
			if (!FlightGlobals.ready)
				return;

			if (FlightGlobals.ActiveVessel == null)
				return;

			if (reload)
			{
				reload = false;

				currentCrew.Clear();

				crewCount = FlightGlobals.ActiveVessel.GetCrewCount();

				var crew = FlightGlobals.ActiveVessel.GetVesselCrew();

				for (int i = 0; i < crew.Count; i++)
				{
					ProtoCrewMember p = crew[i];

					if (p == null)
						continue;

					KerbalTrait K = setupPortrait(p);

					currentCrew.Add(K.ProtoCrew.name, K);
				}
			}

			if (crewCount <= 0)
				return;
		}

		private KerbalTrait setupPortrait(ProtoCrewMember k)
		{
			KerbalPortrait P = null;

			var pField = typeof(Kerbal).GetField("portrait", BindingFlags.NonPublic | BindingFlags.Instance);

			try
			{
				P = pField.GetValue(k.KerbalRef) as KerbalPortrait;
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Portrait Stats] Error in locating Kerbal Crew Portrait; skipping [" + k.name + "]\n" + e);
				return null;
			}

			if (P == null)
				return null;

			return new KerbalTrait(k.KerbalRef, P);
		}

		private void log(string s, params object[] m)
		{
			Debug.Log(string.Format("[Portrait Stats] " + s, m));
		}

		private void vesselCheck(Vessel v)
		{
			reload = true;
		}

	}
}
