using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class HandLayout : MonoBehaviour
    {
        [Header("── Arc Anchors ──────────────────")]
        [SerializeField] private Transform _leftAnchor;
        [SerializeField] private Transform _middleAnchor;
        [SerializeField] private Transform _rightAnchor;

        [Space(4)]
        [Header("── Group Colors ──────────────────")]
        [SerializeField]
        private Color[] _groupColors = new Color[]
        {
            new Color(0.2f, 0.6f, 1f),
            new Color(0.2f, 0.9f, 0.4f),
            new Color(1f, 0.8f, 0.2f),
            new Color(1f, 0.4f, 0.8f),
        };

        [Space(4)]
        [Header("── Animation ────────────────────")]
        [SerializeField] private float _sortDuration = 0.3f;
        [SerializeField] private float _sortDelay = 0.05f;

        private const int HandSize = 11;
        private const int MaxHandSize = 12;

        //--- ARC

        public int GetSlotCount(int cardCount) => cardCount >= MaxHandSize ? MaxHandSize : HandSize;

        public Vector3 GetArcPosition(int index, int total)
        {
            float t = total <= 1 ? 0.5f : (float)index / (total - 1);
            float u = 1f - t;

            return u * u * _leftAnchor.position
                 + 2f * u * t * _middleAnchor.position
                 + t * t * _rightAnchor.position;
        }

        public float GetArcRotation(int index, int total)
        {
            float t = total <= 1 ? 0.5f : (float)index / (total - 1);
            return Mathf.LerpAngle(_leftAnchor.localEulerAngles.z, _rightAnchor.localEulerAngles.z, t);
        }

        public void RefreshArcPositions(IReadOnlyList<Card> cards, bool animated = false)
        {
            int slotCount = GetSlotCount(cards.Count);
            for (int i = 0; i < cards.Count; i++)
            {
                Vector3 targetPos = GetArcPosition(i, slotCount);
                float targetRot = GetArcRotation(i, slotCount);

                if (animated)
                {
                    cards[i].transform.DOMove(targetPos, _sortDuration).SetEase(Ease.OutBack);
                    cards[i].transform.DOLocalRotate(new Vector3(0, 0, targetRot), _sortDuration).SetEase(Ease.OutQuad);
                }
                else
                {
                    cards[i].transform.position = targetPos;
                    cards[i].transform.localEulerAngles = new Vector3(0, 0, targetRot);
                }
            }
        }

        public void AnimateToOrder(IReadOnlyList<Card> cards, List<CardData> newOrder)
        {
            int slotCount = GetSlotCount(cards.Count);
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].transform.SetSiblingIndex(i);
                int idx = i;
                cards[idx].transform.DOMove(GetArcPosition(idx, slotCount), _sortDuration).SetEase(Ease.OutBack).SetDelay(idx * _sortDelay);
                cards[idx].transform.DOLocalRotate(new Vector3(0, 0, GetArcRotation(idx, slotCount)), _sortDuration).SetEase(Ease.OutQuad).SetDelay(idx * _sortDelay);
            }
        }

        //--- GROUP COLORS

        public void ApplyGroupColors(IReadOnlyList<Card> cards, SortResult result)
        {
            foreach (var card in cards)
                card.ClearGroupColor();

            if (result.Groups == null || result.Groups.Count == 0) return;

            for (int i = 0; i < result.Groups.Count; i++)
            {
                Color color = _groupColors[i % _groupColors.Length];
                foreach (var data in result.Groups[i])
                    cards.FirstOrDefault(c => c.Data == data)?.SetGroupColor(color);
            }
        }

        public void RefreshGroupColors(IReadOnlyList<Card> cards, List<CardData> hand)
        {
            ApplyGroupColors(cards, CardSorter.EvaluateInPlace(hand));
        }

        //--- ANIMATIONS

        public float GetSortDuration(int cardCount) => _sortDuration + (cardCount - 1) * _sortDelay;

        public Sequence ShakeRotation(Transform target, float duration, float angle)
        {
            int steps = Mathf.Max(2, Mathf.RoundToInt(duration / 0.12f));
            float stepDuration = duration / (steps + 1);
            Sequence s = DOTween.Sequence();
            for (int i = 0; i < steps; i++)
            {
                float rot = (i % 2 == 0) ? angle : -angle;
                s.Append(target.DOLocalRotate(new Vector3(0, 0, rot), stepDuration).SetEase(Ease.InOutSine));
            }
            s.Append(target.DOLocalRotate(Vector3.zero, stepDuration).SetEase(Ease.InOutSine));
            return s;
        }
    }
}