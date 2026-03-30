using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class BotController : MonoBehaviour
    {
        [Header("── Bot Controller ──────────────")]
        [SerializeField] private Card _cardPrefab;

        [Space(4)]
        [Header("── References ───────────────────")]
        [SerializeField] private RectTransform _avatarRoot;

        [Space(4)]
        [Header("── Timing ───────────────────────")]
        [SerializeField] private float _thinkTime = 1.5f;
        [SerializeField] private float _actionDelay = 0.5f;

        private List<CardData> _botHand = new List<CardData>();
        private Deck _deck;

        private void Awake()
        {
            GameEvents.OnGameStarted += OnGameStarted;
            GameEvents.OnBotTurnStarted += OnBotTurnStarted;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnBotTurnStarted -= OnBotTurnStarted;
        }

        public void SetDeck(Deck deck) => _deck = deck;

        private void OnGameStarted(List<CardData> playerCards, List<CardData> botCards)
        {
            _botHand = new List<CardData>(botCards);
        }

        private void OnBotTurnStarted()
        {
            StartCoroutine(BotTurnRoutine());
        }

        //--- BOT TURN

        private IEnumerator BotTurnRoutine()
        {
            yield return new WaitForSeconds(_thinkTime);

            if (_deck == null || _deck.IsEmpty) yield break;

            CardData drawnCard = _deck.Deal(1)[0];
            GameEvents.DeckCountChanged(_deck.RemainingCount);

            bool animDone = false;
            GameEvents.CardDealt(_avatarRoot, () => animDone = true);
            yield return new WaitUntil(() => animDone);

            _botHand.Add(drawnCard);

            yield return new WaitForSeconds(_actionDelay);

            CardData toDiscard = _botHand[Random.Range(0, _botHand.Count)];
            _botHand.Remove(toDiscard);

            Card view = CreateDiscardView(toDiscard);
            GameEvents.BotDiscarded(toDiscard, view);
            GameEvents.BotTurnCompleted(false);
        }

        //--- ANIMATIONS

        private Card CreateDiscardView(CardData data)
        {
            if (_avatarRoot == null) return null;

            Card view = Instantiate(_cardPrefab, _avatarRoot.parent);
            view.Setup(data);
            view.FlipToFront();
            view.transform.position = _avatarRoot.position;
            view.transform.localScale = Vector3.zero;
            view.transform.SetAsLastSibling();

            return view;
        }

        //--- PUBLIC

        public int GetScore() => CardSorter.SortSmart(_botHand).UnmatchedPoints;

        public void ClearHand() => _botHand.Clear();
    }
}