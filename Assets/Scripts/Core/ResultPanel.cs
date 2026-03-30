using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class ResultPanel : BasePanel
    {
        [Space(12)]
        [Header("── Result Panel ────────────────")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private Button _playAgainButton;

        public System.Action OnPlayAgain;

        protected override void Awake()
        {
            base.Awake();
            _playAgainButton.onClick.AddListener(() =>
            {
                Hide();
                DOVirtual.DelayedCall(0.6f, () => OnPlayAgain?.Invoke());
            });
        }

        private void OnDestroy()
        {
            _playAgainButton.onClick.RemoveAllListeners();
        }

        public void ShowResult(GameResult result, int playerScore, int botScore)
        {
            switch (result)
            {
                case GameResult.Win:
                    _titleText.text = "WIN!";
                    _scoreText.text = $"Congratulations!\nScore: {playerScore}";
                    break;
                case GameResult.Lose:
                    _titleText.text = "LOSE!";
                    _scoreText.text = $"Bot wins!\nBot Score: {botScore}\nYour Score: {playerScore}";
                    break;
                case GameResult.Draw:
                    _titleText.text = "DRAW!";
                    _scoreText.text = $"It's a draw!\nYour Score: {playerScore}\nBot Score: {botScore}";
                    break;
            }
            Show();
        }
    }
}