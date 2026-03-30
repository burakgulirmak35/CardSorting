#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.Editor
{
    public class DevHandTool : EditorWindow
    {
        [MenuItem("Zynga/Dev Hands")]
        public static void ShowWindow()
        {
            var window = GetWindow<DevHandTool>("Dev Hands");
            window.minSize = new Vector2(220, 280);
        }

        private void OnGUI()
        {
            GUILayout.Label("Dev Hands", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Only Play Mode.", MessageType.Warning);
                return;
            }

            DrawHandButton("Mixed Groups\n(3 groups, 2 unmatched)", PdfExampleHand());
            DrawHandButton("Perfect Gin\n(0 unmatched)", GinHand());
            DrawHandButton("Run Heavy\n(two runs)", RunOnlyHand());
            DrawHandButton("Set Heavy\n(two sets)", SetOnlyHand());
            DrawHandButton("No Groups\n(high unmatched)", NoGroupHand());

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox("Restarts game with selected hand.", MessageType.Info);
        }

        private void DrawHandButton(string label, List<CardData> hand)
        {
            if (GUILayout.Button(label, GUILayout.Height(40)))
                GameEvents.DevHandRequested(hand);
            EditorGUILayout.Space(2);
        }

        //--- TEST HANDS

        private static CardData C(Suit s, Rank r) => new CardData(s, r);

        // set of Aces x3, set of 4s x4, run 2-3-4 Spades - 3 groups, 2 unmatched (5 Diamonds + leftover)
        private static List<CardData> PdfExampleHand() => new List<CardData>
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

        // set of Queens x4, run A-2-3-4 Spades, run 5-6-7 Hearts - 3 groups, 0 unmatched
        private static List<CardData> GinHand() => new List<CardData>
        {
            C(Suit.Clubs,    Rank.Queen),
            C(Suit.Diamonds, Rank.Queen),
            C(Suit.Hearts,   Rank.Queen),
            C(Suit.Spades,   Rank.Queen),
            C(Suit.Spades,   Rank.Ace),
            C(Suit.Spades,   Rank.Two),
            C(Suit.Spades,   Rank.Three),
            C(Suit.Spades,   Rank.Four),
            C(Suit.Hearts,   Rank.Five),
            C(Suit.Hearts,   Rank.Six),
            C(Suit.Hearts,   Rank.Seven),
        };

        // run A-2-3-4-5 Clubs, run 7-8-9 Hearts - 2 groups, 3 unmatched (K Diamonds, Q Spades, J Hearts)
        private static List<CardData> RunOnlyHand() => new List<CardData>
        {
            C(Suit.Clubs,    Rank.Ace),
            C(Suit.Clubs,    Rank.Two),
            C(Suit.Clubs,    Rank.Three),
            C(Suit.Clubs,    Rank.Four),
            C(Suit.Clubs,    Rank.Five),
            C(Suit.Hearts,   Rank.Seven),
            C(Suit.Hearts,   Rank.Eight),
            C(Suit.Hearts,   Rank.Nine),
            C(Suit.Diamonds, Rank.King),
            C(Suit.Spades,   Rank.Queen),
            C(Suit.Hearts,   Rank.Jack),
        };

        // set of 7s x4, set of Jacks x3 - 2 groups, 4 unmatched (2 Clubs, 5 Hearts, 9 Spades, K Diamonds)
        private static List<CardData> SetOnlyHand() => new List<CardData>
        {
            C(Suit.Hearts,   Rank.Seven),
            C(Suit.Spades,   Rank.Seven),
            C(Suit.Diamonds, Rank.Seven),
            C(Suit.Clubs,    Rank.Seven),
            C(Suit.Hearts,   Rank.Jack),
            C(Suit.Spades,   Rank.Jack),
            C(Suit.Diamonds, Rank.Jack),
            C(Suit.Clubs,    Rank.Two),
            C(Suit.Hearts,   Rank.Five),
            C(Suit.Spades,   Rank.Nine),
            C(Suit.Diamonds, Rank.King),
        };

        // no runs or sets — 0 groups, 11 unmatched
        private static List<CardData> NoGroupHand() => new List<CardData>
        {
            C(Suit.Hearts,   Rank.Two),
            C(Suit.Spades,   Rank.Five),
            C(Suit.Diamonds, Rank.Nine),
            C(Suit.Clubs,    Rank.King),
            C(Suit.Hearts,   Rank.Queen),
            C(Suit.Spades,   Rank.Jack),
            C(Suit.Hearts,   Rank.Eight),
            C(Suit.Clubs,    Rank.Six),
            C(Suit.Hearts,   Rank.Ace),
            C(Suit.Spades,   Rank.Three),
            C(Suit.Clubs,    Rank.Ten),
        };
    }
}
#endif