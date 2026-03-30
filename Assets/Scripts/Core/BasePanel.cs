using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace ZyngaCardGame.UI
{
    public abstract class BasePanel : MonoBehaviour
    {
        [Header("── Base Panel ──────────────────")]
        [SerializeField] private Image _shadow;
        [SerializeField] private Transform _panel;

        [Space(4)]
        [SerializeField] private Transform[] _animatedElements;

        [Space(8)]
        [Header("── Animation Settings ──────────")]
        [SerializeField] private float _shadowDuration = 0.25f;
        [SerializeField] private float _panelDuration = 0.35f;
        [SerializeField] private float _btnDuration = 0.25f;

        protected virtual void Awake()
        {

        }

        public void Show()
        {
            DOTween.Kill(gameObject);
            gameObject.SetActive(true);

            _shadow.color = Color.clear;
            _panel.localScale = Vector3.zero;

            for (int i = 0; i < _animatedElements.Length; i++)
                _animatedElements[i].localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(_shadow.DOFade(0.65f, _shadowDuration).SetEase(Ease.OutQuad));
            seq.Join(_panel.DOScale(Vector3.one, _panelDuration).SetEase(Ease.OutBack));
            seq.Append(BounceInElements());
        }

        public void Hide()
        {
            DOTween.Kill(gameObject);
            Sequence seq = DOTween.Sequence();

            seq.Append(BounceOutElements());
            seq.Append(_panel.DOScale(Vector3.zero, _panelDuration * 0.8f).SetEase(Ease.InBack));
            seq.Join(_shadow.DOFade(0f, _shadowDuration).SetEase(Ease.InQuad));
            seq.OnComplete(() => gameObject.SetActive(false));
        }

        private Sequence BounceInElements()
        {
            Sequence s = DOTween.Sequence();

            for (int i = 0; i < _animatedElements.Length; i++)
            {
                Sequence elementSeq = DOTween.Sequence();
                elementSeq.Append(_animatedElements[i].DOScale(1.15f, _btnDuration * 0.6f).SetEase(Ease.OutQuad));
                elementSeq.Append(_animatedElements[i].DOScale(1f, _btnDuration * 0.4f).SetEase(Ease.InQuad));
                s.Join(elementSeq);
            }

            return s;
        }

        private Sequence BounceOutElements()
        {
            Sequence s = DOTween.Sequence();

            for (int i = 0; i < _animatedElements.Length; i++)
            {
                Sequence elementSeq = DOTween.Sequence();
                elementSeq.Append(_animatedElements[i].DOScale(1.15f, _btnDuration * 0.4f).SetEase(Ease.OutQuad));
                elementSeq.Append(_animatedElements[i].DOScale(0f, _btnDuration * 0.6f).SetEase(Ease.InQuad));
                s.Join(elementSeq);
            }

            return s;
        }
    }
}