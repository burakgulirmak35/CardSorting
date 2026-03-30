using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class DiscardArea : MonoBehaviour, IPointerClickHandler
    {
        [Header("── Discard Pile ────────────────")]
        [SerializeField] private Transform _discardArea;
        [SerializeField] private Card _cardPrefab;

        [Space(4)]
        [Header("── Animation ───────────────────")]
        [SerializeField] private float _dropDuration = 0.2f;

        [Space(4)]
        [Header("── Random Offset ────────────────")]
        [SerializeField] private float _maxPositionOffset = 8f;
        [SerializeField] private float _maxRotationOffset = 10f;
        [SerializeField] private float _discardScale = 0.8f;

        [Space(4)]
        [Header("── Pile Limit ───────────────────")]
        [SerializeField] private int _maxVisibleCards = 8;

        private List<Card> _pile = new List<Card>();

        private void Awake()
        {
            GameEvents.OnFirstDiscardOffered += DiscardFirstCard;
        }

        private void OnDestroy()
        {
            GameEvents.OnFirstDiscardOffered -= DiscardFirstCard;
        }

        private void DiscardFirstCard(CardData data)
        {
            TrimPile();

            float randomX = Random.Range(-_maxPositionOffset, _maxPositionOffset);
            float randomY = Random.Range(-_maxPositionOffset, _maxPositionOffset);
            float randomRot = Random.Range(-_maxRotationOffset, _maxRotationOffset);

            Card newCard = Instantiate(_cardPrefab, _discardArea);
            newCard.Setup(data);
            newCard.FlipToFront();
            newCard.SetDiscarded();

            newCard.transform.DOLocalMove(new Vector3(randomX, randomY), _dropDuration).SetEase(Ease.OutBack);
            newCard.transform.DOLocalRotate(new Vector3(0, 0, randomRot), _dropDuration).SetEase(Ease.OutBack);
            newCard.transform.DOScale(Vector3.one * _discardScale, _dropDuration).SetEase(Ease.OutBack);

            _pile.Add(newCard);
        }

        public void DiscardCard(Card card)
        {
            if (card == null) return;

            TrimPile();

            float randomX = Random.Range(-_maxPositionOffset, _maxPositionOffset);
            float randomY = Random.Range(-_maxPositionOffset, _maxPositionOffset);
            float randomRot = Random.Range(-_maxRotationOffset, _maxRotationOffset);

            card.transform.SetParent(_discardArea);
            card.FlipToFront();
            card.SetDiscarded();

            card.transform.DOLocalMove(new Vector3(randomX, randomY), _dropDuration).SetEase(Ease.OutBack);
            card.transform.DOLocalRotate(new Vector3(0, 0, randomRot), _dropDuration).SetEase(Ease.OutBack);
            card.transform.DOScale(Vector3.one * _discardScale, _dropDuration).SetEase(Ease.OutBack);

            _pile.Add(card);
        }

        private void TrimPile()
        {
            if (_pile.Count >= _maxVisibleCards)
            {
                Destroy(_pile[0].gameObject);
                _pile.RemoveAt(0);
            }
        }

        public CardData GetTopCard()
        {
            if (_pile.Count == 0) return null;
            return _pile[_pile.Count - 1].Data;
        }

        public Card GetTopCardView()
        {
            if (_pile.Count == 0) return null;
            return _pile[_pile.Count - 1];
        }

        public Card PopTopCard()
        {
            if (_pile.Count == 0) return null;
            Card top = _pile[_pile.Count - 1];
            _pile.RemoveAt(_pile.Count - 1);
            top.ClearDiscarded();
            return top;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            GameEvents.DiscardPileClicked();
        }

        public void Clear()
        {
            foreach (var v in _pile)
                if (v != null) Destroy(v.gameObject);
            _pile.Clear();
        }
    }
}