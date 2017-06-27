using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PortraitStats
{
	public class CTIWrapper
	{
		private static Type CTIAddonType;
		private static Type KerbalTraitSettingType;

		public static CTIAPI CTI;

		private static bool _wrapped = false;
		public static bool ApiReady { get { return _wrapped; } }

		public static bool initCTIWrapper()
		{
			try
			{
				CTIAddonType = null;
				KerbalTraitSettingType = null;
				log("Attempting to wrap Community Trait Icons...");

				CTIAddonType = AssemblyLoader.loadedAssemblies.Select(a => a.assembly.GetExportedTypes()).SelectMany(t => t).FirstOrDefault(t => t.FullName == "CommunityTraitIcons.CTIAddon");
				if (CTIAddonType == null)
					return false;
				log("Community Trait Icons found: ver {0}",CTIAddonType.Assembly.GetName().Version.ToString());

				KerbalTraitSettingType = AssemblyLoader.loadedAssemblies.Select(a => a.assembly.GetExportedTypes()).SelectMany(t => t).FirstOrDefault(t => t.FullName == "CommunityTraitIcons.KerbalTraitSetting");
				if (KerbalTraitSettingType == null)
					return false;

				CTI = new CTIAPI();
				_wrapped = true;
				return true;
			}
			catch (Exception e)
			{
				log("Unable to wrap Community Trait Icons.");
				log("Exception: {0}",e);
				return false;
			}
		}

		public class CTIAPI
		{
			internal CTIAPI()
			{
				try
				{
					_Loaded = CTIAddonType.GetProperty("Loaded", BindingFlags.Public | BindingFlags.Static);
					_getTrait = CTIAddonType.GetMethod("getTrait", BindingFlags.Public | BindingFlags.Static);
				}
				catch (Exception e)
				{
					log("Unable to wrap CTIAPI.");
					log("Exception: {0}",e);
				}
			}

			private PropertyInfo _Loaded;
			public bool Loaded
			{
				get
				{
					if (_Loaded == null) return false;
					return (bool)_Loaded.GetValue(null, null);
				}
			}

			private MethodInfo _getTrait;
			public KerbalTraitSetting getTrait(string traitName)
			{
				if (_getTrait == null) return null;
				object[] paramArr = new object[] { traitName };
				return new KerbalTraitSetting(_getTrait.Invoke(null, paramArr));
			}
		}

		public class KerbalTraitSetting
		{
			internal KerbalTraitSetting(object o)
			{
				_actualKerbalTraitSetting = o;
				_Name = KerbalTraitSettingType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
				_Icon = KerbalTraitSettingType.GetProperty("Icon", BindingFlags.Public | BindingFlags.Instance);
				_Color = KerbalTraitSettingType.GetProperty("Color", BindingFlags.Public | BindingFlags.Instance);
				_makeSprite = KerbalTraitSettingType.GetMethod("makeSprite", BindingFlags.Public | BindingFlags.Instance);
				_makeDialogGUIImage = KerbalTraitSettingType.GetMethod("makeDialogGUIImage", BindingFlags.Public | BindingFlags.Instance);
				_makeDialogGUISprite = KerbalTraitSettingType.GetMethod("makeDialogGUISprite", BindingFlags.Public | BindingFlags.Instance);
				_makeGameObject = KerbalTraitSettingType.GetMethod("makeGameObject", BindingFlags.Public | BindingFlags.Instance);
				_attachImage = KerbalTraitSettingType.GetMethod("attachImage", BindingFlags.Public | BindingFlags.Instance);
			}

			private object _actualKerbalTraitSetting;

			private PropertyInfo _Name;
			public string Name
			{
				get
				{
					if (_Name == null) return null;
					return (string)_Name.GetValue(_actualKerbalTraitSetting, null);
				}
			}

			private PropertyInfo _Icon;
			public Texture2D Icon
			{
				get
				{
					if (_Icon == null) return null;
					return (Texture2D)_Icon.GetValue(_actualKerbalTraitSetting, null);
				}
			}

			private PropertyInfo _Color;
			public Color? Color
			{
				get
				{
					if (_Color == null) return null;
					return (Color)_Color.GetValue(_actualKerbalTraitSetting, null);
				}
			}

			private MethodInfo _makeSprite;
			public Sprite makeSprite()
			{
				if (_makeSprite == null) return null;
				return (Sprite)_makeSprite.Invoke(_actualKerbalTraitSetting, null);
			}

			private MethodInfo _makeDialogGUIImage;
			public DialogGUIImage makeDialogGUIImage(Vector2 s, Vector2 p)
			{
				if (_makeDialogGUIImage == null) return null;
				object[] paramArr = new object[] { s,p };
				return (DialogGUIImage)_makeDialogGUIImage.Invoke(_actualKerbalTraitSetting, paramArr);
			}

			private MethodInfo _makeDialogGUISprite;
			public DialogGUISprite makeDialogGUISprite(Vector2 s, Vector2 p)
			{
				if (_makeDialogGUISprite == null) return null;
				object[] paramArr = new object[] { s,p };
				return (DialogGUISprite)_makeDialogGUISprite.Invoke(_actualKerbalTraitSetting, paramArr);
			}

			private MethodInfo _makeGameObject;
			public GameObject makeGameObject()
			{
				if (_makeGameObject == null) return null;
				return (GameObject)_makeGameObject.Invoke(_actualKerbalTraitSetting, null);
			}

			private MethodInfo _attachImage;
			public bool attachImage(GameObject go)
			{
				if (_attachImage == null) return false;
				object[] paramArr = new object[] { go };
				return (bool)_attachImage.Invoke(_actualKerbalTraitSetting, paramArr);
			}
		}

		private static void log(string s, params object[] m)
		{
			Debug.Log(string.Format("[CTIWrapper] " + s, m));
		}
	}
}
