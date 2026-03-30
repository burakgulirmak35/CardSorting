using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class DragHandler : MonoBehaviour
    {
        [Header("── Drag Handler ─────────────────")]
        [SerializeField] private RectTransform _handRoot;
        [SerializeField] private RectTransform _discardZone;
        [SerializeField] private float _discardSnapDistance = 150f;

        [Space(4)]
        [Header("── Hover ────────────────────────")]
        [SerializeField] private float _hoverLift = 40f;

        public Action<Card> OnCardDiscarded;
        public Action<Card, int> OnCardMoved;

        private Card _draggedCard;
        private PlayerHand _hand;
        private HandLayout _layout;
        private bool _canDiscard = false;

        public void Init(PlayerHand hand, HandLayout layout)
        {
            _hand = hand;
            _layout = layout;
        }

        public void SetCanDiscard(bool value) => _canDiscard = value;

        //--- DRAG

        public void OnBeginDrag(Card card, PointerEventData eventData)
        {
            _draggedCard = card;
            card.SetHoverEnabled(false);
            card.ClearGroupColor();
            card.transform.SetAsLastSibling();
            card.transform.DOScale(1.15f, 0.1f);
            card.transform.DOLocalRotate(Vector3.zero, 0.1f);
            SoundManager.Instance.PlayCardPick();
        }

        private bool _previewPending = false;
        private int _pendingIndex = -1;

        // OnDrag fires every frame. PreviewInsert starts a DOMove on every card each call.
        // To prevent this, a 0.15f cooldown is added via _previewPending flag.
        // Index changes that arrive during cooldown are saved to _pendingIndex,
        // and applied once the cooldown ends.
        public void OnDrag(Card card, PointerEventData eventData)
        {
            if (_draggedCard == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_handRoot, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
            card.transform.localPosition = new Vector3(localPoint.x, localPoint.y, 0);

            int slotCount = _layout.GetSlotCount(_hand.Count);
            int newIdx = GetClosestIndex(localPoint, slotCount);
            int oldIdx = _hand.IndexOf(card);

            if (newIdx == oldIdx) return;

            if (_previewPending)
            {
                _pendingIndex = newIdx;
                return;
            }

            ExecutePreview(card, newIdx, slotCount);
        }

        private void ExecutePreview(Card card, int newIdx, int slotCount)
        {
            _previewPending = true;
            _pendingIndex = -1;
            PreviewInsert(card, newIdx, slotCount);

            DOVirtual.DelayedCall(0.15f, () =>
            {
                _previewPending = false;

                // Cooldown sırasında bekleyen bir index var mıydı? Varsa, onu uygula.
                if (_pendingIndex >= 0 && _draggedCard != null)
                {
                    int sc = _layout.GetSlotCount(_hand.Count);
                    ExecutePreview(_draggedCard, _pendingIndex, sc);
                }
            });
        }

        public void OnEndDrag(Card card, PointerEventData eventData)
        {
            if (_draggedCard == null) return;
            _draggedCard = null;

            if (IsNearDiscardZone(card))
            {
                if (_canDiscard)
                {
                    OnCardDiscarded?.Invoke(card);
                }
                else
                {
                    GameEvents.WarningShown(LocalizationManager.Instance.GetString("warning_draw_first"));
                    ReturnCardToHand(card);
                }
            }
            else
            {
                ReturnCardToHand(card);
            }
        }

        private void ReturnCardToHand(Card card)
        {
            for (int i = 0; i < _hand.Count; i++)
                _hand.Cards[i].transform.SetSiblingIndex(i);

            _layout.RefreshArcPositions(_hand.Cards, animated: true);
            card.transform.DOScale(1f, 0.15f).OnComplete(() => card.SetHoverEnabled(true));
            _layout.RefreshGroupColors(_hand.Cards, _hand.Hand);
        }

        //--- HOVER

        public void OnPointerEnter(Card card)
        {
            if (_draggedCard != null) return;
            int idx = _hand.IndexOf(card);
            if (idx < 0) return;
            int slotCount = _layout.GetSlotCount(_hand.Count);
            card.transform.DOMove(_layout.GetArcPosition(idx, slotCount) + Vector3.up * _hoverLift, 0.15f).SetEase(Ease.OutQuad);
            card.transform.DOScale(1.1f, 0.15f);
        }

        public void OnPointerExit(Card card)
        {
            if (_draggedCard == card) return;
            int idx = _hand.IndexOf(card);
            if (idx < 0) return;
            int slotCount = _layout.GetSlotCount(_hand.Count);
            card.transform.DOMove(_layout.GetArcPosition(idx, slotCount), 0.15f).SetEase(Ease.OutQuad);
            card.transform.DOScale(1f, 0.15f);
        }

        //--- HELPERS

        private bool IsNearDiscardZone(Card card)
        {
            if (_discardZone == null) return false;
            return Vector2.Distance(card.transform.position, _discardZone.position) < _discardSnapDistance;
        }

        private int GetClosestIndex(Vector2 localPoint, int slotCount)
        {
            int closest = 0;
            float minDist = float.MaxValue;

            for (int i = 0; i < _hand.Count; i++)
            {
                Vector2 slotLocal = _handRoot.InverseTransformPoint(_layout.GetArcPosition(i, slotCount));
                float dist = Vector2.Distance(localPoint, slotLocal);
                if (dist < minDist) { minDist = dist; closest = i; }
            }

            return closest;
        }

        private void PreviewInsert(Card card, int newIdx, int slotCount)
        {
            int oldIdx = _hand.IndexOf(card);
            if (oldIdx < 0 || oldIdx == newIdx) return;

            card.ClearGroupColor();
            _hand.RemoveAt(oldIdx);
            _hand.Insert(newIdx, card);

            for (int i = 0; i < _hand.Count; i++)
                _hand.Cards[i].transform.SetSiblingIndex(i);

            for (int i = 0; i < _hand.Count; i++)
            {
                if (_hand.Cards[i] == card) continue;
                _hand.Cards[i].transform.DOKill();
                _hand.Cards[i].transform.DOMove(_layout.GetArcPosition(i, slotCount), 0.15f).SetEase(Ease.OutQuad);
                _hand.Cards[i].transform.DOLocalRotate(new Vector3(0, 0, _layout.GetArcRotation(i, slotCount)), 0.15f);
            }
        }
    }
}