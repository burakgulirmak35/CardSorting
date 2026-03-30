using System.Collections.Generic;
using UnityEngine;

namespace ZyngaCardGame.Core
{
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }
        private string _defaultLanguage = "en";
        private Dictionary<string, string> _strings = new Dictionary<string, string>();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadLanguage(_defaultLanguage);
        }

        public void LoadLanguage(string languageCode)
        {
            _strings.Clear();
            TextAsset file = Resources.Load<TextAsset>($"Localization/{languageCode}");
            LocalizationFile data = JsonUtility.FromJson<LocalizationFile>(file.text);
            foreach (var entry in data.entries)
                _strings[entry.key] = entry.value;
        }

        public string GetString(string key)
        {
            if (_strings.TryGetValue(key, out string value))
                return value;

            Debug.LogWarning($"Key not found: {key}");
            return key;
        }

        [System.Serializable]
        private class LocalizationFile
        {
            public List<LocalizationEntry> entries;
        }

        [System.Serializable]
        private class LocalizationEntry
        {
            public string key;
            public string value;
        }
    }
}