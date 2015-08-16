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

namespace PortraitStats
{
	public class KerbalTrait
	{
		private Rect levelPos;
		private Rect traitPos;
		private Color iconColor;
		private ProtoCrewMember protoCrew;

		public KerbalTrait(Kerbal k)
		{
			protoCrew = k.protoCrewMember;
			traitPos = crewType(k.protoCrewMember.experienceTrait);
			levelPos = levelRect(k.protoCrewMember.experienceLevel);
		}

		public ProtoCrewMember ProtoCrew
		{
			get { return protoCrew; }
		}

		public Rect LevelPos
		{
			get { return levelPos; }
		}

		public Rect TraitPos
		{
			get { return traitPos; }
		}

		public Color IconColor
		{
			get { return iconColor; }
		}

		private Rect crewType(ExperienceTrait t)
		{
			switch (t.Title)
			{
				case "Pilot":
					iconColor = XKCDColors.PastelRed;
					return new Rect(0, 0.78125f, 0.203125f, 0.203125f);
				case "Engineer":
					iconColor = XKCDColors.DarkYellow;
					return new Rect(0.203125f, 0.78125f, 0.203125f, 0.203125f);
				case "Scientist":
					iconColor = XKCDColors.DirtyBlue;
					return new Rect(0.4140625f, 0.78125f, 0.203125f, 0.203125f);
				case "Tourist":
					iconColor = XKCDColors.SapGreen;
					return new Rect(0.6171875f, 0.78125f, 0.203125f, 0.203125f);
				default:
					iconColor = XKCDColors.White;
					return new Rect(0.8125f, 0.796875f, 0.203125f, 0.203125f);
			}
		}

		private Rect levelRect(int i)
		{
			return new Rect((i * 0.171875f) - (i * 0.0078125f), 0, 0.1640625f, 0.765625f);
		}
	}
}
