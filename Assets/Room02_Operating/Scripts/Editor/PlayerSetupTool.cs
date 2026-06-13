using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace EscapeRoom
{
    public static class PlayerSetupTool
    {
        [MenuItem("Tools/Room02/Setup Player")]
        public static void SetupPlayer()
        {
            // 이미 있으면 스킵
            GameObject existing = GameObject.Find("Player");
            if (existing != null && existing.GetComponent<PlayerMove>() != null)
            {
                EditorUtility.DisplayDialog("알림", "Player가 이미 존재합니다.", "확인");
                return;
            }

            // ── Player 캡슐 생성 ──────────────────────────────────────
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            Undo.RegisterCreatedObjectUndo(player, "Create Player");

            // PlayerStart 위치가 있으면 거기서 시작
            GameObject playerStart = GameObject.Find("PlayerStart");
            if (playerStart != null)
                player.transform.position = playerStart.transform.position + Vector3.up * 1f;
            else
                player.transform.position = new Vector3(0f, 1f, 0f);

            // 캡슐 메시 렌더러 끄기 (FPS에서 몸통 안 보이게)
            MeshRenderer mr = player.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;

            // CharacterController 먼저 추가 후 에디터에서 미리 center 설정
            CharacterController cc = player.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 1f, 0f);
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.skinWidth = 0.08f;

            // PlayerMove 추가
            player.AddComponent<PlayerMove>();

            // ── Main Camera를 Player 자식으로 ────────────────────────
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                Undo.SetTransformParent(mainCam.transform, player.transform, "Parent Camera to Player");
                mainCam.transform.localPosition = new Vector3(0f, 0.7f, 0f);
                mainCam.transform.localRotation = Quaternion.identity;
            }
            else
            {
                // Main Camera 없으면 새로 만들기
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                Undo.RegisterCreatedObjectUndo(camGo, "Create Main Camera");
                camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                camGo.transform.SetParent(player.transform);
                camGo.transform.localPosition = new Vector3(0f, 0.7f, 0f);
                camGo.transform.localRotation = Quaternion.identity;
            }

            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("[Room02] Player 세팅 완료.");
            EditorUtility.DisplayDialog("완료",
                "Player 세팅 완료!\n\nCtrl+S로 저장 후 플레이 버튼 눌러줘.", "확인");
        }
    }
}
