using TMPro;
using UnityEngine;

namespace EscapeRoom
{
    public static class FontHelper
    {
        private static TMP_FontAsset cachedFont;

        public static TMP_FontAsset KoreanFont
        {
            get
            {
                if (cachedFont != null) return cachedFont;
                cachedFont = Resources.Load<TMP_FontAsset>("Fonts/MalgunGothic_TMP");
                return cachedFont;
            }
        }

        public static void Apply(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;
            TMP_FontAsset font = KoreanFont;
            if (font != null) tmp.font = font;
        }
    }
}
