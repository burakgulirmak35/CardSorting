using System;

namespace ZyngaCardGame.Core
{
    public enum Suit
    {
        Clubs = 0,
        Diamonds = 1,
        Hearts = 2,
        Spades = 3
    }

    public enum Rank
    {
        Two = 0,
        Three = 1,
        Four = 2,
        Five = 3,
        Six = 4,
        Seven = 5,
        Eight = 6,
        Nine = 7,
        Ten = 8,
        Ace = 9,
        Jack = 10,
        King = 11,
        Queen = 12
    }

    public sealed class CardData : IEquatable<CardData>
    {
        public Suit Suit { get; }
        public Rank Rank { get; }
        public int Id { get; }
        public int Points { get; }

        public CardData(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
            Id = ((int)suit * 13) + (int)rank;
            Points = CalculatePoints(rank);
        }

        private static int CalculatePoints(Rank rank)
        {
            if (rank == Rank.Ace) return 1;
            if (rank == Rank.Jack || rank == Rank.Queen || rank == Rank.King) return 10;
            return (int)rank + 2;
        }

        public override string ToString() => $"{Rank} of {Suit} ({Points} pt)";

        public bool Equals(CardData other)
        {
            if (other is null) return false;
            return Id == other.Id;
        }

        public override bool Equals(object obj) => Equals(obj as CardData);
        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(CardData left, CardData right)
            => left?.Equals(right) ?? right is null;

        public static bool operator !=(CardData left, CardData right)
            => !(left == right);
    }
}