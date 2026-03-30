using UnityEngine;
using UnityEngine.UI;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _howToPlayButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private HowToPlayPanel _howToPlayPanel;
        [SerializeField] private SettingsPanel _settingsPanel;

        private void Start()
        {
            GameEvents.SceneLoadCompleted();

            _playButton.onClick.AddListener(OnPlayClicked);
            _howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveAllListeners();
            _howToPlayButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
        }

        private void OnPlayClicked() => SceneTransition.Instance.LoadScene("GameScene");
        private void OnHowToPlayClicked() => _howToPlayPanel.Show();
        private void OnSettingsClicked() => _settingsPanel.Show();
    }
}