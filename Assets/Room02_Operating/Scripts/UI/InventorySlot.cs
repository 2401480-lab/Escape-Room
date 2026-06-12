using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Room02Operating
{
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;

        public void SetItem(ClueData data)
        {
            if (iconImage != null)
            {
                iconImage.sprite = data.thumbnail;
                iconImage.gameObject.SetActive(data.thumbnail != null);
            }
            if (nameText != null)
                nameText.text = data.displayName;

            gameObject.SetActive(true);
        }

        public void Clear()
        {
            if (iconImage != null) iconImage.sprite = null;
            if (nameText != null) nameText.text = "";
        }
    }
}
