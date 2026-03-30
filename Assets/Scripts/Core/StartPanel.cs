using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class StartPanel : MonoBehaviour
    {
        [Header("── Start Panel ─────────────────")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Transform _buttonText;

        [Space(4)]
        [Header("── Animation ───────────────────")]
        [SerializeField] private float _scaleDuration = 0.25f;

        private void Awake()
        {
            GameEvents.OnShowStartScreen += Show;
            GameEvents.OnGameStarted += OnGameStarted;
            _startButton.onClick.AddListener(OnStartClicked);
        }

        private void OnDestroy()
        {
            GameEvents.OnShowStartScreen -= Show;
            GameEvents.OnGameStarted -= OnGameStarted;
            _startButton.onClick.RemoveAllListeners();
        }

        private void OnGameStarted(System.Collections.Generic.List<ZyngaCardGame.Core.CardData> p,
                                    System.Collections.Generic.List<ZyngaCardGame.Core.CardData> b)
        {
            gameObject.SetActive(false);
        }

        private void Show()
        {
            DOTween.Kill(gameObject);
            gameObject.SetActive(true);
            _buttonText.localScale = Vector3.zero;
            _buttonText.DOScale(1f, _scaleDuration).SetEase(Ease.OutBack);
        }

        private void OnStartClicked()
        {
            DOTween.Kill(gameObject);
            _buttonText.DOScale(0f, _scaleDuration * 0.8f).SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    GameEvents.DealRequested();
                });
        }
    }
}