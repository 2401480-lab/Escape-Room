using UnityEngine;

namespace EscapeRoom
{
    public static class CluePlacementLayout
    {
        private const float PositionTolerance = 0.05f;
        private static readonly Vector3 DefaultScale = new Vector3(0.6f, 0.6f, 0.6f);

        public static bool TryGetPosition(string clueID, out Vector3 position)
        {
            switch (clueID)
            {
                // 복도
                case "normal_cast_notice":
                    position = new Vector3(0.4f, 0.45f, -2.2f);
                    return true;
                case "normal_memorial_frame":
                    position = new Vector3(1.9f, 0.55f, -3.6f);
                    return true;
                case "normal_conversation_memo":
                    position = new Vector3(-0.4f, 0.45f, -4.8f);
                    return true;

                // 병실
                case "normal_medical_certificate":
                    position = new Vector3(-33.2f, 0.45f, -23.5f);
                    return true;
                case "normal_ward_calendar":
                    position = new Vector3(-30.5f, 0.7f, -22.0f);
                    return true;
                case "clue_hasho_will":
                    position = new Vector3(-36.1f, 0.45f, -24.2f);
                    return true;
                case "key_clue_coldest_place":
                    position = new Vector3(-31.6f, 0.45f, -27.0f);
                    return true;

                // 보관실
                case "key_clue_temperature_warning":
                    position = new Vector3(10.2f, 0.7f, -16.9f);
                    return true;
                case "normal_bong_rebuttal":
                    position = new Vector3(13.5f, 0.45f, -13.2f);
                    return true;
                case "key_clue_fridge_scratches":
                    position = new Vector3(15.8f, 0.55f, -14.8f);
                    return true;

                // 분장실
                case "normal_makeup_toolbox":
                    position = new Vector3(-12.0f, 0.45f, -9.6f);
                    return true;
                case "normal_sumi_memo":
                    position = new Vector3(-7.0f, 0.7f, -7.4f);
                    return true;
                case "clue_makeup_diary":
                    position = new Vector3(-14.5f, 0.45f, -11.2f);
                    return true;

                // 수술실
                case "normal_under_table_space":
                    position = new Vector3(6.4f, 0.45f, -21.6f);
                    return true;
                case "normal_mirror_message":
                    position = new Vector3(8.7f, 0.45f, -23.4f);
                    return true;
                default:
                    position = Vector3.zero;
                    return false;
            }
        }

        public static bool TryGetPosition(ClueData clueData, out Vector3 position)
        {
            if (clueData == null)
            {
                position = Vector3.zero;
                return false;
            }

            return TryGetPosition(clueData.clueID, out position);
        }

        public static Vector3 GetFallbackPosition(int index)
        {
            int column = index % 6;
            int row = index / 6;
            return new Vector3(-2f + (column * 1.25f), 0.45f, -6f - (row * 1.25f));
        }

        public static void ApplyExistingSceneCluePositions()
        {
            GameObject cluesRoot = GameObject.Find("Clues");
            if (cluesRoot == null)
            {
                return;
            }

            foreach (Transform child in cluesRoot.transform)
            {
                if (!TryResolveClueID(child, out string clueID))
                {
                    continue;
                }

                if (!TryGetPosition(clueID, out Vector3 position))
                {
                    continue;
                }

                child.localPosition = position;
                child.localScale = DefaultScale;
                child.gameObject.SetActive(true);

                foreach (Renderer renderer in child.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.enabled = true;
                }
            }
        }

        public static bool SceneNeedsPositionRepair(GameObject cluesRoot)
        {
            if (cluesRoot == null)
            {
                return false;
            }

            foreach (Transform child in cluesRoot.transform)
            {
                if (!TryResolveClueID(child, out string clueID) ||
                    !TryGetPosition(clueID, out Vector3 expectedPosition))
                {
                    continue;
                }

                if (Vector3.Distance(child.localPosition, expectedPosition) > PositionTolerance)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryResolveClueID(Transform clueTransform, out string clueID)
        {
            clueID = string.Empty;
            if (clueTransform == null || !clueTransform.name.StartsWith("Clue_"))
            {
                return false;
            }

            ClueBoxInteractable interactable = clueTransform.GetComponent<ClueBoxInteractable>();
            if (interactable != null && interactable.clueData != null &&
                !string.IsNullOrWhiteSpace(interactable.clueData.clueID))
            {
                clueID = interactable.clueData.clueID;
                return true;
            }

            clueID = clueTransform.name.Substring("Clue_".Length);
            return !string.IsNullOrWhiteSpace(clueID);
        }
    }
}
