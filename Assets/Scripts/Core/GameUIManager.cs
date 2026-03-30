using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ZyngaCardGame.Core;
using ZyngaCardGame.UI;

namespace ZyngaCardGame
{
    public class GameUIManager : MonoBehaviour
    {
        [Header("── Panels ──────────────────────")]
        [SerializeField] private SettingsPanel _settingsPanel;
        [SerializeField] private HowToPlayPanel _howToPlayPanel;
        [SerializeField] private ResultPanel _resultPanel;
        [SerializeField] private StartPanel _startPanel;

        [Space(4)]
        [Header("── Menu ─────────────────────────")]
        [SerializeField] private GameMenuPanel _gameMenuPanel;

        [Space(4)]
        [Header("── Score ────────────────────────")]
        [SerializeField] private TMP_Text _scoreText;

        [Space(4)]
        [Header("── Sort Buttons ─────────────────")]
        [SerializeField] private Button _runSortButton;
        [SerializeField] private Button _setSortButton;

        [Space(4)]
        [Header("── Smart Sort Toggle ──────────────")]
        [SerializeField] private Button _smartSortOnButton;
        [SerializeField] private Button _smartSortOffButton;
        [SerializeField] private RectTransform _smartSortVisual;
        [SerializeField] private float _smartSortToggleDuration = 0.1f;

        [Space(4)]
        [Header("── Game Buttons ─────────────────")]
        [SerializeField] private Button _passButton;
        [SerializeField] private Button _finishButton;
        [SerializeField] private TMP_Text _finishCardText;

        private bool _isSmartSort = false;
        private bool _isSmartSortAnimating = false;

        private void Awake()
        {
            GameEvents.OnScoreChanged += UpdateScore;
            GameEvents.OnGameOver += ShowResult;
            GameEvents.OnPlayerDiscarded += OnPlayerDiscarded;
            GameEvents.OnFirstDiscardOffered += OnFirstDiscardOffered;
            GameEvents.OnFinishAvailable += OnFinishAvailable;
            GameEvents.OnFinishCardChanged += OnFinishCardChanged;
            GameEvents.OnCardDrawn += OnCardDrawn;
            GameEvents.OnCardsDealt += OnCardsDealt;
            GameEvents.OnShowStartScreen += OnShowStartScreen;
            GameEvents.OnPlayerPassed += OnPlayerPassed;
            GameEvents.OnDiscardPileClicked += OnDiscardPileClickedUI;
            GameEvents.OnGameStarted += OnGameStarted;
        }

        private void OnDestroy()
        {
            GameEvents.OnScoreChanged -= UpdateScore;
            GameEvents.OnGameOver -= ShowResult;
            GameEvents.OnPlayerDiscarded -= OnPlayerDiscarded;
            GameEvents.OnFirstDiscardOffered -= OnFirstDiscardOffered;
            GameEvents.OnFinishAvailable -= OnFinishAvailable;
            GameEvents.OnFinishCardChanged -= OnFinishCardChanged;
            GameEvents.OnCardDrawn -= OnCardDrawn;
            GameEvents.OnCardsDealt -= OnCardsDealt;
            GameEvents.OnShowStartScreen -= OnShowStartScreen;
            GameEvents.OnPlayerPassed -= OnPlayerPassed;
            GameEvents.OnDiscardPileClicked -= OnDiscardPileClickedUI;
            GameEvents.OnGameStarted -= OnGameStarted;

            _runSortButton.onClick.RemoveAllListeners();
            _setSortButton.onClick.RemoveAllListeners();
            _smartSortOnButton.onClick.RemoveAllListeners();
            _smartSortOffButton.onClick.RemoveAllListeners();
            _passButton.onClick.RemoveAllListeners();
            _finishButton.onClick.RemoveAllListeners();
        }

        private void Start()
        {
            _gameMenuPanel.OnSettingsClicked = () => _settingsPanel.Show();
            _gameMenuPanel.OnHowToPlayClicked = () => _howToPlayPanel.Show();

            _runSortButton.onClick.AddListener(() => { AnimateButton(_runSortButton.transform); GameEvents.SortRequested(SortType.Run); });
            _setSortButton.onClick.AddListener(() => { AnimateButton(_setSortButton.transform); GameEvents.SortRequested(SortType.Set); });

            _smartSortOnButton.onClick.AddListener(() => TrySetSmartSort(true));
            _smartSortOffButton.onClick.AddListener(() => TrySetSmartSort(false));

            _passButton.onClick.AddListener(() => { AnimateButton(_passButton.transform); GameEvents.PlayerPassed(); });
            _finishButton.onClick.AddListener(() => { AnimateButton(_finishButton.transform); GameEvents.PlayerFinished(); });

            _resultPanel.OnPlayAgain = () => GameEvents.PlayAgainRequested();

            _passButton.gameObject.SetActive(false);
            _finishButton.gameObject.SetActive(false);

            _startPanel.gameObject.SetActive(true);

            TrySetSmartSort(false, 0.1f);
        }

        //--- START SCREEN

        private void OnShowStartScreen()
        {
            _passButton.gameObject.SetActive(false);
            _finishButton.gameObject.SetActive(false);
        }

        private void OnGameStarted(System.Collections.Generic.List<ZyngaCardGame.Core.CardData> p,
                                    System.Collections.Generic.List<ZyngaCardGame.Core.CardData> b)
        {
            _passButton.gameObject.SetActive(false);
            _finishButton.gameObject.SetActive(false);
        }

        //--- PASS & FINISH

        private void OnFirstDiscardOffered(CardData card)
        {
            _passButton.gameObject.SetActive(true);
            _passButton.transform.localScale = Vector3.zero;
            _passButton.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        private void OnPlayerPassed()
        {
            _passButton.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    _passButton.gameObject.SetActive(false);
                    ShowFinishButton();
                });
        }

        private void OnDiscardPileClickedUI()
        {
            if (!_passButton.gameObject.activeSelf) return;

            _passButton.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    _passButton.gameObject.SetActive(false);
                    ShowFinishButton();
                });
        }

        private void ShowFinishButton()
        {
            _finishButton.gameObject.SetActive(true);
            _finishButton.transform.localScale = Vector3.zero;
            _finishButton.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            _finishButton.interactable = false;
            if (_finishCardText != null) _finishCardText.text = "";
        }

        private void OnFinishAvailable(bool available)
        {
            if (_finishButton.gameObject.activeSelf)
                _finishButton.interactable = available;
        }

        private void OnFinishCardChanged(string cardName)
        {
            if (_finishCardText != null)
                _finishCardText.text = cardName;
        }

        private void OnPlayerDiscarded(CardData card)
        {
            _finishButton.interactable = false;

            if (!_isSmartSort) return;
            GameEvents.SortRequested(SortType.Smart);
        }

        //--- SMART SORT

        private void OnCardsDealt()
        {
            GameEvents.EvaluateInPlace();
        }

        private void OnCardDrawn()
        {
            if (!_isSmartSort) return;
            GameEvents.SortRequested(SortType.Smart);
        }

        private void TrySetSmartSort(bool toOn, float delay = 0f)
        {
            if (_isSmartSortAnimating) return;
            if (_isSmartSort == toOn) return;

            _isSmartSort = toOn;
            _isSmartSortAnimating = true;

            Transform target = toOn ? _smartSortOnButton.transform : _smartSortOffButton.transform;

            _smartSortVisual.DOKill();
            _smartSortVisual.DOMove(target.position, _smartSortToggleDuration).SetEase(Ease.Linear).OnComplete(() =>
            {
                _isSmartSortAnimating = false;
                if (toOn) GameEvents.SortRequested(SortType.Smart);
            });
        }

        //--- SCORE & RESULT

        private void UpdateScore(int current, int target)
        {
            _scoreText.text = $"{current} / {target}";
            _scoreText.transform.DOKill();
            _scoreText.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1, 0.5f);
        }

        private void ShowResult(GameResult result, int playerScore, int botScore)
        {
            _passButton.gameObject.SetActive(false);
            _finishButton.gameObject.SetActive(false);
            _resultPanel.ShowResult(result, playerScore, botScore);
        }

        private void AnimateButton(Transform btn)
        {
            btn.DOKill();
            btn.localScale = Vector3.one;
            btn.DOPunchScale(Vector3.one * 0.2f, 0.25f, 1, 0.5f);
        }

        public void SetSortButtonsInteractable(bool interactable)
        {
            _runSortButton.interactable = interactable;
            _setSortButton.interactable = interactable;
        }
    }
}