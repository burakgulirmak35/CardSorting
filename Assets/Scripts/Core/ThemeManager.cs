using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class ThemeManager : MonoBehaviour
    {
        public static ThemeManager Instance { get; private set; }

        public static event Action<CardTheme> OnThemeChanged;

        public CardTheme CurrentTheme { get; private set; }

        [SerializeField] private List<Color> _colors;

        [Header("── Theme Addressable References ───")]
        [SerializeField] private List<AssetReferenceT<CardTheme>> _themeRefs;

        private AsyncOperationHandle<CardTheme> _currentHandle;
        private bool _hasHandle = false;

        public int ThemeCount => _themeRefs.Count;
        public int CurrentThemeIndex { get; private set; } = -1;

        public Color GetColor(int index)
        {
            if (index < 0 || index >= _colors.Count) return Color.white;
            return _colors[index];
        }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ApplyTheme(SaveManager.Instance.ThemeIndex);
        }

        private void OnDestroy()
        {
            ReleaseCurrentHandle();
        }

        public void ApplyTheme(int index)
        {
            if (index < 0 || index >= _themeRefs.Count) return;
            CurrentThemeIndex = index;
            LoadTheme(_themeRefs[index]);
        }

        private void LoadTheme(AssetReferenceT<CardTheme> reference)
        {
            AsyncOperationHandle<CardTheme> handle = reference.LoadAssetAsync<CardTheme>();
            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    ReleaseCurrentHandle();

                    _currentHandle = op;
                    _hasHandle = true;
                    CurrentTheme = op.Result;
                    OnThemeChanged?.Invoke(CurrentTheme);
                }
                else
                {
                    Debug.LogError($"Theme could not be loaded: {reference.RuntimeKey}");
                }
            };
        }

        private void ReleaseCurrentHandle()
        {
            if (_hasHandle && _currentHandle.IsValid())
            {
                Addressables.Release(_currentHandle);
                _hasHandle = false;
            }
        }

        public AssetReferenceT<CardTheme> GetThemeRef(int index) => _themeRefs[index];
    }
}