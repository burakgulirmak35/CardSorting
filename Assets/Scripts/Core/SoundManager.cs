using System.Collections;
using UnityEngine;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("SFX Pool")]
        [SerializeField] private AudioSource[] sfxPool;

        [Header("Music")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip[] musicList;
        [SerializeField] private float musicDelay = 2f;

        [Header("Card Sounds")]
        [SerializeField] private AudioClip dealSound;
        [SerializeField] private AudioClip shuffleSound;
        [SerializeField] private AudioClip sortSound;
        [SerializeField] private AudioClip cardPickSound;
        [SerializeField] private AudioClip cardDropSound;

        private int _poolIndex;
        private float _sfxVolume = 1f;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetMusicVolume(SaveManager.Instance.MusicOn ? 1f : 0f);
            SetSfxVolume(SaveManager.Instance.SfxOn ? 1f : 0f);

            if (musicList.Length > 0)
                StartCoroutine(MusicLoop());
        }

        private IEnumerator MusicLoop()
        {
            int index = 0;
            while (true)
            {
                musicSource.clip = musicList[index];
                musicSource.Play();
                yield return new WaitUntil(() => !musicSource.isPlaying);
                yield return new WaitForSeconds(musicDelay);
                index = (index + 1) % musicList.Length;
            }
        }

        public void PlayDeal() => PlaySound(dealSound);
        public void PlayShuffle() => PlaySound(shuffleSound);
        public void PlaySort() => PlaySound(sortSound);
        public void PlayCardPick() => PlaySound(cardPickSound);
        public void PlayCardDrop() => PlaySound(cardDropSound);

        public void SetSfxVolume(float value)
        {
            _sfxVolume = value;
            foreach (var source in sfxPool)
                if (!source.isPlaying)
                    source.volume = value;
        }

        public void SetMusicVolume(float value) => musicSource.volume = value;

        private void PlaySound(AudioClip clip)
        {
            if (clip == null) return;

            AudioSource source = sfxPool[_poolIndex];
            _poolIndex = (_poolIndex + 1) % sfxPool.Length;

            source.spatialBlend = 0f;
            source.Stop();
            source.clip = clip;
            source.volume = _sfxVolume; // ← saklanan değerden al
            source.Play();
        }
    }
}