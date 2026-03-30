using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ZyngaCardGame.Core;
using ZyngaCardGame.UI;

namespace ZyngaCardGame
{
    public enum GameState
    {
        Idle,
        Dealing,
        FirstDiscardOffer,
        BotFirstDiscardOffer,
        PlayerTurn,
        BotTurn,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        [Header("── References ──────────────────")]
        [SerializeField] private HandController _playerHand;
        [SerializeField] private BotController _botController;
        [SerializeField] private DeckView _deckView;
        [SerializeField] private DiscardArea _discardArea;

        [Space(4)]
        [Header("── Settings ────────────────────")]
        [SerializeField] private int _dealCount = 11;
        [SerializeField] private int _targetScore = 10;

        [Space(4)]
        [Header("── Turn Timer ───────────────────")]
        [SerializeField] private float _turnDuration = 30f;
        [SerializeField] private UnityEngine.UI.Image _timerFill;

        private Deck _deck;
        private Stack<CardData> _discardPile = new Stack<CardData>();
        private bool _playerDrawnThisTurn = false;
        private CardData _firstDiscardCard;

        private Coroutine _timerRoutine;
        private bool _discardPickAllowed = true;
        private bool _isDevHand = false;

        public GameState State { get; private set; } = GameState.Idle;

        private void Awake()
        {
            GameEvents.OnCardsDealt += OnCardsDealt;
            GameEvents.OnDeckClicked += OnDeckClicked;
            GameEvents.OnDiscardPileClicked += OnDiscardPileClicked;
            GameEvents.OnPlayerPassed += OnPlayerPassed;
            GameEvents.OnPlayerDiscarded += OnPlayerDiscarded;
            GameEvents.OnPlayerDiscardedView += OnPlayerDiscardedView;
            GameEvents.OnPlayerFinished += OnPlayerFinished;
            GameEvents.OnBotDiscarded += OnBotDiscarded;
            GameEvents.OnBotTurnCompleted += OnBotTurnCompleted;
            GameEvents.OnSortRequested += OnSortRequested;
            GameEvents.OnPlayAgainRequested += OnPlayAgainRequested;
            GameEvents.OnDealRequested += StartGame;
            GameEvents.OnDevHandRequested += OnDevHandRequested;
            GameEvents.OnDeckCountChanged += OnDeckCountChanged;
        }

        private void Start()
        {
            GameEvents.SceneLoadCompleted();
            GameEvents.ShowStartScreen();
        }

        private void OnDestroy()
        {
            GameEvents.OnCardsDealt -= OnCardsDealt;
            GameEvents.OnDeckClicked -= OnDeckClicked;
            GameEvents.OnDiscardPileClicked -= OnDiscardPileClicked;
            GameEvents.OnPlayerPassed -= OnPlayerPassed;
            GameEvents.OnPlayerDiscarded -= OnPlayerDiscarded;
            GameEvents.OnPlayerDiscardedView -= OnPlayerDiscardedView;
            GameEvents.OnPlayerFinished -= OnPlayerFinished;
            GameEvents.OnBotDiscarded -= OnBotDiscarded;
            GameEvents.OnBotTurnCompleted -= OnBotTurnCompleted;
            GameEvents.OnSortRequested -= OnSortRequested;
            GameEvents.OnPlayAgainRequested -= OnPlayAgainRequested;
            GameEvents.OnDealRequested -= StartGame;
            GameEvents.OnDevHandRequested -= OnDevHandRequested;
            GameEvents.OnDeckCountChanged -= OnDeckCountChanged;
        }

        //--- GAME FLOW

        public void StartGame()
        {
            State = GameState.Dealing;
            _discardPile.Clear();
            _discardArea.Clear();
            _playerDrawnThisTurn = false;
            _discardPickAllowed = true;
            _isDevHand = false;
            _firstDiscardCard = null;
            StopTimer();

            _deck = new Deck();
            _botController.SetDeck(_deck);

            List<CardData> playerCards = _deck.Deal(_dealCount);
            List<CardData> botCards = _deck.Deal(_dealCount);
            _firstDiscardCard = _deck.Deal(1)[0];

            _discardPile.Push(_firstDiscardCard);
            UpdateDeckView();

            GameEvents.GameStarted(playerCards, botCards);
        }

        private void OnPlayAgainRequested()
        {
            _discardArea.Clear();
            _playerHand.ClearHand();
            _botController.ClearHand();
            GameEvents.ShowStartScreen();
        }

        private void OnDevHandRequested(List<CardData> playerCards)
        {
            State = GameState.Dealing;
            _discardPile.Clear();
            _discardArea.Clear();
            _playerDrawnThisTurn = false;
            _discardPickAllowed = true;
            _isDevHand = true;
            _firstDiscardCard = null;
            StopTimer();

            _deck = new Deck();
            _botController.SetDeck(_deck);

            foreach (var card in playerCards)
                _deck.RemoveCard(card);

            List<CardData> botCards = _deck.Deal(_dealCount);
            _firstDiscardCard = _deck.Deal(1)[0];

            _discardPile.Push(_firstDiscardCard);
            UpdateDeckView();

            GameEvents.GameStarted(playerCards, botCards);
        }

        private void OnCardsDealt()
        {
            State = GameState.FirstDiscardOffer;
            BroadcastScore();
            GameEvents.FirstDiscardOffered(_firstDiscardCard);

            if (_isDevHand)
            {
                _isDevHand = false;
                GameEvents.SortRequested(SortType.Smart);
            }
        }

        //--- PASS FLOW

        private void OnPlayerPassed()
        {
            if (State != GameState.FirstDiscardOffer) return;

            State = GameState.BotFirstDiscardOffer;
            _discardPickAllowed = false;
            GameEvents.BotPassed();
            StartPlayerTurn();
        }

        private void StartPlayerTurn(bool cardAlreadyDrawn = false)
        {
            State = GameState.PlayerTurn;
            _playerDrawnThisTurn = cardAlreadyDrawn;
            BroadcastScore();
            CheckFinishAvailable();
            if (!cardAlreadyDrawn)
                GameEvents.PlayerTurnStarted();
            StartTimer();
        }

        //--- DECK & DISCARD

        public void OnDeckClicked()
        {
            if (State == GameState.FirstDiscardOffer)
            {
                GameEvents.WarningShown(LocalizationManager.Instance.GetString("warning_pass_or_take"));
                return;
            }

            if (State != GameState.PlayerTurn)
            {
                GameEvents.WarningShown(LocalizationManager.Instance.GetString("warning_not_your_turn"));
                return;
            }

            if (_playerDrawnThisTurn)
            {
                GameEvents.WarningShown(LocalizationManager.Instance.GetString("warning_already_drew"));
                return;
            }

            if (_deck.IsEmpty) return;

            CardData drawnCard = _deck.Deal(1)[0];
            _playerHand.AddCard(drawnCard);
            UpdateDeckView();
            _playerDrawnThisTurn = true;
            BroadcastScore();
            CheckFinishAvailable();
            CheckDeckEmpty();
        }

        private void OnDiscardPileClicked()
        {
            if (State == GameState.FirstDiscardOffer)
            {
                PickFromDiscard();
                StartPlayerTurn(cardAlreadyDrawn: true);
                return;
            }

            if (State != GameState.PlayerTurn) return;

            if (_playerDrawnThisTurn)
            {
                GameEvents.WarningShown(LocalizationManager.Instance.GetString("warning_already_drew"));
                return;
            }

            if (!_discardPickAllowed) return;

            PickFromDiscard();
        }

        private void PickFromDiscard()
        {
            if (_discardPile.Count == 0) return;

            _discardPile.Pop();

            Card card = _discardArea.PopTopCard();
            if (card != null)
                _playerHand.AddCardView(card);

            _playerDrawnThisTurn = true;
            BroadcastScore();
            CheckFinishAvailable();
        }

        //--- PLAYER DISCARD & FINISH

        private void OnPlayerDiscarded(CardData card)
        {
            if (State != GameState.PlayerTurn) return;

            if (!_playerDrawnThisTurn)
            {
                GameEvents.WarningShown(LocalizationManager.Instance.GetString("warning_draw_first"));
                return;
            }

            StopTimer();
            _discardPickAllowed = true;
            _discardPile.Push(card);
            _playerDrawnThisTurn = false;

            GameEvents.FinishAvailable(false);

            State = GameState.BotTurn;
            StartTimer();
            GameEvents.BotTurnStarted();
        }

        private void OnPlayerDiscardedView(UI.Card view)
        {
            _discardArea.DiscardCard(view);
        }

        private void OnPlayerFinished()
        {
            if (State != GameState.PlayerTurn) return;

            if (!_playerDrawnThisTurn)
            {
                GameEvents.WarningShown(LocalizationManager.Instance.GetString("warning_draw_first"));
                return;
            }

            bool canFinish = _playerHand.GetScore() <= _targetScore
                          || _playerHand.ScoreAfterDiscardingWorst() <= _targetScore;
            if (!canFinish) return;

            StopTimer();
            State = GameState.GameOver;
            GameEvents.FinishAvailable(false);

            _playerHand.DiscardWorstCardWithFinishAnimation(() =>
            {
                EndGame(GameResult.Win);
            });
        }

        //--- BOT

        private void OnBotDiscarded(CardData card, Card view)
        {
            _discardPile.Push(card);
            _discardArea.DiscardCard(view);
        }

        private void OnBotTurnCompleted(bool botHasGin)
        {
            StopTimer();
            if (botHasGin) { EndGame(GameResult.Lose); return; }
            if (_deck.IsEmpty) { EndGame(GameResult.Draw); return; }
            StartPlayerTurn();
        }

        //--- TIMER

        private void StartTimer()
        {
            StopTimer();

            if (_timerFill != null)
            {
                _timerFill.fillAmount = 1f;
                _timerFill.DOFillAmount(0f, _turnDuration).SetEase(Ease.Linear);
            }

            _timerRoutine = StartCoroutine(TimerRoutine());
        }

        private void StopTimer()
        {
            if (_timerFill != null)
            {
                _timerFill.DOKill();
                _timerFill.fillAmount = 1f;
            }

            if (_timerRoutine != null)
            {
                StopCoroutine(_timerRoutine);
                _timerRoutine = null;
            }
        }

        private IEnumerator TimerRoutine()
        {
            yield return new WaitForSeconds(_turnDuration);
            OnTimerExpired();
        }

        private void OnTimerExpired()
        {
            if (State != GameState.PlayerTurn) return;

            if (!_playerDrawnThisTurn && !_deck.IsEmpty)
            {
                CardData card = _deck.Deal(1)[0];
                _playerHand.AddCard(card);
                UpdateDeckView();
                _playerDrawnThisTurn = true;
                BroadcastScore();
            }

            _playerHand.DiscardWorstCard(triggerEvent: true);
        }

        //--- SORT

        private void OnSortRequested(SortType sortType)
        {
            if (State == GameState.GameOver) return;
            _playerHand.ApplySort(sortType);
            BroadcastScore();
            CheckFinishAvailable();
        }

        //--- WIN / LOSE

        private void CheckFinishAvailable()
        {
            if (!_playerDrawnThisTurn) { GameEvents.FinishAvailable(false); return; }

            bool canFinish = _playerHand.GetScore() <= _targetScore || _playerHand.ScoreAfterDiscardingWorst() <= _targetScore;
            GameEvents.FinishAvailable(canFinish);

            if (canFinish)
            {
                CardData worst = _playerHand.GetWorstCard();
                GameEvents.FinishCardChanged(worst != null ? $"Drop {worst.Rank} of {worst.Suit}" : "");
            }
        }

        private void EndGame(GameResult result)
        {
            StopTimer();
            State = GameState.GameOver;
            GameEvents.FinishAvailable(false);
            int playerScore = _playerHand.GetScore();
            int botScore = _botController.GetScore();
            GameEvents.GameOver(result, playerScore, botScore);
        }

        private void CheckDeckEmpty()
        {
            if (_deck.IsEmpty)
                EndGame(GameResult.Draw);
        }

        //--- HELPERS

        private void OnDeckCountChanged(int count)
        {
            if (_deckView != null) _deckView.UpdateCount(count);
        }

        private void BroadcastScore() => GameEvents.ScoreChanged(_playerHand.GetScore(), _targetScore);
        private void UpdateDeckView() => _deckView.UpdateCount(_deck.RemainingCount);
    }
}