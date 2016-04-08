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

using Experience;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI.TooltipTypes;
using KSP.UI.Screens.Flight;

namespace PortraitStats
{
	public class KerbalTrait
	{
		private const int UILayer = 5;
		private Color iconColor;
		private ProtoCrewMember protoCrew;
		private Kerbal crew;
		private Image icon;
		private Sprite iconSprite;
		private TooltipController_Text crewTip;
		private TooltipController_Text levelTip;
		private GameObject roleObject;
		private KerbalPortrait portrait;

		public KerbalTrait(Kerbal k, KerbalPortrait p)
		{
			portrait = p;
			crew = k;
			protoCrew = k.protoCrewMember;
			GameObject hover = p.hoverObjectsContainer;
			GameObject role = hover.transform.GetChild(2).gameObject;
			roleObject = role;
			setupGameObjects(role, hover, protoCrew);
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

		private Rect crewType(ExperienceTrait t)
		{
			switch (t.TypeName)
			{
				case "Pilot":
					iconColor = PortraitStats.pilotColor;
					return new Rect(0, 100, 26, 26);
				case "Engineer":
					iconColor = PortraitStats.engineerColor;
					return new Rect(26, 100,26, 26);
				case "Scientist":
					iconColor = PortraitStats.scientistColor;
					return new Rect(53, 100, 26, 26);
				case "Tourist":
					iconColor = PortraitStats.touristColor;
					return new Rect(79, 100, 26, 26);
				default:
					iconColor = PortraitStats.unknownColor;
					return new Rect(104, 100, 26, 26);
			}
		}

		private Sprite setupIconImage()
		{
			//return Sprite.Create(PortraitStats.atlas, crewType(protoCrew.experienceTrait), new Vector2(0.5f, 0.5f));

			return Sprite.Create(new Texture2D(26, 26), new Rect(0, 0, 26, 26), new Vector2(0.5f, 0.5f));
		}

		private void setupGameObjects(GameObject r, GameObject h, ProtoCrewMember c)
		{
			if (PortraitStats.showAlways)
			{
				Transform parent = h.transform.parent;

				r.transform.SetParent(parent);
			}

			//if (PortraitStats.useIcon)
			//{
			//	r.transform.GetChild(0).gameObject.SetActive(false);

			//	Image back = r.GetComponent<Image>();

			//	if (back == null)
			//		return;

			//	back.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 51, 69);

			//	//RectTransform RT = back.rectTransform;

			//	//log("Image BackGround: Anchor {0:F3}\nAnchor3D {1:F3}\nAnchorMax {2:F3}\nAnchorMin {3:F3}\nPosition {4:F3}\nOffsetMax {5:F3}\nOffsetMin {6:F3}\nPivot {7:F3}\nSize {8:F3}", RT.anchoredPosition, RT.anchoredPosition3D, RT.anchorMax, RT.anchorMin, RT.rect, RT.offsetMax, RT.offsetMin, RT.pivot, RT.sizeDelta);

			//	//RT = r.transform.GetChild(0).gameObject.GetComponent<Text>().rectTransform;

			//	//log("Level Text: Anchor {0:F3}\nAnchor3D {1:F3}\nAnchorMax {2:F3}\nAnchorMin {3:F3}\nPosition {4:F3}\nOffsetMax {5:F3}\nOffsetMin {6:F3}\nPivot {7:F3}\nSize {8:F3}", RT.anchoredPosition, RT.anchoredPosition3D, RT.anchorMax, RT.anchorMin, RT.rect, RT.offsetMax, RT.offsetMin, RT.pivot, RT.sizeDelta);

			//	//RT = r.transform.GetChild(1).gameObject.GetComponent<Image>().rectTransform;

			//	//log("Stars Image: Anchor {0:F3}\nAnchor3D {1:F3}\nAnchorMax {2:F3}\nAnchorMin {3:F3}\nPosition {4:F3}\nOffsetMax {5:F3}\nOffsetMin {6:F3}\nPivot {7:F3}\nSize {8:F3}", RT.anchoredPosition, RT.anchoredPosition3D, RT.anchorMax, RT.anchorMin, RT.rect, RT.offsetMax, RT.offsetMin, RT.pivot, RT.sizeDelta);

			//	//CanvasRenderer cr = back.canvasRenderer;

			//	//log("Image Background Renderer: AbsDepth {0}\nRelDepth {1}", cr.absoluteDepth, cr.relativeDepth);

			//	//cr = r.transform.GetChild(0).gameObject.GetComponent<Text>().canvasRenderer;

			//	//log("Level Text Renderer: AbsDepth {0}\nRelDepth {1}", cr.absoluteDepth, cr.relativeDepth);

			//	//cr = r.transform.GetChild(1).gameObject.GetComponent<Image>().canvasRenderer;

			//	//log("Stars Image Renderer: AbsDepth {0}\nRelDepth {1}", cr.absoluteDepth, cr.relativeDepth);

			//	createIcon(r.transform, setupIconImage());
			//}

			if (PortraitStats.traitTooltip)
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
			GameObject icon = new GameObject("Image");

			icon.layer = UILayer;

			RectTransform RT = icon.AddComponent<RectTransform>();
			RT.pivot = new Vector2(0, 0);
			RT.offsetMax = new Vector2(-32, 27);
			RT.offsetMin = new Vector2(-6, 1);
			RT.anchorMax = new Vector2(0, 0);
			RT.anchorMin = new Vector2(0, 0);
			RT.anchoredPosition3D = new Vector3(6, 1, 0);
			RT.anchoredPosition = new Vector2(6, 1);
			RT.localScale = new Vector3(1, 1, 1f);
			RT.localPosition.Set(0f, 0f, 0f);

			log("Sprite: Anchor {0:F3}\nAnchor3D {1:F3}\nAnchorMax {2:F3}\nAnchorMin {3:F3}\nPosition {4:F3}\nOffsetMax {5:F3}\nOffsetMin {6:F3}\nPivot {7:F3}\nSize {8:F3}", RT.anchoredPosition, RT.anchoredPosition3D, RT.anchorMax, RT.anchorMin, RT.rect, RT.offsetMax, RT.offsetMin, RT.pivot, RT.sizeDelta);

			CanvasRenderer cr = icon.AddComponent<CanvasRenderer>();

			log("Sprite Renderer: AbsDepth {0}\nRelDepth {1}", cr.absoluteDepth, cr.relativeDepth);

			Image i = icon.AddComponent<Image>();
			//i.GetComponent<SpriteRenderer>().color = iconColor;
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
