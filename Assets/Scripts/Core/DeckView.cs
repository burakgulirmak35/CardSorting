using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class DeckView : MonoBehaviour, IPointerClickHandler
    {
        [Header("── Deck View ───────────────────")]
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private Card _topCard;

        [Space(4)]
        [Header("── Animation ───────────────────")]
        [SerializeField] private float _punchScale = 0.15f;
        [SerializeField] private float _punchDuration = 0.25f;
        [SerializeField] private float _dealScale = 0.8f;
        [SerializeField] private float _dealOpenDuration = 0.15f;
        [SerializeField] private float _dealMoveDuration = 0.4f;

        private void Awake()
        {
            GameEvents.OnCardDealt += OnCardDealt;
        }

        private void OnDestroy()
        {
            GameEvents.OnCardDealt -= OnCardDealt;
        }

        public void UpdateCount(int count)
        {
            _countText.text = count.ToString();
            if (count > 0)
                transform.DOPunchScale(Vector3.one * _punchScale, _punchDuration, 1, 0.5f);
        }

        private void OnCardDealt(Transform target, System.Action onComplete)
        {
            if (_topCard == null) return;

            // Reset — kapalı yüz, kendi pozisyonunda, scale 0
            _topCard.FlipToBack();
            _topCard.transform.localPosition = Vector3.zero;
            _topCard.transform.localScale = Vector3.zero;
            _topCard.gameObject.SetActive(true);

            Sequence seq = DOTween.Sequence();

            // Deck'te hızlıca aç
            seq.Append(_topCard.transform.DOScale(_dealScale, _dealOpenDuration).SetEase(Ease.OutBack));

            // Hedefe git, giderken kapan
            seq.Append(_topCard.transform.DOMove(target.position, _dealMoveDuration).SetEase(Ease.InOutQuad));
            seq.Join(_topCard.transform.DOScale(0f, _dealMoveDuration).SetEase(Ease.InQuad));

            seq.OnComplete(() =>
            {
                _topCard.gameObject.SetActive(false);
                _topCard.transform.localPosition = Vector3.zero;
                _topCard.transform.localScale = Vector3.zero;
                onComplete?.Invoke();
            });
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            GameEvents.DeckClicked();
        }
    }
}