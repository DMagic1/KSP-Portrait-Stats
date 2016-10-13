using System;
using System.Reflection;
using System.IO;
using UnityEngine;
namespace PortraitStats
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class PortraitStatsSettings : MonoBehaviour
	{
		[Persistent]
		public bool ShowAlways = true;
		[Persistent]
		public bool UseIcon = true;
		[Persistent]
		public bool ExtendedTooltips = true;
		[Persistent]
		public bool HoverHighlight = true;
		[Persistent]
		public bool TransferButton = true;
		[Persistent]
		public int ReloadDelay = 5;
		[Persistent]
		public Color PilotColor = XKCDColors.PastelRed;
		[Persistent]
		public Color EngineerColor = XKCDColors.DarkYellow;
		[Persistent]
		public Color ScientistColor = XKCDColors.DirtyBlue;
		[Persistent]
		public Color TouristColor = XKCDColors.SapGreen;
		[Persistent]
		public Color UnknownColor = XKCDColors.White;

		private const string fileName = "PluginData/Settings.cfg";
		private string fullPath;
		private StatsGameSettings settings;

		private static bool loaded;
		private static PortraitStatsSettings instance;

		public static PortraitStatsSettings Instance
		{
			get { return instance; }
		}

		private void Awake()
		{
			if (loaded)
				Destroy(gameObject);

			DontDestroyOnLoad(gameObject);

			loaded = true;

			instance = this;

			fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName).Replace("\\", "/");
			GameEvents.OnGameSettingsApplied.Add(SettingsApplied);

			if (Load())
				PortraitStats.log("Settings file loaded");
		}

		private void OnDestroy()
		{
			GameEvents.OnGameSettingsApplied.Remove(SettingsApplied);
		}

		public void SettingsApplied()
		{
			if (HighLogic.CurrentGame != null)
				settings = HighLogic.CurrentGame.Parameters.CustomParams<StatsGameSettings>();

			if (settings == null)
				return;

			if (settings.UseAsDefault)
			{
				ShowAlways = settings.AlwaysShow;
				UseIcon = settings.UseIcon;
				ExtendedTooltips = settings.ExtendedTooltips;
				HoverHighlight = settings.HoverHighlight;
				TransferButton = settings.TransferButton;
				ReloadDelay = settings.ReloadDelay;

				if (Save())
					PortraitStats.log("Settings file saved");
			}
		}

		public bool Load()
		{
			bool b = false;

			try
			{
				if (File.Exists(fullPath))
				{
					ConfigNode node = ConfigNode.Load(fullPath);
					ConfigNode unwrapped = node.GetNode(GetType().Name);
					ConfigNode.LoadObjectFromConfig(this, unwrapped);
					b = true;
				}
				else
				{
					PortraitStats.log("Settings file could not be found [{0}]", fullPath);
					b = false;
				}
			}
			catch (Exception e)
			{
				PortraitStats.log("Error while loading settings file from [{0}]\n{1}", fullPath, e);
				b = false;
			}

			return b;
		}

		public bool Save()
		{
			bool b = false;

			try
			{
				ConfigNode node = AsConfigNode();
				ConfigNode wrapper = new ConfigNode(GetType().Name);
				wrapper.AddNode(node);
				wrapper.Save(fullPath);
				b = true;
			}
			catch (Exception e)
			{
				PortraitStats.log("Error while saving settings file from [{0}]\n{1}", fullPath, e);
				b = false;
			}

			return b;
		}

		private ConfigNode AsConfigNode()
		{
			try
			{
				ConfigNode node = new ConfigNode(GetType().Name);

				node = ConfigNode.CreateConfigFromObject(this, node);
				return node;
			}
			catch (Exception e)
			{
				PortraitStats.log("Failed to generate settings file node...\n{0}", e);
				return new ConfigNode(GetType().Name);
			}
		}
		
	}
}
