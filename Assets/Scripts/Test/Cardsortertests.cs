using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ZyngaCardGame.Core;

#if UNITY_EDITOR

/*
    CardSorterTests
    ---------------
    Tests the three sorting strategies (SortRun, SortSet, SortSmart) and
    the positional evaluator (EvaluateInPlace) used for real-time drag coloring.

    The reference hand (TestHand) is taken from the assignment spec.
    Expected result: 3 groups, 2 unmatched cards, 2 unmatched points.

    Issues hit during development:
    - SortSmart was initially returning 5 unmatched points (picking run-only path).
      Fixed by adding TryTrimSets and TrySplitRuns passes.
    - Ace was forming Q-K-A runs. Enforced low-only Ace in GetRunRankValue.
    - EvaluateInPlace was reordering the hand. Refactored to scan adjacency
      in the original list order without sorting.
    - Duplicate-suit sets were slipping through. Added distinct-suit check in FindSets.
*/

namespace ZyngaCardGame.Tests
{
    [TestFixture]
    public class CardSorterTests
    {
        private static CardData C(Suit suit, Rank rank) => new CardData(suit, rank);

        private static List<CardData> TestHand() => new List<CardData>
        {
            C(Suit.Hearts,   Rank.Ace),
            C(Suit.Spades,   Rank.Two),
            C(Suit.Diamonds, Rank.Five),
            C(Suit.Hearts,   Rank.Four),
            C(Suit.Spades,   Rank.Ace),
            C(Suit.Diamonds, Rank.Three),
            C(Suit.Clubs,    Rank.Four),
            C(Suit.Spades,   Rank.Four),
            C(Suit.Diamonds, Rank.Ace),
            C(Suit.Spades,   Rank.Three),
            C(Suit.Diamonds, Rank.Four),
        };

        // Two runs expected — Spades (A-2-3-4) and Diamonds (3-4-5)
        // Anything else means the greedy suit scan missed a sequence
        [Test]
        public void SortRun_TestHand_FindsTwoGroups()
        {
            List<CardData> hand = TestHand();
            SortResult result = CardSorter.SortRun(hand);

            Assert.AreEqual(2, result.Groups.Count, "Expected two run groups");
        }

        // Spades run should be A-2-3-4, a 4-card sequence
        // Fails if Ace is treated as high or the scan stops early
        [Test]
        public void SortRun_TestHand_SpadesRunIsAce234()
        {
            List<CardData> hand = TestHand();
            SortResult result = CardSorter.SortRun(hand);

            List<CardData> spadesRun = result.Groups.FirstOrDefault(g =>
                g.All(c => c.Suit == Suit.Spades));

            Assert.IsNotNull(spadesRun, "Spades run group should be found");
            Assert.AreEqual(4, spadesRun.Count, "Spades run should have 4 cards (A,2,3,4)");

            List<Rank> ranks = spadesRun.Select(c => c.Rank).OrderBy(r => r).ToList();
            CollectionAssert.Contains(ranks, Rank.Ace);
            CollectionAssert.Contains(ranks, Rank.Two);
            CollectionAssert.Contains(ranks, Rank.Three);
            CollectionAssert.Contains(ranks, Rank.Four);
        }

        // Diamonds run should be 3-4-5, exactly 3 cards
        [Test]
        public void SortRun_TestHand_DiamondsRunIs345()
        {
            List<CardData> hand = TestHand();
            SortResult result = CardSorter.SortRun(hand);

            List<CardData> diamondsRun = result.Groups.FirstOrDefault(g =>
                g.All(c => c.Suit == Suit.Diamonds));

            Assert.IsNotNull(diamondsRun, "Diamonds run group should be found");
            Assert.AreEqual(3, diamondsRun.Count, "Diamonds run should have 3 cards (3,4,5)");
        }

        // 4 cards left over after both runs are claimed
        // Ace of Diamonds, Ace of Hearts, Four of Hearts, Four of Clubs
        [Test]
        public void SortRun_TestHand_FourCardsUnmatched()
        {
            List<CardData> hand = TestHand();
            SortResult result = CardSorter.SortRun(hand);

            Assert.AreEqual(4, result.UnmatchedCards.Count,
                "4 cards should remain unmatched");
        }

        // Nothing to group — all different suits and non-consecutive
        [Test]
        public void SortRun_NoRun_ReturnsAllUnmatched()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Hearts,   Rank.Two),
                C(Suit.Spades,   Rank.Five),
                C(Suit.Diamonds, Rank.Nine),
                C(Suit.Clubs,    Rank.King),
            };
            SortResult result = CardSorter.SortRun(hand);

            Assert.AreEqual(0, result.Groups.Count);
            Assert.AreEqual(4, result.UnmatchedCards.Count);
        }

        // Minimum valid run — exactly 3 consecutive same-suit cards
        [Test]
        public void SortRun_ExactlyThreeConsecutive_FormsSingleGroup()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Hearts, Rank.Five),
                C(Suit.Hearts, Rank.Six),
                C(Suit.Hearts, Rank.Seven),
            };
            SortResult result = CardSorter.SortRun(hand);

            Assert.AreEqual(1, result.Groups.Count);
            Assert.AreEqual(0, result.UnmatchedCards.Count);
        }

        // Two consecutive isn't enough — run minimum is 3
        [Test]
        public void SortRun_TwoCardsConsecutive_NotAGroup()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Clubs,  Rank.Three),
                C(Suit.Clubs,  Rank.Four),
                C(Suit.Hearts, Rank.Jack),
            };
            SortResult result = CardSorter.SortRun(hand);

            Assert.AreEqual(0, result.Groups.Count, "2 consecutive cards do not form a group");
            Assert.AreEqual(3, result.UnmatchedCards.Count);
        }

        // Consecutive ranks don't matter if suits are different
        [Test]
        public void SortRun_DifferentSuitSameRanks_NotARun()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Hearts,   Rank.Five),
                C(Suit.Spades,   Rank.Six),
                C(Suit.Diamonds, Rank.Seven),
            };
            SortResult result = CardSorter.SortRun(hand);

            Assert.AreEqual(0, result.Groups.Count, "Different suits cannot form a run");
        }

        // Long run — all 5 cards should be absorbed into one group
        [Test]
        public void SortRun_LongRun_AllCardsGrouped()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Spades, Rank.Ace),
                C(Suit.Spades, Rank.Two),
                C(Suit.Spades, Rank.Three),
                C(Suit.Spades, Rank.Four),
                C(Suit.Spades, Rank.Five),
            };
            SortResult result = CardSorter.SortRun(hand);

            Assert.AreEqual(1, result.Groups.Count);
            Assert.AreEqual(5, result.Groups[0].Count);
            Assert.AreEqual(0, result.UnmatchedCards.Count);
        }

        // Ace is low only — Q-K-A is not a valid run
        // This tripped us up early on, GetRunRankValue maps Ace=1 explicitly
        [Test]
        public void SortRun_AceIsLowOnly_QueenKingAceNotARun()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Hearts, Rank.Queen),
                C(Suit.Hearts, Rank.King),
                C(Suit.Hearts, Rank.Ace),
            };
            SortResult result = CardSorter.SortRun(hand);

            Assert.AreEqual(0, result.Groups.Count, "Q-K-A should not be a run, Ace is low only");
        }


        // -- SortSet ------------------------------------------------------------

        // Two sets expected — Aces (3 suits) and Fours (all 4 suits)
        [Test]
        public void SortSet_TestHand_FindsTwoGroups()
        {
            List<CardData> hand = TestHand();
            SortResult result = CardSorter.SortSet(hand);

            Assert.AreEqual(2, result.Groups.Count, "Expected two set groups");
        }

        // Aces set — at least 3 different suits
        [Test]
        public void SortSet_TestHand_AcesGroupFound()
        {
            List<CardData> hand = TestHand();
            SortResult result = CardSorter.SortSet(hand);

            List<CardData> acesGroup = result.Groups.FirstOrDefault(g =>
                g.All(c => c.Rank == Rank.Ace));

            Assert.IsNotNull(acesGroup, "Aces set should be found");
            Assert.GreaterOrEqual(acesGroup.Count, 3, "Aces set should have at least 3 cards");
        }

        // Fours set — all 4 suits present in the hand, so group size should be 4
        [Test]
        public void SortSet_TestHand_FoursGroupHasAllFourSuits()
        {
            List<CardData> hand = TestHand();
            SortResult result = CardSorter.SortSet(hand);

            List<CardData> foursGroup = result.Groups.FirstOrDefault(g =>
                g.All(c => c.Rank == Rank.Four));

            Assert.IsNotNull(foursGroup, "Fours set should be found");
            Assert.AreEqual(4, foursGroup.Count, "Fours set should have 4 different suits");
        }

        // 4 cards left over — Two of Spades, Three of Diamonds, Five of Diamonds, Three of Spades
        [Test]
        public void SortSet_TestHand_FourCardsUnmatched()
        {
            List<CardData> hand = TestHand();
            SortResult result = CardSorter.SortSet(hand);

            Assert.AreEqual(4, result.UnmatchedCards.Count,
                "4 cards should remain unmatched");
        }

        // Duplicate suit in same-rank group — only distinct suits count
        // GroupBy suit -> First() handles this, but worth verifying explicitly
        [Test]
        public void SortSet_DuplicateSuit_ExcludedFromGroup()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Hearts,   Rank.Seven),
                C(Suit.Hearts,   Rank.Seven), // duplicate suit
                C(Suit.Spades,   Rank.Seven),
                C(Suit.Diamonds, Rank.Seven),
            };
            SortResult result = CardSorter.SortSet(hand);
            Assert.AreEqual(1, result.Groups.Count);
            List<Suit> suits = result.Groups[0].Select(c => c.Suit).ToList();
            Assert.AreEqual(suits.Count, suits.Distinct().Count(), "Set must not contain duplicate suits");
        }

        // Minimum valid set — exactly 3 different suits
        [Test]
        public void SortSet_ThreeDifferentSuits_FormsSingleSet()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Hearts,   Rank.King),
                C(Suit.Spades,   Rank.King),
                C(Suit.Diamonds, Rank.King),
            };
            SortResult result = CardSorter.SortSet(hand);

            Assert.AreEqual(1, result.Groups.Count);
            Assert.AreEqual(3, result.Groups[0].Count);
            Assert.AreEqual(0, result.UnmatchedCards.Count);
        }

        // All 4 suits — set of 4
        [Test]
        public void SortSet_FourDifferentSuits_FormsSetOf4()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Hearts,   Rank.Jack),
                C(Suit.Spades,   Rank.Jack),
                C(Suit.Diamonds, Rank.Jack),
                C(Suit.Clubs,    Rank.Jack),
            };
            SortResult result = CardSorter.SortSet(hand);

            Assert.AreEqual(1, result.Groups.Count);
            Assert.AreEqual(4, result.Groups[0].Count);
        }

        // Only 2 matching cards — below the 3-card minimum
        [Test]
        public void SortSet_OnlyTwoMatchingCards_NotASet()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Hearts, Rank.Nine),
                C(Suit.Spades, Rank.Nine),
                C(Suit.Clubs,  Rank.Two),
            };
            SortResult result = CardSorter.SortSet(hand);

            Assert.AreEqual(0, result.Groups.Count, "Only 2 matching cards cannot form a set");
        }


        // -- SortSmart ----------------------------------------------------------

        // The key assertion — 2 unmatched points is the optimal result for this hand
        // Run-only gives 27 pts, Set-only gives 13 pts, Smart finds the 2-point combo
        [Test]
        public void SortSmart_TestHand_UnmatchedPointsIs2()
        {
            List<CardData> hand = TestHand();
            SortResult result = CardSorter.SortSmart(hand);

            Assert.AreEqual(2, result.UnmatchedPoints,
                "Smart sort unmatched points should be 2");
        }

        // 2 cards left over — Ace of Hearts and Ace of Diamonds
        [Test]
        public void SortSmart_TestHand_TwoCardsUnmatched()
        {
            List<CardData> hand = TestHand();
            SortResult result = CardSorter.SortSmart(hand);

            Assert.AreEqual(2, result.UnmatchedCards.Count,
                "2 cards should remain unmatched");
        }

        // 3 groups — one Spades run, one Fours set, one Diamonds run
        [Test]
        public void SortSmart_TestHand_FindsThreeGroups()
        {
            List<CardData> hand = TestHand();
            SortResult result = CardSorter.SortSmart(hand);

            Assert.AreEqual(3, result.Groups.Count,
                "Smart sort should find 3 groups");
        }

        // Should never settle for 5+ points when 2 is achievable
        // This caught the bug where TrySplitRuns and TryTrimSets weren't being tried
        [Test]
        public void SortSmart_TestHand_DoesNotPickSuboptimalPath()
        {
            List<CardData> hand = TestHand();
            SortResult result = CardSorter.SortSmart(hand);

            Assert.Less(result.UnmatchedPoints, 5,
                "SortSmart should not pick the 5-point solution when a better one exists");
        }

        // Smart should never be worse than run-only on the same hand
        [Test]
        public void SortSmart_NeverWorserThanRunOnly()
        {
            List<CardData> hand = TestHand();
            SortResult runResult = CardSorter.SortRun(hand);
            SortResult smartResult = CardSorter.SortSmart(hand);

            Assert.LessOrEqual(smartResult.UnmatchedPoints, runResult.UnmatchedPoints,
                "Smart result should be better than or equal to Run-only");
        }

        // Smart should never be worse than set-only on the same hand
        [Test]
        public void SortSmart_NeverWorseThanSetOnly()
        {
            List<CardData> hand = TestHand();
            SortResult setResult = CardSorter.SortSet(hand);
            SortResult smartResult = CardSorter.SortSmart(hand);

            Assert.LessOrEqual(smartResult.UnmatchedPoints, setResult.UnmatchedPoints,
                "Smart result should be better than or equal to Set-only");
        }

        // All cards fit into groups — Gin, 0 unmatched points
        [Test]
        public void SortSmart_AllCardsGrouped_ZeroUnmatchedPoints()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Spades,   Rank.Ace),    // run: Spades A-2-3
                C(Suit.Spades,   Rank.Two),
                C(Suit.Spades,   Rank.Three),
                C(Suit.Hearts,   Rank.Five),   // run: Hearts 5-6-7
                C(Suit.Hearts,   Rank.Six),
                C(Suit.Hearts,   Rank.Seven),
                C(Suit.Diamonds, Rank.King),   // set: Kings x3
                C(Suit.Spades,   Rank.King),
                C(Suit.Clubs,    Rank.King),
                C(Suit.Diamonds, Rank.Nine),   // set: Nines x3
                C(Suit.Hearts,   Rank.Nine),
                C(Suit.Clubs,    Rank.Nine),
            };
            SortResult result = CardSorter.SortSmart(hand);

            Assert.AreEqual(0, result.UnmatchedPoints,
                "When all cards are grouped, unmatched points should be 0");
        }

        // No groups possible — unmatched points should equal full hand value
        [Test]
        public void SortSmart_NoGroupsPossible_UnmatchedEqualsHandTotal()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Hearts,   Rank.Two),
                C(Suit.Spades,   Rank.Five),
                C(Suit.Diamonds, Rank.Nine),
                C(Suit.Clubs,    Rank.King),
            };
            int expected = hand.Sum(c => c.Points);
            SortResult result = CardSorter.SortSmart(hand);

            Assert.AreEqual(expected, result.UnmatchedPoints,
                "With no groups, unmatched points should equal total hand value");
        }


        // -- EvaluateInPlace ----------------------------------------------------

        // Adjacent same-suit run detected, lone King left over
        [Test]
        public void EvaluateInPlace_AdjacentRun_GroupedCorrectly()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Clubs, Rank.Four),
                C(Suit.Clubs, Rank.Five),
                C(Suit.Clubs, Rank.Six),
                C(Suit.Hearts, Rank.King),
            };
            SortResult result = CardSorter.EvaluateInPlace(hand);

            Assert.AreEqual(1, result.Groups.Count, "Adjacent cards should form a run group");
            Assert.AreEqual(1, result.UnmatchedCards.Count);
            Assert.AreEqual(10, result.UnmatchedPoints, "Only the King should remain unmatched");
        }

        // Same three Clubs but with a card inserted — run breaks
        // EvaluateInPlace only checks adjacency, not global best arrangement
        [Test]
        public void EvaluateInPlace_RunBrokenByInterleavedCard_NotGrouped()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Clubs,  Rank.Four),
                C(Suit.Hearts, Rank.King), // breaks adjacency
                C(Suit.Clubs,  Rank.Five),
                C(Suit.Clubs,  Rank.Six),
            };
            SortResult result = CardSorter.EvaluateInPlace(hand);

            Assert.AreEqual(0, result.Groups.Count,
                "Cards with another card in between cannot form a run");
        }

        // Adjacent same-rank set detected, lone Two left over
        [Test]
        public void EvaluateInPlace_AdjacentSet_GroupedCorrectly()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Hearts,   Rank.Seven),
                C(Suit.Spades,   Rank.Seven),
                C(Suit.Diamonds, Rank.Seven),
                C(Suit.Clubs,    Rank.Two),
            };
            SortResult result = CardSorter.EvaluateInPlace(hand);

            Assert.AreEqual(1, result.Groups.Count, "Adjacent cards should form a set group");
            Assert.AreEqual(1, result.UnmatchedCards.Count);
        }

        // Three Sevens with a card in between — set breaks
        [Test]
        public void EvaluateInPlace_SetBrokenByInterleavedCard_NotGrouped()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Hearts,   Rank.Seven),
                C(Suit.Clubs,    Rank.Two),   // breaks adjacency
                C(Suit.Spades,   Rank.Seven),
                C(Suit.Diamonds, Rank.Seven),
            };
            SortResult result = CardSorter.EvaluateInPlace(hand);

            Assert.AreEqual(0, result.Groups.Count,
                "Cards with another card in between cannot form a set");
        }

        // Moving a card between grouped cards breaks the in-place detection
        // This mirrors what happens after a drag-and-drop reorder
        [Test]
        public void EvaluateInPlace_CardDraggedIntoMiddleOfRun_BreaksGroup()
        {
            List<CardData> ordered = new List<CardData>
            {
                C(Suit.Hearts, Rank.King),
                C(Suit.Clubs,  Rank.Four),
                C(Suit.Clubs,  Rank.Five),
                C(Suit.Clubs,  Rank.Six),
            };
            Assert.AreEqual(1, CardSorter.EvaluateInPlace(ordered).Groups.Count);

            List<CardData> disordered = new List<CardData>
            {
                C(Suit.Clubs,  Rank.Four),
                C(Suit.Clubs,  Rank.Five),
                C(Suit.Hearts, Rank.King), // dragged in between
                C(Suit.Clubs,  Rank.Six),
            };
            Assert.AreEqual(0, CardSorter.EvaluateInPlace(disordered).Groups.Count,
                "Clubs 4-5-6 should not group if another card is in between");
        }

        // Empty hand — no crash, all zeroes
        [Test]
        public void EvaluateInPlace_EmptyHand_ReturnsZeroes()
        {
            SortResult result = CardSorter.EvaluateInPlace(new List<CardData>());

            Assert.AreEqual(0, result.Groups.Count);
            Assert.AreEqual(0, result.UnmatchedCards.Count);
            Assert.AreEqual(0, result.UnmatchedPoints);
        }

        // Two cards — can't meet 3-card minimum no matter what
        [Test]
        public void EvaluateInPlace_TwoCardsOnly_NoGroupFormed()
        {
            List<CardData> hand = new List<CardData>
            {
                C(Suit.Spades, Rank.Ace),
                C(Suit.Spades, Rank.Two),
            };
            SortResult result = CardSorter.EvaluateInPlace(hand);

            Assert.AreEqual(0, result.Groups.Count, "2 cards cannot form a group");
        }


        // -- SortResult ---------------------------------------------------------

        // Group cards should appear before unmatched cards in the flat list
        [Test]
        public void SortResult_GetFlatOrder_GroupsBeforeUnmatched()
        {
            List<CardData> hand = TestHand();
            SortResult result = CardSorter.SortSmart(hand);
            List<CardData> flat = result.GetFlatOrder();

            Assert.AreEqual(hand.Count, flat.Count, "Flat order should contain all cards");

            int groupCardCount = result.Groups.Sum(g => g.Count);
            List<CardData> flatGroupPart = flat.Take(groupCardCount).ToList();

            for (int groupIndex = 0; groupIndex < result.Groups.Count; groupIndex++)
            {
                List<CardData> group = result.Groups[groupIndex];
                for (int cardIndex = 0; cardIndex < group.Count; cardIndex++)
                {
                    CardData card = group[cardIndex];
                    CollectionAssert.Contains(flatGroupPart, card,
                        "Group cards should appear in the first section of flat order");
                }
            }
        }

        // No card should be lost or duplicated
        [Test]
        public void SortResult_GetFlatOrder_AllCardsPresent()
        {
            List<CardData> hand = TestHand();
            SortResult result = CardSorter.SortSmart(hand);
            List<CardData> flat = result.GetFlatOrder();

            for (int i = 0; i < hand.Count; i++)
            {
                CardData card = hand[i];
                CollectionAssert.Contains(flat, card, $"Card should not be lost: {card}");
            }
        }


        // -- CardData -----------------------------------------------------------

        // Ace = 1 point
        [Test]
        public void CardData_AcePoints_Is1()
        {
            CardData ace = C(Suit.Hearts, Rank.Ace);
            Assert.AreEqual(1, ace.Points);
        }

        // Jack, Queen, King = 10 points each
        [Test]
        public void CardData_FaceCards_Are10Points()
        {
            Assert.AreEqual(10, C(Suit.Hearts, Rank.Jack).Points);
            Assert.AreEqual(10, C(Suit.Hearts, Rank.Queen).Points);
            Assert.AreEqual(10, C(Suit.Hearts, Rank.King).Points);
        }

        // Numeric cards match their face value
        [Test]
        public void CardData_NumberCards_MatchFaceValue()
        {
            Assert.AreEqual(2, C(Suit.Spades, Rank.Two).Points);
            Assert.AreEqual(5, C(Suit.Spades, Rank.Five).Points);
            Assert.AreEqual(10, C(Suit.Spades, Rank.Ten).Points);
        }

        // Same suit and rank = equal
        [Test]
        public void CardData_SameSuitAndRank_AreEqual()
        {
            CardData a = C(Suit.Clubs, Rank.Nine);
            CardData b = C(Suit.Clubs, Rank.Nine);
            Assert.AreEqual(a, b);
        }

        // Different suit = not equal, even with same rank
        [Test]
        public void CardData_DifferentSuit_NotEqual()
        {
            CardData a = C(Suit.Clubs, Rank.Nine);
            CardData b = C(Suit.Hearts, Rank.Nine);
            Assert.AreNotEqual(a, b);
        }
    }
}

#endif
