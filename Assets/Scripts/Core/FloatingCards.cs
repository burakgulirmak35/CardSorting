using UnityEngine;
using DG.Tweening;

namespace ZyngaCardGame.UI
{
    public class FloatingCards : MonoBehaviour
    {
        [Header("── Floating Cards ──────────────")]
        [SerializeField] private RectTransform[] _cards;

        [Space(4)]
        [Header("── Animation Settings ──────────")]
        [SerializeField] private float _floatAmount = 10f;
        [SerializeField] private float _floatDuration = 1.2f;
        [SerializeField] private float _rotateAmount = 5f;
        [SerializeField] private float _rotateDuration = 1.5f;

        private Vector3[] _startPositions;
        private Vector3[] _startRotations;

        private void Start()
        {
            _startPositions = new Vector3[_cards.Length];
            _startRotations = new Vector3[_cards.Length];

            for (int i = 0; i < _cards.Length; i++)
            {
                _startPositions[i] = _cards[i].localPosition;
                _startRotations[i] = _cards[i].localEulerAngles;

                float randomDelay = Random.Range(0.2f, 1f);
                Animate(i, randomDelay);
            }
        }

        private void Animate(int index, float delay)
        {
            float randomRotate = Random.Range(-_rotateAmount, _rotateAmount);
            _cards[index].DOLocalMoveY(_startPositions[index].y + _floatAmount, _floatDuration).SetEase(Ease.InOutSine).SetDelay(delay).SetLoops(-1, LoopType.Yoyo);
            _cards[index].DOLocalRotate(new Vector3(0f, 0f, _startRotations[index].z + randomRotate), _rotateDuration).SetEase(Ease.InOutSine).SetDelay(delay).SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _cards.Length; i++)
                _cards[i].DOKill();
        }
    }
}