using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class SceneTransition : MonoBehaviour
    {
        public static SceneTransition Instance { get; private set; }

        [SerializeField] private Image _fadePanel;
        [SerializeField] private float _duration;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _fadePanel.gameObject.SetActive(false);

            GameEvents.OnSceneLoadCompleted += FadeIn;
        }

        private void OnDestroy()
        {
            GameEvents.OnSceneLoadCompleted -= FadeIn;
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(TransitionRoutine(sceneName));
        }

        private IEnumerator TransitionRoutine(string sceneName)
        {
            _fadePanel.gameObject.SetActive(true);
            _fadePanel.color = Color.clear;
            yield return _fadePanel.DOFade(1f, _duration).SetEase(Ease.InQuad).WaitForCompletion();
            SceneManager.LoadScene(sceneName);
        }

        private void FadeIn()
        {
            _fadePanel.gameObject.SetActive(true);
            _fadePanel.color = Color.black;
            DOVirtual.DelayedCall(_duration / 3f, () =>
            {
                _fadePanel.DOFade(0f, _duration).SetEase(Ease.OutQuad).OnComplete(() => _fadePanel.gameObject.SetActive(false));
            });
        }
    }
}