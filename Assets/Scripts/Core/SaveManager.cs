using System.IO;
using UnityEngine;

namespace ZyngaCardGame.Core
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SaveFileName = "settings.json";
        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        private SettingsData _data;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public int ThemeIndex
        {
            get => _data.themeIndex;
            set { _data.themeIndex = value; Save(); }
        }

        public bool MusicOn
        {
            get => _data.musicOn;
            set { _data.musicOn = value; Save(); }
        }

        public bool SfxOn
        {
            get => _data.sfxOn;
            set { _data.sfxOn = value; Save(); }
        }

        private void Save()
        {
            string json = JsonUtility.ToJson(_data);
            File.WriteAllText(SavePath, json);
        }

        private void Load()
        {
            if (!File.Exists(SavePath))
            {
                _data = new SettingsData();
                return;
            }

            string json = File.ReadAllText(SavePath);
            _data = JsonUtility.FromJson<SettingsData>(json);
        }

        [System.Serializable]
        private class SettingsData
        {
            public int themeIndex = 0;
            public bool musicOn = true;
            public bool sfxOn = true;
        }
    }
}