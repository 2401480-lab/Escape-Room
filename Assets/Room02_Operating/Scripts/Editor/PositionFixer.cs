using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EscapeRoom
{
    public static class PositionFixer
    {
        [MenuItem("Tools/Room02/Fix Player & Clue Positions")]
        public static void FixPositions()
        {
            float floorY = FindFloorY();
            Debug.Log($"[Room02] 감지된 바닥 높이: {floorY}");

            FixPlayer(floorY);
            FixClues(floorY);

            EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("완료",
                $"바닥 높이 {floorY:F2} 감지\nPlayer + 큐브 위치 조정 완료!\n\nCtrl+S 저장 후 플레이 해줘.", "확인");
        }

        // 위에서 아래로 Raycast 쏴서 바닥 높이 찾기
        static float FindFloorY()
        {
            // 여러 지점에서 쏴서 평균 내기
            Vector3[] origins = {
                new Vector3(  0, 50,  0),
                new Vector3(  2, 50,  2),
                new Vector3( -2, 50, -2),
                new Vector3(  3, 50,  0),
                new Vector3(  0, 50,  3),
            };

            float total = 0f;
            int count = 0;
            foreach (var origin in origins)
            {
                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 200f))
                {
                    total += hit.point.y;
                    count++;
                }
            }

            return count > 0 ? total / count : 0f;
        }

        static void FixPlayer(float floorY)
        {
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogWarning("[Room02] Player 오브젝트 없음. Tools > Room02 > Setup Player 먼저 실행.");
                return;
            }

            // CharacterController 높이 절반만큼 올려서 바닥에 딱 붙이기
            CharacterController cc = player.GetComponent<CharacterController>();
            float halfHeight = cc != null ? cc.height * 0.5f : 1f;
            player.transform.position = new Vector3(0f, floorY + halfHeight, 0f);

            EditorUtility.SetDirty(player);
            Debug.Log($"[Room02] Player 위치 → Y: {player.transform.position.y:F2}");
        }

        static void FixClues(float floorY)
        {
            GameObject cluesRoot = GameObject.Find("Clues");
            if (cluesRoot == null)
            {
                Debug.LogWarning("[Room02] Clues 루트 없음.");
                return;
            }

            int count = 0;
            foreach (Transform child in cluesRoot.GetComponentsInChildren<Transform>())
            {
                if (child == cluesRoot.transform) continue;
                if (child.GetComponent<ClueInteractable>() == null) continue;

                // 책상/선반 위 단서는 바닥+1.1, 바닥 단서는 바닥+0.05
                float offsetY = child.name.ToLower().Contains("bloodstain") ||
                                child.name.ToLower().Contains("footprint") ||
                                child.name.ToLower().Contains("under") ? 0.05f : 1.1f;

                Vector3 pos = child.position;
                pos.y = floorY + offsetY;
                child.position = pos;
                EditorUtility.SetDirty(child.gameObject);
                count++;
            }

            Debug.Log($"[Room02] 큐브 {count}개 위치 조정 완료.");
        }
    }
}
