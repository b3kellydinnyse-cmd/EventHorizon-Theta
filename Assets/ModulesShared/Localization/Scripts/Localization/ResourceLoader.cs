using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Services.Localization
{
	internal static class ResourceLoader
	{
		public static List<XmlLanguageInfo> LoadLocalizationList()
		{
			var asset = Resources.Load<TextAsset>("Localization/" + AppConfig.localizationFile);
			if (!asset) return null;

			var serializer = new XmlSerializer(typeof(XmlLocalizationList));

			XmlLocalizationList list;
			using (var reader = new System.IO.StringReader(asset.text))
				list = serializer.Deserialize(reader) as XmlLocalizationList;

			return list.items;
		}

		public static bool TryLoadFromResources(string language, Dictionary<string, string> keys)
		{
			var assets = Resources.LoadAll<TextAsset>("Localization/" + language);
			if (assets.Length == 0)
				return false;

			var serializer = new XmlSerializer(typeof(XmlLocalization));
			foreach (var resource in assets)
			{
				try
				{
                    LoadFromXml(serializer, resource.text, keys, false);
                }
				catch (Exception e)
				{
					Debug.LogError("Unable to load localization file: " + e.Message);
				}
			}

			return true;
		}

		public static bool TryLoadFromDatabase(GameDatabase.IDatabase database, string language, Dictionary<string, string> keys)
		{
			if (database == null)
				return false;

			try
			{
				var data = database.GetLocalization(language);
				if (string.IsNullOrEmpty(data))
					return false;

                // Read AllowOverride config directly from IDatabase property
                bool allowOverride = false;
                if (database.LocalizationSettings != null)
                {
                    allowOverride = database.LocalizationSettings.AllowOverride;
                }

                var serializer = new XmlSerializer(typeof(XmlLocalization));
                LoadFromXml(serializer, data, keys, allowOverride);
                return true;
			}
			catch (Exception e)
			{
				Debug.LogError("Unable to load localization from database: " + e.Message);
				return false;
			}
		}

        private static void LoadFromXml(XmlSerializer serializer, string data, Dictionary<string, string> keys, bool allowOverride)
        {
            XmlLocalization localization;
            using (var reader = new System.IO.StringReader(data))
                localization = serializer.Deserialize(reader) as XmlLocalization;

            foreach (var item in localization.items)
            {
                if (allowOverride)
                {
                    keys[item.name] = item.value;
                }
                else
                {
                    if (keys.ContainsKey(item.name))
                    {
                        UnityEngine.Debug.Log("LocalizationManager: duplicate name skipped - " + item.name);
                        continue;
                    }
                    keys.Add(item.name, item.value);
                }
            }
        }
    }
}
