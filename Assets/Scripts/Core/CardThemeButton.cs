using UnityEngine;
using UnityEngine.UI;
using ZyngaCardGame.Core;

namespace ZyngaCardGame.UI
{
    public class CardThemeButton : MonoBehaviour
    {
        [field: SerializeField] public Button Button { get; private set; }
        [field: SerializeField] public CardTheme Theme { get; private set; }

        [SerializeField] private Image _dotImage;
        [SerializeField] private GameObject _selectedIcon;
        [SerializeField] private GameObject _unSelectedIcon;

        public void Setup(CardTheme theme)
        {
            Theme = theme;
        }

        public void SetupWithColor(Color themeColor)
        {
            _dotImage.color = themeColor;
        }

        public void SetSelected(bool selected)
        {
            _selectedIcon.SetActive(selected);
            _unSelectedIcon.SetActive(!selected);
        }
    }
}