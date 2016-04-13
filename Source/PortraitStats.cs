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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens.Flight;
using UnityEngine.UI;
using Contracts;
using FinePrint.Contracts;
using FinePrint.Contracts.Parameters;

namespace PortraitStats
{

	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class PortraitStats : MonoBehaviour
	{
		private Dictionary<string, KerbalTrait> currentCrew = new Dictionary<string, KerbalTrait>();
		private bool reload;
		private bool careerMode;
		private CameraManager.CameraMode cMode;
		private bool resetting;

		public static Texture2D pilotTex;
		public static Texture2D engTex;
		public static Texture2D sciTex;
		public static Texture2D tourTex;
		public static Texture2D unknownTex;

		private static bool loaded = false;
		private static ConfigNode settingsFile;
		public static bool traitTooltip = true;
		public static bool expTooltip;
		public static bool showAlways;
		public static bool useIcon;

		private static PortraitStats instance;

		public static PortraitStats Instance
		{
			get { return instance; }
		}

		private void Start()
		{
			instance = this;

			GameEvents.onVesselWasModified.Add(vesselCheck);
			GameEvents.onVesselChange.Add(vesselChange);
			GameEvents.OnCameraChange.Add(cameraChange);
			GameEvents.onCrewTransferred.Add(crewTransfer);
			GameEvents.Contract.onContractsLoaded.Add(onContractsLoaded);
			GameEvents.Contract.onParameterChange.Add(onContractParamModified);
			GameEvents.Contract.onAccepted.Add(onContractChange);

			careerMode = HighLogic.CurrentGame.Mode == Game.Modes.CAREER;

			reload = true;

			if (!loaded)
			{
				loaded = true;

				pilotTex = GameDatabase.Instance.GetTexture("PortraitStats/Icons/pilotIcon", false);
				engTex = GameDatabase.Instance.GetTexture("PortraitStats/Icons/engineerIcon", false);
				sciTex = GameDatabase.Instance.GetTexture("PortraitStats/Icons/scientistIcon", false);
				tourTex = GameDatabase.Instance.GetTexture("PortraitStats/Icons/touristIcon", false);
				unknownTex = GameDatabase.Instance.GetTexture("PortraitStats/Icons/questionIcon", false);

				settingsFile = GameDatabase.Instance.GetConfigNode("PortraitStats/Settings/Portrait_Stats_Config");
				if (settingsFile != null)
				{
					settingsFile.TryGetValue("traitToolTip", ref traitTooltip);
					settingsFile.TryGetValue("showAlways", ref showAlways);
					settingsFile.TryGetValue("expToolTip", ref expTooltip);
					settingsFile.TryGetValue("useIcon", ref useIcon);
				}
			}
		}

		private void OnDestroy()
		{
			GameEvents.onVesselWasModified.Remove(vesselCheck);
			GameEvents.onVesselChange.Remove(vesselChange);
			GameEvents.OnCameraChange.Remove(cameraChange);
			GameEvents.onCrewTransferred.Remove(crewTransfer);
			GameEvents.Contract.onContractsLoaded.Remove(onContractsLoaded);
			GameEvents.Contract.onParameterChange.Remove(onContractParamModified);
			GameEvents.Contract.onAccepted.Remove(onContractChange);
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
						k.CrewTip.textString = crewTooltip(k);
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
			StringBuilder text = new StringBuilder();
			text.Append("<b>" + c.name + "</b>");
			text.AppendLine();
			if (PortraitStats.Instance.careerMode)
				text.Append("<b>Experience:</b> " + c.experience.ToString("F2") + "/" + KerbalRoster.GetExperienceLevelRequirement(c.experienceLevel));
			string log = KerbalRoster.GenerateExperienceLog(c.careerLog);
			if (!string.IsNullOrEmpty(log))
			{
				text.AppendLine();
				text.Append("<b>Career Log:</b>");
				text.AppendLine();
				text.Append(log);
			}
			log = KerbalRoster.GenerateExperienceLog(c.flightLog);
			if (!string.IsNullOrEmpty(log))
			{
				text.AppendLine();
				text.Append("<b>Current Flight:</b>");
				text.AppendLine();
				text.Append(log);
			}
			return text.ToString();
		}

		private string crewTooltip(KerbalTrait c)
		{
			StringBuilder text = new StringBuilder();
			if (c.ProtoCrew.experienceTrait.TypeName == "Tourist")
			{
				text.Append("<b>" + c.ProtoCrew.name + "'s itinerary:</b>");
				if (c.TouristParams.Count > 0)
				{
					for (int i = 0; i < c.TouristParams.Count; i++)
					{
						text.AppendLine();
						string s = c.TouristParams[i];
						text.Append(s);
					}
				}
				else
				{
					text.AppendLine();
					text.Append("Get thee home!");
				}
			}
			else
			{
				text.Append("<b>" + c.ProtoCrew.name + "</b>");
				if (c.ProtoCrew.isBadass)
					text.Append(" - Badass");
				text.AppendLine();
				text.Append("Courage: " + c.ProtoCrew.courage.ToString("P0") + " Stupidity: " + c.ProtoCrew.stupidity.ToString("P0"));
				text.AppendLine();
				text.Append("<b>" + c.ProtoCrew.experienceTrait.Title + "</b>");
				if (!string.IsNullOrEmpty(c.ProtoCrew.experienceTrait.Description))
				{
					text.AppendLine();
					text.Append(c.ProtoCrew.experienceTrait.Description);
				}
				if (!string.IsNullOrEmpty(c.ProtoCrew.experienceTrait.DescriptionEffects))
				{
					text.AppendLine();
					text.Append("<b>Effects</b>");
					text.AppendLine();
					text.Append(c.ProtoCrew.experienceTrait.DescriptionEffects);
				}
			}

			return text.ToString();
		}

		private void LateUpdate()
		{
			if (!FlightGlobals.ready)
				return;

			if (FlightGlobals.ActiveVessel == null)
				return;

			if (FlightGlobals.ActiveVessel.isEVA)
				return;

			if (reload)
			{
				reload = false;

				var crew = FlightGlobals.ActiveVessel.GetVesselCrew();

				for (int i = 0; i < crew.Count; i++)
				{
					ProtoCrewMember p = crew[i];

					if (p == null)
						continue;

					if (currentCrew.ContainsKey(p.name))
						continue;

					KerbalTrait K = setupPortrait(p);

					if (K == null)
						continue;

					currentCrew.Add(K.ProtoCrew.name, K);
				}
			}
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

		private void vesselCheck(Vessel v)
		{
			if (resetting)
				StopCoroutine("delayedReset");

			StartCoroutine("delayedReset", true);
		}

		private void vesselChange(Vessel v)
		{
			currentCrew.Clear();

			if (resetting)
				StopCoroutine("delayedReset");

			StartCoroutine("delayedReset", false);
		}

		private void cameraChange(CameraManager.CameraMode c)
		{
			if (c == CameraManager.CameraMode.Flight)
			{
				if (cMode != CameraManager.CameraMode.Flight)
				{
					if (resetting)
						StopCoroutine("delayedReset");

					StartCoroutine("delayedReset", true);
				}
			}

			cMode = c;
		}
		
		private void crewTransfer(GameEvents.HostedFromToAction<ProtoCrewMember, Part> pp)
		{
			currentCrew.Clear();

			if (resetting)
				StopCoroutine("delayedReset");

			StartCoroutine("delayedReset", false);
		}

		private void onContractsLoaded()
		{
			touristUpdate();
		}
		
		private void onContractParamModified(Contract c, ContractParameter p)
		{
			if (p.GetType() == typeof(KerbalDestinationParameter))
				touristUpdate();
		}
		
		private void onContractChange(Contract c)
		{
			if (c.GetType() == typeof(TourismContract))
				touristUpdate();
		}

		private void touristUpdate()
		{
			foreach (KerbalTrait k in currentCrew.Values)
			{
				if (k.ProtoCrew.experienceTrait.TypeName == "Tourist")
					k.touristUpdate();
			}
		}

		private IEnumerator delayedReset(bool clean)
		{
			resetting = true;

			int timer = 0;

			while (timer < 5)
			{
				timer++;
				yield return null;
			}

			if (clean)
			{
				var nullPortraits = currentCrew.Where(k => k.Value.ProtoCrew == null).Select(k => k.Key).ToList();

				for (int i = 0; i < nullPortraits.Count; i++)
				{
					string s = nullPortraits[i];

					if (currentCrew.ContainsKey(s))
						currentCrew.Remove(s);
				}
			}

			reload = true;
			resetting = false;
		}
		
	}
}
