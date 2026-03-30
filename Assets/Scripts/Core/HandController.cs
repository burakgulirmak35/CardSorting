using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class HandController : MonoBehaviour
    {
        [Header("── References ──────────────────")]
        [SerializeField] private RectTransform _handRoot;
        [SerializeField] private RectTransform _deckPosition;
        [SerializeField] private RectTransform _screenCenter;
        [SerializeField] private RectTransform _discardZone;
        [SerializeField] private CardPool _cardPool;
        [SerializeField] private HandLayout _layout;
        [SerializeField] private DragHandler _dragHandler;

        [Space(4)]
        [Header("── Deal Animation ────────────────")]
        [SerializeField] private float _dealInterval = 0.08f;
        [SerializeField] private float _dealDuration = 0.35f;

        private PlayerHand _hand = new PlayerHand();
        private bool _canDiscard = false;
        private bool _isSorting = false;
        private int _targetScore = 10;

        private void Awake()
        {
            _dragHandler.Init(_hand, _layout);
            _dragHandler.OnCardDiscarded = OnCardDiscardedByDrag;

            GameEvents.OnGameStarted += OnGameStarted;
            GameEvents.OnPlayerTurnStarted += OnPlayerTurnStarted;
            GameEvents.OnScoreChanged += OnScoreChanged;
            GameEvents.OnEvaluateInPlace += OnEvaluateInPlace;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnPlayerTurnStarted -= OnPlayerTurnStarted;
            GameEvents.OnScoreChanged -= OnScoreChanged;
            GameEvents.OnEvaluateInPlace -= OnEvaluateInPlace;

            DOTween.Kill(_handRoot);
        }

        //--- EVENTS

        private void OnEvaluateInPlace()
        {
            _layout.RefreshGroupColors(_hand.Cards, _hand.Hand);
            GameEvents.ScoreChanged(_hand.GetScore(), _targetScore);
        }

        private void OnGameStarted(List<CardData> playerCards, List<CardData> botCards)
        {
            _canDiscard = false;
            _dragHandler.SetCanDiscard(false);
            DealHand(playerCards);
        }

        private void OnPlayerTurnStarted()
        {
            _canDiscard = false;
            _dragHandler.SetCanDiscard(false);
        }

        private void OnScoreChanged(int current, int target) => _targetScore = target;

        //--- DEAL

        private void DealHand(List<CardData> dataList)
        {
            ClearHand();

            foreach (var data in dataList)
            {
                Card card = GetCard();
                card.Setup(data);
                card.transform.position = _deckPosition != null ? _deckPosition.position : _handRoot.position;
                card.transform.localScale = Vector3.zero;
                _hand.Add(card);
            }

            StartCoroutine(DealRoutine());
        }

        private IEnumerator DealRoutine()
        {
            SoundManager.Instance.PlayDeal();
            int count = _hand.Count;

            for (int i = 0; i < count; i++)
            {
                int idx = i;
                Card card = _hand.Cards[idx];
                int slotCount = _layout.GetSlotCount(count);

                card.transform.DOMove(_layout.GetArcPosition(idx, slotCount), _dealDuration).SetEase(Ease.OutBack);
                card.transform.DOLocalRotate(new Vector3(0, 0, _layout.GetArcRotation(idx, slotCount)), _dealDuration).SetEase(Ease.OutQuad);
                card.transform.DOScale(Vector3.one, _dealDuration).SetEase(Ease.OutBack).OnComplete(() => card.FlipToFront());

                yield return new WaitForSeconds(_dealInterval);
            }

            yield return new WaitForSeconds(_dealDuration);
            GameEvents.CardsDealt();
        }

        //--- ADD CARD

        public void AddCard(CardData data)
        {
            Card card = GetCard();
            card.Setup(data);
            card.FlipToFront();
            card.transform.position = _deckPosition != null ? _deckPosition.position : _handRoot.position;
            card.transform.localScale = Vector3.zero;
            _hand.Add(card);

            SoundManager.Instance.PlayCardPick();
            card.transform.DOScale(Vector3.one, _dealDuration).SetEase(Ease.OutBack);
            _layout.RefreshArcPositions(_hand.Cards, animated: true);
            _layout.RefreshGroupColors(_hand.Cards, _hand.Hand);

            SetCanDiscard(true);
            GameEvents.ScoreChanged(_hand.GetScore(), _targetScore);
            DOVirtual.DelayedCall(_dealDuration, () => GameEvents.CardDrawn());
        }

        public void AddCardView(Card card)
        {
            card.ClearDiscarded();
            card.transform.SetParent(_handRoot);
            card.transform.DOScale(Vector3.one, _dealDuration).SetEase(Ease.OutBack);
            _hand.Add(card);

            SetupCardCallbacks(card);
            SoundManager.Instance.PlayCardPick();
            _layout.RefreshArcPositions(_hand.Cards, animated: true);
            _layout.RefreshGroupColors(_hand.Cards, _hand.Hand);

            SetCanDiscard(true);
            GameEvents.ScoreChanged(_hand.GetScore(), _targetScore);
            DOVirtual.DelayedCall(_dealDuration, () => GameEvents.CardDrawn());
        }

        //--- SORT

        public void ApplySort(SortType sortType)
        {
            if (_isSorting && sortType != SortType.Smart) return;
            _isSorting = true;

            List<CardData> hand = _hand.Hand;
            SortResult result = sortType switch
            {
                SortType.Run => CardSorter.SortRun(hand),
                SortType.Set => CardSorter.SortSet(hand),
                SortType.Smart => CardSorter.SortSmart(hand),
                _ => CardSorter.SortSmart(hand)
            };

            List<CardData> flatOrder = result.GetFlatOrder();

            List<Card> reordered = flatOrder
                .Select(data => _hand.Cards.FirstOrDefault(c => c.Data == data))
                .Where(c => c != null)
                .ToList();

            _hand.Reorder(reordered);
            _layout.AnimateToOrder(_hand.Cards, flatOrder);
            _layout.ApplyGroupColors(_hand.Cards, CardSorter.EvaluateInPlace(flatOrder));

            SoundManager.Instance.PlaySort();
            GameEvents.ScoreChanged(_hand.GetScore(), _targetScore);

            float totalDuration = _layout.GetSortDuration(_hand.Count);
            DOVirtual.DelayedCall(totalDuration, () => _isSorting = false);
        }

        //--- DISCARD

        private void OnCardDiscardedByDrag(Card card)
        {
            CardData data = card.Data;
            _hand.Remove(card);
            card.SetDiscarded();

            SetCanDiscard(false);
            SoundManager.Instance.PlayCardDrop();
            _layout.RefreshArcPositions(_hand.Cards, animated: true);
            GameEvents.ScoreChanged(_hand.GetScore(), _targetScore);
            GameEvents.PlayerDiscarded(data);
            GameEvents.PlayerDiscardedView(card);
        }

        public void DiscardWorstCard(bool triggerEvent = false)
        {
            Card card = _hand.GetWorstCard();
            if (card == null) return;

            _hand.Remove(card);
            card.SetDiscarded();

            SoundManager.Instance.PlayCardDrop();
            _layout.RefreshArcPositions(_hand.Cards, animated: true);
            GameEvents.ScoreChanged(_hand.GetScore(), _targetScore);

            if (triggerEvent)
            {
                card.transform.DOMove(_discardZone.position, 0.25f).SetEase(Ease.OutBack);
                card.transform.DOScale(Vector3.zero, 0.25f).SetDelay(0.2f);
                GameEvents.PlayerDiscarded(card.Data);
                GameEvents.PlayerDiscardedView(card);
            }
            else
            {
                card.transform.DOMove(_discardZone.position, 0.25f).SetEase(Ease.OutBack);
                card.transform.DOScale(Vector3.zero, 0.25f).SetDelay(0.2f).OnComplete(() => ReleaseCard(card));
            }
        }

        public void DiscardWorstCardWithFinishAnimation(System.Action onComplete)
        {
            Card card = _hand.GetWorstCard();
            if (card == null) { onComplete?.Invoke(); return; }

            _hand.Remove(card);
            card.SetDiscarded();
            card.FlipToFront();
            card.transform.SetAsLastSibling();

            _layout.RefreshArcPositions(_hand.Cards, animated: true);
            Sequence shakeLoop = _layout.ShakeRotation(card.transform, 1f, 5f).SetLoops(-1).SetEase(Ease.Linear);
            Sequence seq = DOTween.Sequence();
            seq.Append(card.transform.DOMove(_screenCenter.position, 0.5f));
            seq.Join(card.transform.DOScale(1.2f, 0.5f));
            seq.Append(card.transform.DOScale(1.5f, 0.5f));
            seq.AppendCallback(() =>
            {
                shakeLoop.Kill();
                card.transform.DOLocalRotate(Vector3.zero, 0.15f);
            });
            seq.Append(card.transform.DOMove(_discardZone.position, 0.2f));
            seq.Join(card.transform.DOScale(0.8f, 0.2f));
            seq.AppendCallback(() => SoundManager.Instance.PlayCardDrop());
            seq.AppendInterval(0.1f);
            seq.OnComplete(() =>
            {
                GameEvents.PlayerDiscarded(card.Data);
                GameEvents.PlayerDiscardedView(card);
                onComplete?.Invoke();
            });
        }

        //--- PUBLIC GETTERS

        public int GetScore() => _hand.GetScore();
        public int ScoreAfterDiscardingWorst() => _hand.ScoreAfterDiscardingWorst();
        public CardData GetWorstCard() => _hand.GetWorstCardData();
        public List<CardData> GetHand() => _hand.Hand;

        //--- HELPERS

        private void SetCanDiscard(bool value)
        {
            _canDiscard = value;
            _dragHandler.SetCanDiscard(value);
        }

        private Card GetCard()
        {
            Card card = _cardPool.Get(_handRoot);
            SetupCardCallbacks(card);
            return card;
        }

        private void ReleaseCard(Card card) => _cardPool.Release(card);

        private void SetupCardCallbacks(Card card)
        {
            card.OnBeginDragEvent = (c, e) => _dragHandler.OnBeginDrag(c, e);
            card.OnDragEvent = (c, e) => _dragHandler.OnDrag(c, e);
            card.OnEndDragEvent = (c, e) => _dragHandler.OnEndDrag(c, e);
            card.OnPointerEnterEvent = c => _dragHandler.OnPointerEnter(c);
            card.OnPointerExitEvent = c => _dragHandler.OnPointerExit(c);
        }

        public void ClearHand()
        {
            foreach (var c in _hand.Cards)
                if (c != null) ReleaseCard(c);
            _hand.Clear();
        }
    }
}