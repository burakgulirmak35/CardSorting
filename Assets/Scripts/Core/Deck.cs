using System;
using System.Collections.Generic;
using System.Linq;

namespace ZyngaCardGame.Core
{
    public sealed class Deck
    {
        private readonly List<CardData> _cards;
        private readonly Random _rng;

        public int RemainingCount => _cards.Count;
        public bool IsEmpty => _cards.Count == 0;

        public Deck(int? seed = null)
        {
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
            _cards = BuildDeck();
        }

        public List<CardData> Deal(int count = 11)
        {
            if (count > _cards.Count)
                throw new InvalidOperationException(
                    $"Not enough cards: {_cards.Count} remaining, {count} requested.");

            List<CardData> hand = _cards.GetRange(_cards.Count - count, count);
            _cards.RemoveRange(_cards.Count - count, count);
            return hand;
        }

        public void Shuffle()
        {
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }

        public void Reset()
        {
            _cards.Clear();
            _cards.AddRange(BuildDeck());
        }

        public IReadOnlyList<CardData> PeekAll() => _cards.AsReadOnly();

        public bool RemoveCard(CardData card)
        {
            CardData match = _cards.FirstOrDefault(c => c.Id == card.Id);
            if (match == null) return false;
            _cards.Remove(match);
            return true;
        }

        private List<CardData> BuildDeck()
        {
            List<CardData> deck = new List<CardData>(52);

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    deck.Add(new CardData(suit, rank));

            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }

            return deck;
        }
    }
}