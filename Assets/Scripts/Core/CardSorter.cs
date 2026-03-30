using System;
using System.Collections.Generic;
using System.Linq;

namespace ZyngaCardGame.Core
{
    public static class CardSorter
    {
        //--- PUBLIC API

        public static SortResult SortRun(List<CardData> hand)
        {
            List<List<CardData>> groups = FindRuns(hand);
            return BuildResult(hand, groups);
        }

        public static SortResult SortSet(List<CardData> hand)
        {
            List<List<CardData>> groups = FindSets(hand);
            return BuildResult(hand, groups);
        }

        public static SortResult SortSmart(List<CardData> hand)
        {
            SortResult best = null;

            SortResult runOnly = SortRun(hand);
            best = runOnly;

            SortResult setOnly = SortSet(hand);
            if (setOnly.UnmatchedPoints < best.UnmatchedPoints)
                best = setOnly;

            SortResult combined = SortCombined(hand);
            if (combined.UnmatchedPoints < best.UnmatchedPoints)
                best = combined;

            return best;
        }

        //--- RUN (1-2-3)

        private static List<List<CardData>> FindRuns(List<CardData> hand)
        {
            List<List<CardData>> result = new List<List<CardData>>();
            List<CardData> remaining = new List<CardData>(hand);

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                List<CardData> suited = remaining
                    .Where(c => c.Suit == suit)
                    .OrderBy(c => GetRunRankValue(c.Rank))
                    .ToList();

                if (suited.Count < 3) continue;

                List<List<CardData>> runs = ExtractRuns(suited);
                foreach (var run in runs)
                {
                    if (run.Count >= 3)
                    {
                        result.Add(run);
                        foreach (var c in run)
                            remaining.Remove(c);
                    }
                }
            }

            return result;
        }

        private static List<List<CardData>> ExtractRuns(List<CardData> suited)
        {
            List<List<CardData>> runs = new List<List<CardData>>();
            List<CardData> current = new List<CardData> { suited[0] };

            for (int i = 1; i < suited.Count; i++)
            {
                int prev = GetRunRankValue(suited[i - 1].Rank);
                int curr = GetRunRankValue(suited[i].Rank);

                if (curr == prev + 1)
                {
                    current.Add(suited[i]);
                }
                else
                {
                    if (current.Count >= 3)
                        runs.Add(new List<CardData>(current));
                    current = new List<CardData> { suited[i] };
                }
            }

            if (current.Count >= 3)
                runs.Add(current);

            return runs;
        }

        private static int GetRunRankValue(Rank rank)
        {
            switch (rank)
            {
                case Rank.Ace: return 1;
                case Rank.Two: return 2;
                case Rank.Three: return 3;
                case Rank.Four: return 4;
                case Rank.Five: return 5;
                case Rank.Six: return 6;
                case Rank.Seven: return 7;
                case Rank.Eight: return 8;
                case Rank.Nine: return 9;
                case Rank.Ten: return 10;
                case Rank.Jack: return 11;
                case Rank.Queen: return 12;
                case Rank.King: return 13;
                default: return 0;
            }
        }

        //--- SET (7-7-7)

        private static List<List<CardData>> FindSets(List<CardData> hand)
        {
            List<List<CardData>> result = new List<List<CardData>>();
            List<CardData> remaining = new List<CardData>(hand);

            IEnumerable<IGrouping<Rank, CardData>> byRank = remaining.GroupBy(c => c.Rank);

            foreach (IGrouping<Rank, CardData> group in byRank)
            {
                List<CardData> cards = group.ToList();
                if (cards.Count >= 3)
                {
                    List<Suit> distinctSuits = cards.Select(c => c.Suit).Distinct().ToList();
                    if (distinctSuits.Count >= 3)
                    {
                        List<CardData> setCards = cards
                            .GroupBy(c => c.Suit)
                            .Select(g => g.First())
                            .Take(4)
                            .ToList();

                        if (setCards.Count >= 3)
                        {
                            result.Add(setCards);
                            foreach (var c in setCards)
                                remaining.Remove(c);
                        }
                    }
                }
            }

            return result;
        }

        //--- SMART: Run + Set combined

        private static SortResult SortCombined(List<CardData> hand)
        {
            SortResult best = null;

            List<List<CardData>> runGroups = FindRuns(hand);
            List<CardData> afterRun = GetRemaining(hand, runGroups);
            List<List<CardData>> setGroups = FindSets(afterRun);
            List<List<CardData>> combined1 = new List<List<CardData>>(runGroups);
            combined1.AddRange(setGroups);
            best = BuildResult(hand, combined1);

            List<List<CardData>> setGroups2 = FindSets(hand);
            List<CardData> afterSet = GetRemaining(hand, setGroups2);
            List<List<CardData>> runGroups2 = FindRuns(afterSet);
            List<List<CardData>> combined2 = new List<List<CardData>>(setGroups2);
            combined2.AddRange(runGroups2);
            SortResult result2 = BuildResult(hand, combined2);
            if (result2.UnmatchedPoints < best.UnmatchedPoints)
                best = result2;

            SortResult splitResult = TrySplitRuns(hand);
            if (splitResult != null && splitResult.UnmatchedPoints < best.UnmatchedPoints)
                best = splitResult;

            SortResult trimSetResult = TryTrimSets(hand);
            if (trimSetResult != null && trimSetResult.UnmatchedPoints < best.UnmatchedPoints)
                best = trimSetResult;

            return best;
        }

        private static SortResult TryTrimSets(List<CardData> hand)
        {
            SortResult best = null;

            IEnumerable<IGrouping<Rank, CardData>> byRank = hand.GroupBy(c => c.Rank);
            foreach (IGrouping<Rank, CardData> rankGroup in byRank)
            {
                List<CardData> bySuit = rankGroup.GroupBy(c => c.Suit)
                                      .Select(g => g.First()).ToList();
                if (bySuit.Count < 4) continue;

                for (int skip = 0; skip < bySuit.Count; skip++)
                {
                    List<CardData> setCards = bySuit.Where((_, i) => i != skip).ToList();
                    List<List<CardData>> groups = new List<List<CardData>> { setCards };
                    List<CardData> remaining = GetRemaining(hand, groups);

                    List<List<CardData>> runs = FindRuns(remaining);
                    groups.AddRange(runs);
                    List<CardData> afterRuns = GetRemaining(remaining, runs);
                    List<List<CardData>> sets = FindSets(afterRuns);
                    groups.AddRange(sets);

                    SortResult result = BuildResult(hand, groups);
                    if (best == null || result.UnmatchedPoints < best.UnmatchedPoints)
                        best = result;
                }
            }

            return best;
        }

        private static SortResult TrySplitRuns(List<CardData> hand)
        {
            SortResult best = null;

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                List<CardData> suited = hand
                    .Where(c => c.Suit == suit)
                    .OrderBy(c => GetRunRankValue(c.Rank))
                    .ToList();

                if (suited.Count < 3) continue;

                List<List<CardData>> sequences = ExtractAllSequences(suited);

                foreach (List<CardData> seq in sequences)
                {
                    if (seq.Count < 4) continue; // sadece kesilebilir uzun run'lar

                    for (int cutLen = 3; cutLen <= seq.Count - 3; cutLen++)
                    {
                        List<CardData> part1 = seq.Take(cutLen).ToList();
                        List<CardData> part2 = seq.Skip(cutLen).ToList();

                        if (part2.Count < 3) break;

                        List<List<CardData>> groups = new List<List<CardData>> { part1, part2 };
                        List<CardData> remaining = GetRemaining(hand, groups);

                        List<List<CardData>> sets = FindSets(remaining);
                        groups.AddRange(sets);
                        List<CardData> afterSets = GetRemaining(remaining, sets);
                        List<List<CardData>> runs = FindRuns(afterSets);
                        groups.AddRange(runs);

                        SortResult result = BuildResult(hand, groups);
                        if (best == null || result.UnmatchedPoints < best.UnmatchedPoints)
                            best = result;
                    }
                }
            }

            return best;
        }

        private static List<List<CardData>> ExtractAllSequences(List<CardData> suited)
        {
            List<List<CardData>> sequences = new List<List<CardData>>();
            List<CardData> current = new List<CardData> { suited[0] };

            for (int i = 1; i < suited.Count; i++)
            {
                int prev = GetRunRankValue(suited[i - 1].Rank);
                int curr = GetRunRankValue(suited[i].Rank);

                if (curr == prev + 1)
                    current.Add(suited[i]);
                else
                {
                    if (current.Count >= 3) sequences.Add(new List<CardData>(current));
                    current = new List<CardData> { suited[i] };
                }
            }

            if (current.Count >= 3) sequences.Add(current);
            return sequences;
        }

        //--- POSITIONAL — mevcut sırayı koruyarak sadece yanyana grupları bulur

        public static SortResult EvaluateInPlace(List<CardData> orderedHand)
        {
            List<List<CardData>> groups = new List<List<CardData>>();
            HashSet<int> used = new HashSet<int>();

            for (int start = 0; start < orderedHand.Count - 2; start++)
            {
                if (used.Contains(start)) continue;

                List<CardData> run = new List<CardData> { orderedHand[start] };
                Suit suit = orderedHand[start].Suit;
                int lastVal = GetRunRankValue(orderedHand[start].Rank);

                for (int j = start + 1; j < orderedHand.Count; j++)
                {
                    if (used.Contains(j)) break;
                    CardData next = orderedHand[j];
                    if (next.Suit != suit) break;
                    int val = GetRunRankValue(next.Rank);
                    if (val != lastVal + 1) break;
                    run.Add(next);
                    lastVal = val;
                }

                if (run.Count >= 3)
                {
                    groups.Add(run);
                    for (int k = start; k < start + run.Count; k++)
                        used.Add(k);
                }
            }

            for (int start = 0; start < orderedHand.Count - 2; start++)
            {
                if (used.Contains(start)) continue;

                List<CardData> set = new List<CardData> { orderedHand[start] };
                Rank rank = orderedHand[start].Rank;
                HashSet<Suit> suits = new HashSet<Suit> { orderedHand[start].Suit };

                for (int j = start + 1; j < orderedHand.Count && set.Count < 4; j++)
                {
                    if (used.Contains(j)) break;
                    CardData next = orderedHand[j];
                    if (next.Rank != rank) break;
                    if (suits.Contains(next.Suit)) break;
                    set.Add(next);
                    suits.Add(next.Suit);
                }

                if (set.Count >= 3)
                {
                    groups.Add(set);
                    for (int k = start; k < start + set.Count; k++)
                        used.Add(k);
                }
            }

            return BuildResult(orderedHand, groups);
        }

        //--- HELPERS

        private static SortResult BuildResult(List<CardData> hand, List<List<CardData>> groups)
        {
            List<CardData> usedCards = groups.SelectMany(g => g).ToList();
            List<CardData> unmatchedCards = hand.Where(c => !usedCards.Contains(c)).ToList();
            int unmatchedPoints = unmatchedCards.Sum(c => c.Points);

            return new SortResult
            {
                Groups = groups,
                UnmatchedCards = unmatchedCards,
                UnmatchedPoints = unmatchedPoints
            };
        }

        private static List<CardData> GetRemaining(List<CardData> hand, List<List<CardData>> groups)
        {
            List<CardData> used = groups.SelectMany(g => g).ToList();
            return hand.Where(c => !used.Contains(c)).ToList();
        }
    }

    public class SortResult
    {
        public List<List<CardData>> Groups { get; set; }
        public List<CardData> UnmatchedCards { get; set; }
        public int UnmatchedPoints { get; set; }

        public List<CardData> GetFlatOrder()
        {
            List<CardData> result = new List<CardData>();
            foreach (var group in Groups)
                result.AddRange(group);
            result.AddRange(UnmatchedCards);
            return result;
        }
    }
}