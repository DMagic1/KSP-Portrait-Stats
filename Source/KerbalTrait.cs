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
using Contracts;
using FinePrint.Contracts;
using FinePrint.Contracts.Parameters;

namespace PortraitStats
{
	public class KerbalTrait
	{
		private const int UILayer = 5;
		private ProtoCrewMember protoCrew;
		private Kerbal crew;
		private TooltipController_Text crewTip;
		private TooltipController_Text levelTip;
		private GameObject iconObject;
		private KerbalPortrait portrait;
		private List<string> touristParams = new List<string>();

		public KerbalTrait(Kerbal k, KerbalPortrait p)
		{
			portrait = p;
			crew = k;
			protoCrew = k.protoCrewMember;
			GameObject hover = p.hoverObjectsContainer;
			GameObject role = hover.transform.GetChild(2).gameObject;
			setupGameObjects(role, hover, protoCrew);
			if (protoCrew.experienceTrait.TypeName == "Tourist")
				touristUpdate();
		}

		public List<string> TouristParams
		{
			get { return touristParams; }
		}

		public ProtoCrewMember ProtoCrew
		{
			get { return protoCrew; }
		}

		public TooltipController_Text CrewTip
		{
			get { return crewTip; }
		}

		public TooltipController_Text LevelTip
		{
			get { return levelTip; }
		}

		public KerbalPortrait Portrait
		{
			get { return portrait; }
		}

		public Kerbal Crew
		{
			get { return crew; }
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

				if (activeParams.Count <= 0)
					continue;

				for (int j = 0; j < activeParams.Count; j++)
				{
					KerbalTourParameter p = activeParams[j] as KerbalTourParameter;

					if (p == null)
						continue;

					if (p.State != ParameterState.Incomplete)
						continue;

					if (p.kerbalName != protoCrew.name)
						continue;

					var destinationParams = p.AllParameters.Where(a => a.GetType() == typeof(KerbalDestinationParameter) && a.State == ParameterState.Incomplete).ToList();

					if (destinationParams.Count <= 0)
						continue;

					for (int k = 0; k < destinationParams.Count; k++)
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
			}
		}

		private Sprite crewIcon(ExperienceTrait t)
		{
			switch (t.TypeName)
			{
				case "Pilot":
					return Sprite.Create(PortraitStats.pilotTex, new Rect(0, 0, 28, 28), new Vector2(0.5f, 0.5f));
				case "Engineer":
					return Sprite.Create(PortraitStats.engTex, new Rect(0, 0, 28, 28), new Vector2(0.5f, 0.5f));
				case "Scientist":
					return Sprite.Create(PortraitStats.sciTex, new Rect(0, 0, 28, 28), new Vector2(0.5f, 0.5f));
				case "Tourist":
					return Sprite.Create(PortraitStats.tourTex, new Rect(0, 0, 28, 28), new Vector2(0.5f, 0.5f));
				default:
					return Sprite.Create(PortraitStats.unknownTex, new Rect(0, 0, 24, 24), new Vector2(0.5f, 0.5f));
			}
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
				r.transform.GetChild(0).gameObject.SetActive(false);

				Image back = r.GetComponent<Image>();

				if (back == null)
					return;

				back.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 51, 69);

				iconObject = createIcon(r.transform, crewIcon(c.experienceTrait));

				if (PortraitStats.traitTooltip)
				{
					crewTip = iconObject.AddComponent<TooltipController_Text>();

					crewTip.TooltipPrefabType = h.transform.GetChild(0).gameObject.GetComponent<TooltipController_Text>().TooltipPrefabType;
				}
			}
			else if (PortraitStats.traitTooltip)
			{
				crewTip = r.transform.GetChild(0).gameObject.AddComponent<TooltipController_Text>();

				crewTip.TooltipPrefabType = h.transform.GetChild(0).gameObject.GetComponent<TooltipController_Text>().TooltipPrefabType;
			}

			if (PortraitStats.expTooltip)
			{
				levelTip = r.transform.GetChild(1).gameObject.AddComponent<TooltipController_Text>();

				levelTip.TooltipPrefabType = h.transform.GetChild(0).gameObject.GetComponent<TooltipController_Text>().TooltipPrefabType;
			}
		}

		private GameObject createIcon(Transform parent, Sprite s)
		{
			GameObject icon = new GameObject("Icon");

			icon.layer = UILayer;

			RectTransform RT = icon.AddComponent<RectTransform>();
			RT.pivot = new Vector2(0, 0);
			RT.offsetMax = new Vector2(-26, 19);
			RT.offsetMin = new Vector2(-6, 1);
			RT.anchorMax = new Vector2(0.9f, 0.8f);
			RT.anchorMin = new Vector2(0.2f, 0.2f);
			RT.anchoredPosition3D = new Vector3(-56, -5, 0);
			RT.anchoredPosition = new Vector2(-56, -5);
			RT.localScale = new Vector3(1, 1, 1);
			RT.localPosition.Set(0f, 0f, 0f);

			CanvasRenderer cr = icon.AddComponent<CanvasRenderer>();

			//log("Sprite: Anchor {0:F3}\nAnchor3D {1:F3}\nAnchorMax {2:F3}\nAnchorMin {3:F3}\nPosition {4:F3}\nOffsetMax {5:F3}\nOffsetMin {6:F3}\nPivot {7:F3}\nSize {8:F3}\nScale {9:F3}", RT.anchoredPosition, RT.anchoredPosition3D, RT.anchorMax, RT.anchorMin, RT.rect, RT.offsetMax, RT.offsetMin, RT.pivot, RT.sizeDelta, RT.localScale);

			Image i = icon.AddComponent<Image>();
			i.sprite = s;			

			icon.transform.SetParent(parent, false);

			icon.SetActive(true);

			return icon;
		}

		private void log(string s, params object[] m)
		{
			Debug.Log(string.Format("[Portrait Stats] " + s, m));
		}
	}
}
