using System;
using System.Collections.Generic;
using UnityEngine;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.Core
{
    public enum GameResult { Win, Lose, Draw }
    public static class GameEvents
    {
        //--- Scene
        public static event Action OnSceneLoadCompleted;
        public static void SceneLoadCompleted() => OnSceneLoadCompleted?.Invoke();

        //--- Game Flow
        public static event Action<List<CardData>, List<CardData>> OnGameStarted;
        public static void GameStarted(List<CardData> playerCards, List<CardData> botCards) => OnGameStarted?.Invoke(playerCards, botCards);

        public static event Action OnCardsDealt;
        public static void CardsDealt() => OnCardsDealt?.Invoke();

        public static event Action OnPlayAgainRequested;
        public static void PlayAgainRequested() => OnPlayAgainRequested?.Invoke();

        public static event Action OnDealRequested;
        public static void DealRequested() => OnDealRequested?.Invoke();

        public static event Action<List<CardData>> OnDevHandRequested;
        public static void DevHandRequested(List<CardData> hand) => OnDevHandRequested?.Invoke(hand);

        public static event Action OnShowStartScreen;
        public static void ShowStartScreen() => OnShowStartScreen?.Invoke();

        public static event Action<GameResult, int, int> OnGameOver;
        public static void GameOver(GameResult result, int playerScore, int botScore) => OnGameOver?.Invoke(result, playerScore, botScore);

        //--- Deck
        public static event Action OnDeckClicked;
        public static void DeckClicked() => OnDeckClicked?.Invoke();

        public static event Action<Transform, Action> OnCardDealt;
        public static void CardDealt(Transform target, Action onComplete) => OnCardDealt?.Invoke(target, onComplete);

        //--- Discard Pile
        public static event Action OnDiscardPileClicked;
        public static void DiscardPileClicked() => OnDiscardPileClicked?.Invoke();

        //--- Pass Flow
        public static event Action<CardData> OnFirstDiscardOffered;
        public static void FirstDiscardOffered(CardData card) => OnFirstDiscardOffered?.Invoke(card);

        public static event Action OnPlayerPassed;
        public static void PlayerPassed() => OnPlayerPassed?.Invoke();

        public static event Action OnBotPassed;
        public static void BotPassed() => OnBotPassed?.Invoke();

        //--- Player
        public static event Action OnPlayerTurnStarted;
        public static void PlayerTurnStarted() => OnPlayerTurnStarted?.Invoke();

        public static event Action OnCardDrawn;
        public static void CardDrawn() => OnCardDrawn?.Invoke();

        public static event Action<CardData> OnPlayerDiscarded;
        public static void PlayerDiscarded(CardData card) => OnPlayerDiscarded?.Invoke(card);

        public static event Action<ZyngaCardGame.UI.Card> OnPlayerDiscardedView;
        public static void PlayerDiscardedView(ZyngaCardGame.UI.Card view) => OnPlayerDiscardedView?.Invoke(view);

        public static event Action OnPlayerFinished;
        public static void PlayerFinished() => OnPlayerFinished?.Invoke();

        public static event Action<SortType> OnSortRequested;
        public static void SortRequested(SortType sortType) => OnSortRequested?.Invoke(sortType);

        public static event Action OnEvaluateInPlace;
        public static void EvaluateInPlace() => OnEvaluateInPlace?.Invoke();

        public static event Action<int, int> OnScoreChanged;
        public static void ScoreChanged(int current, int target) => OnScoreChanged?.Invoke(current, target);

        public static event Action<bool> OnFinishAvailable;
        public static void FinishAvailable(bool available) => OnFinishAvailable?.Invoke(available);

        public static event Action<string> OnFinishCardChanged;
        public static void FinishCardChanged(string cardName) => OnFinishCardChanged?.Invoke(cardName);

        //--- Bot
        public static event Action OnBotTurnStarted;
        public static void BotTurnStarted() => OnBotTurnStarted?.Invoke();

        public static event Action<int> OnDeckCountChanged;
        public static void DeckCountChanged(int count) => OnDeckCountChanged?.Invoke(count);

        public static event Action<CardData, ZyngaCardGame.UI.Card> OnBotDiscarded;
        public static void BotDiscarded(CardData data, ZyngaCardGame.UI.Card view) => OnBotDiscarded?.Invoke(data, view);

        public static event Action<bool> OnBotTurnCompleted;
        public static void BotTurnCompleted(bool hasGin) => OnBotTurnCompleted?.Invoke(hasGin);

        //--- Warning
        public static event Action<string> OnWarningShown;
        public static void WarningShown(string message) => OnWarningShown?.Invoke(message);
    }
}