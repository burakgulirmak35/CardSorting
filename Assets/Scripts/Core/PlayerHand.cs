using System.Collections.Generic;
using System.Linq;
using ZyngaCardGame.UI;

namespace ZyngaCardGame.Core
{
    public class PlayerHand
    {
        private readonly List<Card> _cards = new List<Card>();

        public IReadOnlyList<Card> Cards => _cards;
        public List<CardData> Hand => _cards.Select(c => c.Data).ToList();
        public int Count => _cards.Count;

        //--- MANIPULATION

        public void Add(Card card) => _cards.Add(card);

        public void Remove(Card card) => _cards.Remove(card);

        public void Insert(int index, Card card) => _cards.Insert(index, card);

        public void RemoveAt(int index) => _cards.RemoveAt(index);

        public int IndexOf(Card card) => _cards.IndexOf(card);

        public Card Find(System.Func<Card, bool> predicate) => _cards.Find(c => predicate(c));

        public void Reorder(List<Card> newOrder)
        {
            _cards.Clear();
            _cards.AddRange(newOrder);
        }

        public void Clear() => _cards.Clear();

        //--- SCORE

        public int GetScore() => CardSorter.EvaluateInPlace(Hand).UnmatchedPoints;

        public int ScoreAfterDiscardingWorst()
        {
            List<CardData> hand = Hand;
            CardData worst = GetWorstCardData();
            return CardSorter.EvaluateInPlace(hand.Where(c => c != worst).ToList()).UnmatchedPoints;
        }

        //--- WORST CARD

        public CardData GetWorstCardData()
        {
            List<CardData> hand = Hand;
            SortResult result = CardSorter.SortSmart(hand);

            return result.UnmatchedCards.Count > 0
                ? result.UnmatchedCards.OrderByDescending(c => c.Points).First()
                : hand.OrderByDescending(c => c.Points).First();
        }

        public Card GetWorstCard()
        {
            CardData worst = GetWorstCardData();
            return _cards.Find(c => c.Data == worst);
        }
    }
}