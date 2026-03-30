using UnityEngine;
using TMPro;
using DG.Tweening;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class WarningPopup : MonoBehaviour
    {
        [Header("── Warning Popup ───────────────")]
        [SerializeField] private GameObject _popup;
        [SerializeField] private Transform _popupTransform;
        [SerializeField] private TMP_Text _messageText;

        [Space(4)]
        [Header("── Positions ───────────────────")]
        [SerializeField] private Transform _hidePos;
        [SerializeField] private Transform _showPos;

        [Space(4)]
        [Header("── Animation ───────────────────")]
        [SerializeField] private float _slideInDuration = 0.3f;
        [SerializeField] private float _stayDuration = 1f;
        [SerializeField] private float _slideOutDuration = 0.3f;

        private Sequence _sequence;

        private void Awake()
        {
            GameEvents.OnWarningShown += Show;

            _popup.SetActive(false);
            _popupTransform.position = _hidePos.position;
        }

        private void OnDestroy()
        {
            GameEvents.OnWarningShown -= Show;
        }

        private void Show(string message)
        {
            _sequence?.Kill();
            _popupTransform.position = _hidePos.position;
            _messageText.text = message;

            _popup.SetActive(true);

            _sequence = DOTween.Sequence();
            _sequence.Append(_popupTransform.DOMove(_showPos.position, _slideInDuration));
            _sequence.AppendInterval(_stayDuration);
            _sequence.Append(_popupTransform.DOMove(_hidePos.position, _slideOutDuration));
            _sequence.OnComplete(() => { _popup.SetActive(false); });
        }
    }
}