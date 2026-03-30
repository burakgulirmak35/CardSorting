using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace ZyngaCardGame.UI
{
    public class GameMenuPanel : MonoBehaviour
    {
        [Header("── Menu Panel ──────────────────")]
        [SerializeField] private RectTransform _menuPanel;
        [SerializeField] private Button _menuToggleButton;

        [Space(4)]
        [Header("── Toggle Icons ─────────────────")]
        [SerializeField] private GameObject _openIcon;
        [SerializeField] private GameObject _closeIcon;

        [Space(4)]
        [Header("── Menu Buttons ─────────────────")]
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _howToPlayButton;
        [SerializeField] private Button _leaveButton;
        [SerializeField] private RectTransform[] _animatedButtons;

        [Space(4)]
        [Header("── Animation ────────────────────")]
        [SerializeField] private float _menuDuration = 0.25f;
        [SerializeField] private float _btnDuration = 0.2f;
        [SerializeField] private float _btnDelay = 0.1f;

        private bool _menuOpen = false;

        public System.Action OnSettingsClicked;
        public System.Action OnHowToPlayClicked;

        private void Start()
        {
            _menuPanel.gameObject.SetActive(false);
            _openIcon.SetActive(true);
            _closeIcon.SetActive(false);

            _menuToggleButton.onClick.AddListener(ToggleMenu);
            _settingsButton.onClick.AddListener(() => { Close(); OnSettingsClicked?.Invoke(); });
            _howToPlayButton.onClick.AddListener(() => { Close(); OnHowToPlayClicked?.Invoke(); });
            _leaveButton.onClick.AddListener(() => { Close(); SceneTransition.Instance.LoadScene("MainMenuScene"); });
        }

        private void OnDestroy()
        {
            _menuToggleButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
            _howToPlayButton.onClick.RemoveAllListeners();
            _leaveButton.onClick.RemoveAllListeners();
        }

        //--- PUBLIC

        public void Close() => CloseMenu();

        //--- MENU TOGGLE

        private void ToggleMenu()
        {
            if (_menuOpen) CloseMenu();
            else OpenMenu();
        }

        private void OpenMenu()
        {
            _menuPanel.DOKill();
            _menuOpen = true;
            _openIcon.SetActive(false);
            _closeIcon.SetActive(true);

            for (int i = 0; i < _animatedButtons.Length; i++)
                _animatedButtons[i].localScale = Vector3.zero;

            _menuPanel.gameObject.SetActive(true);
            _menuPanel.localScale = new Vector3(1f, 0f, 1f);
            _menuPanel.DOScaleY(1f, _menuDuration).SetEase(Ease.OutBack).OnComplete(AnimateButtonsIn);
        }

        private void CloseMenu()
        {
            _menuPanel.DOKill();
            _menuOpen = false;
            _openIcon.SetActive(true);
            _closeIcon.SetActive(false);
            AnimateButtonsOut();
        }

        private void AnimateButtonsIn()
        {
            for (int i = 0; i < _animatedButtons.Length; i++)
                _animatedButtons[i].DOScale(1f, _btnDuration).SetEase(Ease.OutBack).SetDelay(i * _btnDelay);
        }

        private void AnimateButtonsOut()
        {
            for (int i = _animatedButtons.Length - 1; i >= 0; i--)
            {
                int delay = _animatedButtons.Length - 1 - i;
                _animatedButtons[i].DOScale(0f, _btnDuration).SetEase(Ease.InBack).SetDelay(delay * _btnDelay);
            }

            _menuPanel.DOScaleY(0f, _menuDuration).SetEase(Ease.InBack)
                .SetDelay(_btnDuration)
                .OnComplete(() => _menuPanel.gameObject.SetActive(false));
        }
    }
}