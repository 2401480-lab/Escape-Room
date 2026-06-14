using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom
{
    public static class HorrorUITheme
    {
        public static readonly Color PanelBlack = new Color(0.015f, 0.012f, 0.014f, 0.94f);
        public static readonly Color PanelDeep = new Color(0.045f, 0.037f, 0.04f, 0.96f);
        public static readonly Color PanelRed = new Color(0.16f, 0.02f, 0.025f, 0.94f);
        public static readonly Color BloodRed = new Color(0.62f, 0.035f, 0.045f, 1f);
        public static readonly Color DriedBlood = new Color(0.28f, 0.025f, 0.03f, 1f);
        public static readonly Color TextMain = new Color(0.93f, 0.9f, 0.84f, 1f);
        public static readonly Color TextDim = new Color(0.63f, 0.58f, 0.52f, 1f);
        public static readonly Color SickYellow = new Color(0.82f, 0.68f, 0.32f, 1f);

        public static void ApplyPanel(Image image, Color? color = null)
        {
            if (image == null)
            {
                return;
            }

            image.color = color ?? PanelBlack;
        }

        public static void ApplyText(TextMeshProUGUI text, float fontSize, Color? color = null)
        {
            if (text == null)
            {
                return;
            }

            FontHelper.Apply(text);
            text.fontSize = fontSize;
            text.color = color ?? TextMain;
            text.enableWordWrapping = true;
            text.characterSpacing = 1.5f;
        }

        public static void ApplyButton(Button button, Image image = null)
        {
            if (button == null)
            {
                return;
            }

            if (image == null)
            {
                image = button.GetComponent<Image>();
            }

            ApplyPanel(image, PanelRed);

            ColorBlock colors = button.colors;
            colors.normalColor = PanelRed;
            colors.highlightedColor = new Color(0.28f, 0.035f, 0.04f, 1f);
            colors.pressedColor = BloodRed;
            colors.selectedColor = DriedBlood;
            colors.disabledColor = new Color(0.05f, 0.05f, 0.05f, 0.55f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
        }
    }
}
