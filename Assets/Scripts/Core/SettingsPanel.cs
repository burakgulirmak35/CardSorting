using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class SettingsPanel : BasePanel
    {
        [Space(12)]
        [Header("── Settings Panel ───────────────")]
        [SerializeField] private Button _closeButton;

        [Space(4)]
        [Header("── Theme ────────────────────────")]
        [SerializeField] private CardThemeButton _themeButtonPrefab;
        [SerializeField] private Transform _themesContainer;

        [Space(4)]
        [Header("── Sound ───────────────────────")]
        [SerializeField] private Button _musicToggle;
        [SerializeField] private Button _sfxToggle;
        [SerializeField] private TMP_Text _musicToggleText;
        [SerializeField] private TMP_Text _sfxToggleText;

        [Space(4)]
        [SerializeField] private Color _onColor = Color.green;
        [SerializeField] private Color _offColor = Color.gray;

        private CardThemeButton[] _themeButtons;
        private int _selectedThemeIndex = -1;
        private bool _musicOn;
        private bool _sfxOn;

        protected override void Awake()
        {
            base.Awake();
            _closeButton.onClick.AddListener(Hide);
            _musicToggle.onClick.AddListener(ToggleMusic);
            _sfxToggle.onClick.AddListener(ToggleSfx);

            ThemeManager.OnThemeChanged += OnThemeLoaded;
            BuildThemeButtons();
        }

        private void Start()
        {
            _musicOn = SaveManager.Instance.MusicOn;
            _sfxOn = SaveManager.Instance.SfxOn;

            ApplyMusic();
            ApplySfx();

            SelectTheme(SaveManager.Instance.ThemeIndex);
        }

        private void OnThemeLoaded(CardTheme theme)
        {
            if (_themeButtons == null || _selectedThemeIndex < 0) return;
            _themeButtons[_selectedThemeIndex].Setup(theme);
        }

        private void BuildThemeButtons()
        {
            int targetCount = ThemeManager.Instance.ThemeCount;
            int existingCount = _themesContainer.childCount;

            for (int i = existingCount - 1; i >= targetCount; i--)
                Destroy(_themesContainer.GetChild(i).gameObject);

            for (int i = existingCount; i < targetCount; i++)
                Instantiate(_themeButtonPrefab, _themesContainer);

            _themeButtons = new CardThemeButton[targetCount];

            for (int i = 0; i < targetCount; i++)
            {
                _themeButtons[i] = _themesContainer.GetChild(i).GetComponent<CardThemeButton>();
                _themeButtons[i].SetupWithColor(ThemeManager.Instance.GetColor(i));

                int index = i;
                _themeButtons[i].Button.onClick.RemoveAllListeners();
                _themeButtons[i].Button.onClick.AddListener(() => SelectTheme(index));
            }
        }

        private void SelectTheme(int index)
        {
            if (_themeButtons != null && _selectedThemeIndex >= 0 && _themeButtons[_selectedThemeIndex] != null)
                _themeButtons[_selectedThemeIndex].SetSelected(false);

            _selectedThemeIndex = index;
            _themeButtons[_selectedThemeIndex].SetSelected(true);
            SaveManager.Instance.ThemeIndex = index;

            if (ThemeManager.Instance.CurrentTheme != null && ThemeManager.Instance.CurrentThemeIndex == index)
                OnThemeLoaded(ThemeManager.Instance.CurrentTheme);
            else
                ThemeManager.Instance.ApplyTheme(index);
        }

        private void ToggleMusic()
        {
            _musicOn = !_musicOn;
            SaveManager.Instance.MusicOn = _musicOn;
            ApplyMusic();
        }

        private void ToggleSfx()
        {
            _sfxOn = !_sfxOn;
            SaveManager.Instance.SfxOn = _sfxOn;
            ApplySfx();
        }

        private void ApplyMusic()
        {
            _musicToggleText.text = _musicOn ? "ON" : "OFF";
            _musicToggle.GetComponent<Image>().color = _musicOn ? _onColor : _offColor;
            SoundManager.Instance.SetMusicVolume(_musicOn ? 1f : 0f);
        }

        private void ApplySfx()
        {
            _sfxToggleText.text = _sfxOn ? "ON" : "OFF";
            _sfxToggle.GetComponent<Image>().color = _sfxOn ? _onColor : _offColor;
            SoundManager.Instance.SetSfxVolume(_sfxOn ? 1f : 0f);
        }

        private void OnDestroy()
        {
            ThemeManager.OnThemeChanged -= OnThemeLoaded;
            _closeButton.onClick.RemoveAllListeners();
            _musicToggle.onClick.RemoveAllListeners();
            _sfxToggle.onClick.RemoveAllListeners();

            if (_themeButtons != null)
                for (int i = 0; i < _themeButtons.Length; i++)
                    _themeButtons[i].Button.onClick.RemoveAllListeners();
        }
    }
}