using UnityEngine;
using UnityEngine.UI;

namespace ZyngaCardGame.UI
{
    public class HowToPlayPanel : BasePanel
    {
        [Space(12)]
        [Header("── How To Play Panel ───────────")]
        [SerializeField] private Button _exitButton;

        protected override void Awake()
        {
            base.Awake();
            _exitButton.onClick.AddListener(Hide);
        }
    }
}