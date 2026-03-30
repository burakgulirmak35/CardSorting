using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ZyngaCardGame.Core;
using DG.Tweening;

namespace ZyngaCardGame.UI
{
    public class Card : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [Header("── Card ─────────────────────────")]
        [SerializeField] private Image _cardFront;
        [SerializeField] private Image _cardBack;
        [SerializeField] private Image _groupColor;

        public CardData Data { get; private set; }

        public Action<Card, PointerEventData> OnBeginDragEvent;
        public Action<Card, PointerEventData> OnDragEvent;
        public Action<Card, PointerEventData> OnEndDragEvent;
        public Action<Card> OnPointerEnterEvent;
        public Action<Card> OnPointerExitEvent;

        private bool _hoverEnabled = true;
        private bool _isDiscarded = false;

        private void Awake()
        {
            ThemeManager.OnThemeChanged += OnThemeChanged;
            if (Data != null) ApplyTheme(ThemeManager.Instance.CurrentTheme);
        }

        private void OnDestroy()
        {
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        private void OnEnable()
        {
            if (Data != null) ApplyTheme(ThemeManager.Instance.CurrentTheme);
        }

        public void Setup(CardData data)
        {
            Data = data;
            ApplyTheme(ThemeManager.Instance.CurrentTheme);
            FlipToBack();
        }

        public void ApplyTheme(CardTheme theme)
        {
            _cardFront.sprite = theme.cardFaces[Data.Id];
            _cardBack.sprite = theme.cardBack;
        }

        private void OnThemeChanged(CardTheme theme)
        {
            if (Data != null) ApplyTheme(theme);
        }

        public void FlipToFront()
        {
            _cardFront.gameObject.SetActive(true);
            _cardBack.gameObject.SetActive(false);
        }

        public void FlipToBack()
        {
            _cardFront.gameObject.SetActive(false);
            _cardBack.gameObject.SetActive(true);
        }

        public void SetHoverEnabled(bool enabled) => _hoverEnabled = enabled;

        public void SetGroupColor(Color color)
        {
            if (_groupColor == null) return;
            color.a = 50f / 255f;
            _groupColor.color = color;
            _groupColor.gameObject.SetActive(true);
        }

        public void ClearGroupColor()
        {
            if (_groupColor == null) return;
            _groupColor.gameObject.SetActive(false);
        }

        public void SetDiscarded()
        {
            _isDiscarded = true;
            enabled = false;
        }

        public void ClearDiscarded()
        {
            _isDiscarded = false;
            _hoverEnabled = true;
            enabled = true;
        }

        //--- Handlers

        public void OnBeginDrag(PointerEventData e)
        {
            if (_isDiscarded) return;
            OnBeginDragEvent?.Invoke(this, e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (_isDiscarded) return;
            OnDragEvent?.Invoke(this, e);
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (_isDiscarded) return;
            OnEndDragEvent?.Invoke(this, e);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (_isDiscarded || !_hoverEnabled) return;
            OnPointerEnterEvent?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (_isDiscarded || !_hoverEnabled) return;
            OnPointerExitEvent?.Invoke(this);
        }

        public void ResetCard()
        {
            this.DOKill();
            transform.DOKill();
            ClearGroupColor();
            ClearDiscarded();
            OnBeginDragEvent = null;
            OnDragEvent = null;
            OnEndDragEvent = null;
            OnPointerEnterEvent = null;
            OnPointerExitEvent = null;
        }
    }
}