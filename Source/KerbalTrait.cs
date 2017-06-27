#region license
/*The MIT License (MIT)
Kerbal Trait - Store info on Kerbal traits and experience level

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
using System.Linq;
using Experience;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI.TooltipTypes;
using KSP.UI.Screens.Flight;
using KSP.Localization;
using Contracts;
using Contracts.Parameters;
using FinePrint.Contracts;
using FinePrint.Contracts.Parameters;
using TMPro;

namespace PortraitStats
{
	public class KerbalTrait
	{
		// private const int UILayer = 5;
		private ProtoCrewMember protoCrew;
		private Kerbal crew;
		private GameObject iconObject;
		private Color iconColor = XKCDColors.White;
		private CTIWrapper.KerbalTraitSetting trait;
		private KerbalPortrait portrait;
		private bool highlighting;
		private PartSelector highlighter;
		private int timer;
		private List<string> touristParams = new List<string>();
		private Button transferButton;
		private string cachedTooltip;

		public KerbalTrait(Kerbal k, KerbalPortrait p)
		{
			portrait = p;
			crew = k;
			protoCrew = k.protoCrewMember;
			if (PortraitStats.ctiOk)
			{
				trait = CTIWrapper.CTI.getTrait(protoCrew.experienceTrait.TypeName);
				iconColor = (trait.Color != null) ? (Color)trait.Color : XKCDColors.White; 
			}
			GameObject hover = p.hoverObjectsContainer;
			GameObject role = hover.transform.GetChild(2).gameObject;
			setupGameObjects(role, hover, protoCrew);
			addEVAListener();
			if (protoCrew.experienceTrait.TypeName == "Tourist")
				touristUpdate();
			cachedTooltip = portrait.tooltip.descriptionString;
		}

		public List<string> TouristParams
		{
			get { return touristParams; }
		}

		public ProtoCrewMember ProtoCrew
		{
			get { return protoCrew; }
		}

		public Button TransferButton
		{
			get { return transferButton; }
		}

		public string CachedTooltip
		{
			get { return cachedTooltip; }
		}

		public KerbalPortrait Portrait
		{
			get { return portrait; }
		}

		public Kerbal Crew
		{
			get { return crew; }
		}

		private void addEVAListener()
		{
			portrait.evaButton.onClick.AddListener(EVAListener);
		}

		private void EVAListener()
		{
			if (highlighting)
			{
				timer = 0;
				highlighting = false;
				highlighter.Dismiss();
			}
		}

		public void setHighlight(bool on)
		{
			if (on && !highlighting)
			{
				if (timer < 2)
					timer++;
				else
				{
					highlighting = true;
					highlighter = PartSelector.Create(crew.InPart, null, iconColor, iconColor);
				}
			}

			if (highlighting && !on)
			{
				timer = 0;
				highlighting = false;
				highlighter.Dismiss();
			}
		}

		public void touristUpdate()
		{
			if (ContractSystem.Instance == null)
				return;

			if (ContractSystem.Instance.GetActiveContractCount() <= 0)
				return;

			Contract[] currentContracts = ContractSystem.Instance.GetCurrentActiveContracts<TourismContract>();

			for (int i = 0; i < currentContracts.Length; i++)
			{
				Contract c = currentContracts[i];

				if (c == null)
					continue;

				if (c.ContractState != Contract.State.Active)
					continue;

				var activeParams = c.AllParameters.Where(a => a.GetType() == typeof(KerbalTourParameter) && a.State == ParameterState.Incomplete).ToList();

				int l = activeParams.Count;

				for (int j = 0; j < l; j++)
				{
					KerbalTourParameter p = activeParams[j] as KerbalTourParameter;

					if (p == null)
						continue;

					if (p.State != ParameterState.Incomplete)
						continue;

					if (p.kerbalName != protoCrew.name)
						continue;

					var destinationParams = p.AllParameters.Where(a => a.GetType() == typeof(KerbalDestinationParameter) && a.State == ParameterState.Incomplete).ToList();

					int e = destinationParams.Count;

					for (int k = 0; k < e; k++)
					{
						KerbalDestinationParameter d = destinationParams[k] as KerbalDestinationParameter;

						if (d == null)
							continue;

						if (d.State != ParameterState.Incomplete)
							continue;

						if (d.kerbalName != protoCrew.name)
							continue;

						touristParams.Add(d.Title);
					}
				}

				var geeParams = c.AllParameters.Where(a => a.GetType() == typeof(KerbalGeeAdventureParameter) && a.State == ParameterState.Incomplete).ToList();

				l = geeParams.Count;

				for (int j = 0; j < l; j++)
				{
					KerbalGeeAdventureParameter g = geeParams[j] as KerbalGeeAdventureParameter;

					if (g == null)
						continue;

					if (g.State != ParameterState.Incomplete)
						continue;

					if (g.kerbalName != protoCrew.name)
						continue;

					ReachDestination destination = g.GetParameter<ReachDestination>();

					if (destination == null)
						continue;

					if (destination.Destination == null)
						continue;

					ReachSituation situation = g.GetParameter<ReachSituation>();

					if (situation == null)
						continue;

					string article = getArticle(situation.Situation);

					touristParams.Add(string.Format("Pass out from gee forces {0}\n{1} at {2}", article, ReachSituation.GetTitleStringShort(situation.Situation), Localizer.Format("<<1>>", destination.Destination.displayName)));
				}
			}
		}

		private string getArticle(Vessel.Situations sit)
		{
			switch(sit)
			{
				case Vessel.Situations.LANDED:
				case Vessel.Situations.SPLASHED:
				case Vessel.Situations.FLYING:
					return "while";
				case Vessel.Situations.SUB_ORBITAL:
				case Vessel.Situations.ESCAPING:
					return "while on a";
				case Vessel.Situations.ORBITING:
					return "while in";
				case Vessel.Situations.PRELAUNCH:
					return "while at";
				case Vessel.Situations.DOCKED:
					return "durrr";
			}

			return "";
		}

		private void setupGameObjects(GameObject r, GameObject h, ProtoCrewMember c)
		{
			if (PortraitStats.showAlways)
			{
				Transform parent = h.transform.parent;

				r.transform.SetParent(parent);
			}

			if (PortraitStats.useIcon)
			{
				r.transform.GetChild(1).gameObject.SetActive(false);

				Image back = r.GetComponent<Image>();

				if (back != null)
					back.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 51, 69);

				iconObject = createIcon(r.transform);
			}

			if (PortraitStats.transferButton)
			{
				transferButton = MonoBehaviour.Instantiate(portrait.evaButton, h.transform) as Button;

				transferButton.name = "Transfer Button";

				TextMeshProUGUI xferText = transferButton.GetComponentInChildren<TextMeshProUGUI>();

				if (xferText != null)
					xferText.text = "XFR";

				TooltipController_Text tooltip = transferButton.GetComponent<TooltipController_Text>();

				if (tooltip != null)
					tooltip.textString = "Initiate Crew Transfer";

				RectTransform rect = transferButton.GetComponent<RectTransform>();

				if (rect != null)
					rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - rect.rect.height);

				transferButton.onClick.RemoveAllListeners();

				transferButton.onClick.AddListener(new UnityEngine.Events.UnityAction(initiateTransfer));

				transferButton.gameObject.SetActive(false);
			}
		}

		private void initiateTransfer()
		{
			if (FlightGlobals.ActiveVessel == null)
				return;

			if (FlightGlobals.ActiveVessel.GetVesselCrew().Count <= 0)
				return;

			if (CrewHatchController.fetch == null)
				return;

			if (CrewHatchController.fetch.Active)
				return;

			CrewHatchController.fetch.SpawnCrewDialog(crew.InPart, false, true);

			if (CrewHatchController.fetch.CrewDialog == null)
				return;

			RectTransform rect = CrewHatchController.fetch.CrewDialog.GetComponent<RectTransform>();

			if (rect == null)
				return;

			CrewHatchController.fetch.CrewDialog.transform.localPosition = fixMousePosition(rect);
		}

		private Vector2 fixMousePosition(RectTransform r)
		{
			bool b;
			Vector2 pos = CanvasUtil.ScreenToUISpacePos(Input.mousePosition, DialogCanvasUtil.DialogCanvasRect, out b);
			return new Vector2(pos.x - r.rect.width, pos.y + r.rect.height);
		}

		private GameObject createIcon(Transform parent)
		{
			GameObject icon = trait.makeGameObject();

			RectTransform RT = icon.GetComponent<RectTransform>();
			RT.pivot = new Vector2(0, 0);
			RT.offsetMax = new Vector2(-26, 19);
			RT.offsetMin = new Vector2(-6, 1);
			RT.anchorMax = new Vector2(0.9f, 0.8f);
			RT.anchorMin = new Vector2(0.2f, 0.2f);
			RT.anchoredPosition3D = new Vector3(-56, -5, 0);
			RT.anchoredPosition = new Vector2(-56, -5);
			RT.localScale = new Vector3(1, 1, 1);
			RT.localPosition.Set(0f, 0f, 0f);

			icon.transform.SetParent(parent, false);

			icon.SetActive(true);

			return icon;
		}

	}
}
