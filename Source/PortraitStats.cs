﻿#region license
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
using System.Text;
using UnityEngine;
using KSP.UI.Screens.Flight;
using Contracts;
using FinePrint.Contracts;
using FinePrint.Contracts.Parameters;

namespace PortraitStats
{

	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class PortraitStats : MonoBehaviour
	{
		private DictionaryValueList<string, KerbalTrait> currentCrew = new DictionaryValueList<string, KerbalTrait>();
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
		public static bool showAlways;
		public static bool useIcon;
		public static bool extendedTooltips;
		public static bool hoverHighlight;
		public static bool transferButton;
		public static int reloadDelay = 5;

		public static Color pilotColor = XKCDColors.PastelRed;
		public static Color engineerColor = XKCDColors.DarkYellow;
		public static Color scientistColor = XKCDColors.DirtyBlue;
		public static Color touristColor = XKCDColors.SapGreen;
		public static Color unknownColor = XKCDColors.White;

        public static Dictionary<string, KerbalTraitSetting> traitSettings = new Dictionary<string, KerbalTraitSetting>();
		private static PortraitStats instance;

		public static PortraitStats Instance
		{
			get { return instance; }
		}

		public static void log(string s, params object[] m)
		{
			Debug.Log(string.Format("[Portrait Stats] " + s, m));
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

                ConfigNode node = GameDatabase.Instance.GetConfigNode("PortraitStats/PortraitStatTraits/PortraitStatsTraits");
                if (node != null)
                {
                    var nodes = node.GetNodes("Trait");

                    for (int i = 0; i < nodes.Length; i++)
                    {
                        bool valid = true;
                        var newTrait = new KerbalTraitSetting();
                        if (nodes[i].HasValue("name"))
                            newTrait.Name = nodes[i].GetValue("name");
                        else
                            valid = false;

                        if (nodes[i].HasValue("icon"))
                            newTrait.Icon = GameDatabase.Instance.GetTexture(nodes[i].GetValue("icon"), false);
                        else
                            valid = false;

                        if (nodes[i].HasValue("color"))
                            newTrait.Color = parseColor(nodes[i], "color", XKCDColors.White);
                        else
                            valid = false;

                        if (valid)
                        {
                            try
                            {
                                traitSettings.Add(newTrait.Name, newTrait);
                            }
                            catch (Exception ex)
                            {
                                log("Error: {0}", ex.Message);
                            }
                        }
                        else
                        {
                            log("Invalid Trait node format - load failed - skipped");
                        }
                    }
                }
                else
                {
                    log("Could not find trait settings config - using defaults");
                    traitSettings.Add("Pilot", new KerbalTraitSetting("Pilot", GameDatabase.Instance.GetTexture("PortraitStats/Icons/pilotIcon", false), XKCDColors.PastelRed));
                    traitSettings.Add("Engineer", new KerbalTraitSetting("Engineer", GameDatabase.Instance.GetTexture("PortraitStats/Icons/engineerIcon", false), XKCDColors.DarkYellow));
                    traitSettings.Add("Scientist", new KerbalTraitSetting("Scientist", GameDatabase.Instance.GetTexture("PortraitStats/Icons/scientistIcon", false), XKCDColors.DirtyBlue));
                    traitSettings.Add("Tourist", new KerbalTraitSetting("Tourist", GameDatabase.Instance.GetTexture("PortraitStats/Icons/touristIcon", false), XKCDColors.SapGreen));
                }

                if (!traitSettings.ContainsKey("Unknown"))
                    traitSettings.Add("Unknown", new KerbalTraitSetting("Unknown", GameDatabase.Instance.GetTexture("PortraitStats/Icons/questionIcon", false), XKCDColors.White));
            }

            StatsGameSettings settings = HighLogic.CurrentGame.Parameters.CustomParams<StatsGameSettings>();

			showAlways = settings.AlwaysShow;
			useIcon = settings.UseIcon;
			extendedTooltips = settings.ExtendedTooltips;
			hoverHighlight = settings.HoverHighlight;
			transferButton = settings.TransferButton;

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
			GameEvents.onVesselChange.Remove(vesselChange);
			GameEvents.OnCameraChange.Remove(cameraChange);
			GameEvents.onCrewTransferred.Remove(crewTransfer);
			GameEvents.Contract.onContractsLoaded.Remove(onContractsLoaded);
			GameEvents.Contract.onParameterChange.Remove(onContractParamModified);
			GameEvents.Contract.onAccepted.Remove(onContractChange);
		}

		private void Update()
		{
			if (!transferButton && !hoverHighlight)
				return;

			var enumerator = currentCrew.GetDictEnumerator();

			while (enumerator.MoveNext())
			{
				KerbalTrait k = enumerator.Current.Value;

				if (k == null)
					continue;

				if (k.Portrait.hoverArea.Hover)
				{
					if (extendedTooltips)
						k.Portrait.tooltip.descriptionString = k.CachedTooltip + extraTooltip(k);

					if (transferButton)
					{
						Vessel v = k.Crew.InPart.vessel;

						if (v.GetCrewCapacity() > v.GetCrewCount())
						{
							if (!k.TransferButton.gameObject.activeSelf)
								k.TransferButton.gameObject.SetActive(true);
						}
						else if (k.TransferButton.gameObject.activeSelf)
							k.TransferButton.gameObject.SetActive(false);
					}

					k.setHighlight(hoverHighlight);
				}
				else
					k.setHighlight(false);
			}
		}

		private string extraTooltip(KerbalTrait k)
		{
			StringBuilder sb = StringBuilderCache.Acquire();

			if (k.ProtoCrew.experienceTrait.TypeName == "Tourist")
			{
				sb.AppendLine();
				sb.AppendLine();
				sb.Append(string.Format("<b>{0}'s itinerary:</b>", k.ProtoCrew.name));
				if (k.TouristParams.Count > 0)
				{
					for (int i = 0; i < k.TouristParams.Count; i++)
					{
						sb.AppendLine();
						string s = k.TouristParams[i];
						sb.Append(s);
					}
				}
				else
				{
					sb.AppendLine();
					sb.Append("Get thee home!");
				}
				sb.AppendLine();
				sb.AppendLine();
				sb.Append(k.Crew.InPart.partInfo.title);
			}
			else
			{
				sb.AppendLine();
				sb.AppendLine();
				sb.Append(string.Format("<b>Courage:</b> {0:P0} <b>Stupidity:</b> {1:P0}{2}{3}", k.ProtoCrew.courage, k.ProtoCrew.stupidity, k.ProtoCrew.veteran ? " - Veteran" : "", k.ProtoCrew.isBadass ? " - Badass" : ""));
				sb.AppendLine();
				sb.AppendLine();
				sb.Append(k.Crew.InPart.partInfo.title);
				sb.AppendLine();
				sb.AppendLine();
				if (PortraitStats.Instance.careerMode)
					sb.Append(string.Format("<b>Experience:</b> {0:F2}/{1}", k.ProtoCrew.experience, KerbalRoster.GetExperienceLevelRequirement(k.ProtoCrew.experienceLevel)));
				string log = KerbalRoster.GenerateExperienceLog(k.ProtoCrew.flightLog);
				if (!string.IsNullOrEmpty(log))
				{
					sb.AppendLine();
					sb.AppendLine();
					sb.Append("<b>Current Flight:</b>");
					sb.AppendLine();
					sb.Append(log);
				}
			}

			return sb.ToStringAndRelease();
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

				var crew = KerbalPortraitGallery.Instance.Portraits;

				for (int i = 0; i < crew.Count; i++)
				{
					KerbalPortrait p = crew[i];

					if (p == null)
						continue;

					Kerbal k = p.crewMember;

					if (k == null)
						return;

					if (currentCrew.Contains(p.crewMemberName))
						continue;

					if (k.state == Kerbal.States.DEAD)
						continue;

					KerbalTrait K = new KerbalTrait(k, p);

					if (K == null)
						continue;

					currentCrew.Add(p.crewMemberName, K);
				}
			}
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

			while (timer < reloadDelay)
			{
				timer++;
				yield return null;
			}

			if (clean)
			{
				List<string> nullPortraits = new List<string>();

				var enumerator = currentCrew.GetDictEnumerator();

				while (enumerator.MoveNext())
				{
					var pair = enumerator.Current;

					if (pair.Value.ProtoCrew == null)
						nullPortraits.Add(pair.Key);
				}

				for (int i = nullPortraits.Count - 1; i >= 0; i--)
				{
					string s = nullPortraits[i];

					if (currentCrew.Contains(s))
						currentCrew.Remove(s);
				}
			}

			reload = true;
			resetting = false;
		}
		
	}
}
