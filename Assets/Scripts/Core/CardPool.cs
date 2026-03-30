using System.Collections.Generic;
using UnityEngine;

namespace ZyngaCardGame.UI
{
    public class CardPool : MonoBehaviour
    {
        [Header("── Card Pool ───────────────────")]
        [SerializeField] private Card _cardPrefab;

        [Space(4)]
        [Header("── Pool Settings ────────────────")]
        [SerializeField] private int _defaultCapacity = 12;

        private List<Card> _available = new List<Card>();

        private void Awake()
        {
            for (int i = 0; i < _defaultCapacity; i++)
                _available.Add(CreateCard());
        }

        private void OnDestroy()
        {
            _available.Clear();
        }

        private Card CreateCard()
        {
            Card card = Instantiate(_cardPrefab);
            card.gameObject.SetActive(false);
            return card;
        }

        public Card Get(Transform parent)
        {
            Card card;

            if (_available.Count > 0)
            {
                card = _available[_available.Count - 1];
                _available.RemoveAt(_available.Count - 1);
            }
            else
            {
                card = CreateCard();
            }

            card.transform.SetParent(parent);
            card.gameObject.SetActive(true);
            return card;
        }

        public void Release(Card card)
        {
            if (card == null) return;
            card.ResetCard();
            card.gameObject.SetActive(false);
            _available.Add(card);
        }
    }
}