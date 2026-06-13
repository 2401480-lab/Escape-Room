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

            // CC center가 (0,1,0)이므로 player.y = floorY 이면 CC 바닥 = floorY
            player.transform.position = new Vector3(0f, floorY, 0f);

            // 바닥 감지 실패(floorY=0)한 경우 안전망으로 투명 Floor 평면 추가
            EnsureFloorCollider(floorY);

            EditorUtility.SetDirty(player);
            Debug.Log($"[Room02] Player 위치 → Y: {player.transform.position.y:F2}");
        }

        // 건물 콜라이더가 없는 경우 대비해 보이지 않는 바닥 콜라이더 생성
        static void EnsureFloorCollider(float floorY)
        {
            if (GameObject.Find("_FloorCollider") != null) return;

            GameObject floor = new GameObject("_FloorCollider");
            Undo.RegisterCreatedObjectUndo(floor, "Create FloorCollider");
            floor.transform.position = new Vector3(0f, floorY - 0.05f, 0f);

            BoxCollider box = floor.AddComponent<BoxCollider>();
            box.size = new Vector3(200f, 0.1f, 200f);
            EditorUtility.SetDirty(floor);
            Debug.Log($"[Room02] 안전망 바닥 콜라이더 생성 Y={floorY - 0.05f:F2}");
        }

        static void FixClues(float globalFloorY)
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

                // 바닥 단서 vs 선반 위 단서 구분
                bool isFloorLevel = child.name.ToLower().Contains("bloodstain") ||
                                    child.name.ToLower().Contains("footprint") ||
                                    child.name.ToLower().Contains("under");
                float desiredOffset = isFloorLevel ? 0.05f : 1.1f;

                // 이 큐브 바로 위에서 Raycast → 각자 바닥 높이 찾기
                Vector3 rayOrigin = new Vector3(child.position.x, child.position.y + 30f, child.position.z);
                float localFloor = globalFloorY;
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f))
                    localFloor = hit.point.y;

                Vector3 pos = child.position;
                pos.y = localFloor + desiredOffset;
                child.position = pos;
                EditorUtility.SetDirty(child.gameObject);
                count++;
            }

            Debug.Log($"[Room02] 큐브 {count}개 개별 위치 조정 완료.");
        }
    }
}
