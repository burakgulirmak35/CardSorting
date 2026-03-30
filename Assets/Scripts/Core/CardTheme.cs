using UnityEngine;

namespace ZyngaCardGame.Core
{
    [CreateAssetMenu(fileName = "CardTheme", menuName = "ZyngaCard/Card Theme")]
    public class CardTheme : ScriptableObject
    {
        public string themeName;
        public Sprite cardBack;
        [Space(10)]
        [Header("Clubs (2→10,A,J,K,Q)")]
        [Header("Diamonds (2→10,A,J,K,Q)")]
        [Header("Hearts (2→10,A,J,K,Q)")]
        [Header("Spades (2→10,A,J,K,Q)")]
        [Space(10)]
        public Sprite[] cardFaces;
    }
}