using System;
using System.Collections.Generic;
using UnityEngine;

namespace PortraitStats
{
	public class StatsGameSettings : GameParameters.CustomParameterNode
	{
		private string _title = "Portrait Stats";
		private int _order = 0;
		private string _section = "DMagic Mods";
		private bool _haspresets = true;
		private GameParameters.GameMode _gamemode = GameParameters.GameMode.ANY;

		public StatsGameSettings()
		{
			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				if (PortraitStatsSettings.Instance == null)
					return;

				AlwaysShow = PortraitStatsSettings.Instance.ShowAlways;
				UseIcon = PortraitStatsSettings.Instance.UseIcon;
				ExtendedTooltips = PortraitStatsSettings.Instance.ExtendedTooltips;
				HoverHighlight = PortraitStatsSettings.Instance.HoverHighlight;
				TransferButton = PortraitStatsSettings.Instance.TransferButton;
				ReloadDelay = PortraitStatsSettings.Instance.ReloadDelay;
			}
		}

		public override string Title
		{
			get { return _title; }
		}

		public override string Section
		{
			get { return _section; }
		}

		public override int SectionOrder
		{
			get { return _order; }
		}

		public override bool HasPresets
		{
			get { return _haspresets; }
		}

		public override GameParameters.GameMode GameMode
		{
			get { return _gamemode; }
		}

		public override void SetDifficultyPreset(GameParameters.Preset preset)
		{
			
		}

		public override bool Enabled(System.Reflection.MemberInfo member, GameParameters parameters)
		{
			if (member.Name == "Reload")
				return HighLogic.LoadedSceneIsFlight;

			return true;
		}

		[GameParameters.CustomStringParameterUI("", lines = 2, autoPersistance = false)]
		public string Reload = "A re-load is required for changes to take effect.";
		[GameParameters.CustomParameterUI("Always Show Info", toolTip = "Show trait and level info without hovering", autoPersistance = true)]
		public bool AlwaysShow = true;
		[GameParameters.CustomParameterUI("Use Trait Icon", toolTip = "Use icons in place of text for Kerbal class", autoPersistance = true)]
		public bool UseIcon = true;
		[GameParameters.CustomParameterUI("Use Expanded Tooltips", toolTip = "Show additional information in the portrait tooltips", autoPersistance = true)]
		public bool ExtendedTooltips = true;
		[GameParameters.CustomParameterUI("Highlight Current Part", toolTip = "Highlight the Kerbal's current part when hovering over their portrait", autoPersistance = true)]
		public bool HoverHighlight = true;
		[GameParameters.CustomParameterUI("Show Crew Transfer Button", toolTip = "Show crew transfer button below the standard IVA and EVA buttons", autoPersistance = true)]
		public bool TransferButton = true;
		[GameParameters.CustomIntParameterUI("Reload Delay", minValue = 3, maxValue = 20, toolTip = "May be modified if there are errors when adding or removing crew", autoPersistance = true)]
		public int ReloadDelay = 5;
		[GameParameters.CustomParameterUI("Use Settings As Default", toolTip = "Use the current settings as the defaults for all new games", autoPersistance = false)]
		public bool UseAsDefault;
	}
}
