using UnityEngine;
using UnityEditor;
using System.IO;

namespace Room02Operating
{
    // Unity 상단 메뉴 Tools > Room02 > Generate All Clue Assets 클릭하면 전체 생성
    public static class ClueDataGenerator
    {
        private const string OUTPUT_PATH = "Assets/Room02_Operating/ScriptableObjects";

        [MenuItem("Tools/Room02/Generate All Clue Assets")]
        public static void GenerateAll()
        {
            if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
                AssetDatabase.CreateFolder("Assets/Room02_Operating", "ScriptableObjects");

            var clues = new[]
            {
                // ── 구역 1: 입구 로비 (Gatehouse + DayRoom + VisitRoom) ──────────
                new ClueEntry("clue_newspaper",
                    "폐원 전 신문기사",
                    "신문 기사",
                    "낡은 신문 기사. '폐요양병원 환자 하시호 씨, 의문의 사망.' 사건의 시작을 알리는 기록이다.",
                    "신문 기사 살펴보기",
                    false),

                new ClueEntry("clue_visitor_log",
                    "방문자 명단",
                    "방문자 명단",
                    "수술 당일 병원 방문자 기록부. 진세웅의 이름이 적혀 있다. 방문 목적란은 비어 있다.",
                    "방문자 명단 확인하기",
                    true),

                new ClueEntry("clue_incident_report",
                    "사건 일지",
                    "사건 일지",
                    "병원 내부 사건 일지. 유안나가 집도한 수술 중 환자 사망 사례가 여럿 기재되어 있다. 하시호의 이름도 보인다.",
                    "사건 일지 읽기",
                    false),

                // ── 구역 2: 복도/대기실 (HallDownstair + HallUpstair + DiningRoom) ──
                new ClueEntry("clue_cctv_memo",
                    "CCTV 사각지대 메모",
                    "CCTV 메모",
                    "손글씨로 적힌 메모. 수술실 쪽 복도 CCTV가 고장났다는 내용과 함께 수리 예정일이 적혀 있다. 범행 당일과 겹친다.",
                    "메모 읽기",
                    true),

                new ClueEntry("clue_staff_schedule",
                    "수술 당일 직원 배치표",
                    "직원 배치표",
                    "수술 당일 병원 직원 배치표. 수술실 보조 인원이 평소보다 적고, 진세웅의 이름은 없다.",
                    "직원 배치표 확인하기",
                    false),

                new ClueEntry("clue_broken_locker",
                    "부서진 락커",
                    "부서진 락커",
                    "복도 끝 락커 하나가 억지로 열린 흔적이 있다. 안에 병원 직원 가운이 한 벌 없어졌다.",
                    "락커 조사하기",
                    false),

                // ── 구역 3: 병실 (Bedroom + SeclusionRoomA/B) ──────────────────
                new ClueEntry("clue_hasho_photo",
                    "하시호·진세웅 사진",
                    "낡은 사진",
                    "두 청년이 함께 찍은 오래된 사진. 뒷면에 '세웅아, 항상 고마워 — 시호'라고 적혀 있다.",
                    "사진 살펴보기",
                    true),

                new ClueEntry("clue_hasho_diary",
                    "하시호의 일기",
                    "낡은 일기",
                    "하시호의 일기. 마지막 페이지에 '유안나 선생이 내 상태를 숨기고 있다. 무섭다'는 내용이 담겨 있다.",
                    "일기 읽기",
                    true),

                new ClueEntry("clue_medicine_bottle",
                    "하시호 처방 약병",
                    "약병",
                    "하시호에게 처방된 약병. 복용량과 처방 기록이 조작된 흔적이 있다.",
                    "약병 조사하기",
                    false),

                new ClueEntry("clue_letter",
                    "진세웅이 쓴 편지",
                    "편지",
                    "진세웅이 하시호에게 쓴 미완성 편지. '네 원수는 내가 반드시...' 문장에서 끊겨 있다.",
                    "편지 읽기",
                    true),

                // ── 구역 4: 보관실/분장실 (OfficeA + KitchenStock + ExamRoom) ──
                new ClueEntry("clue_poison_bottle",
                    "독약 보관 흔적",
                    "빈 약품 보관함",
                    "약품 보관함에 독성 물질 자리가 비어 있다. 재고 기록에는 분명히 존재했던 약품이다.",
                    "보관함 조사하기",
                    true),

                new ClueEntry("clue_storage_log",
                    "약품 재고 기록부",
                    "재고 기록부",
                    "약품 재고 기록부. 수술 전날 독성 약품 한 병이 반출 기록 없이 사라진 것이 확인된다.",
                    "재고 기록 확인하기",
                    true),

                new ClueEntry("clue_paint_can",
                    "페인트 통",
                    "페인트 통",
                    "수술실 바닥 도색에 사용된 페인트 통. 수술 당일 아직 완전히 건조되지 않은 상태였다.",
                    "페인트 통 조사하기",
                    false),

                new ClueEntry("clue_gloves",
                    "페인트 묻은 장갑",
                    "작업용 장갑",
                    "분장실 구석에서 발견된 장갑. 페인트가 묻어 있고, 바닥과 같은 색이다. 병원 직원용 장갑이 아니다.",
                    "장갑 조사하기",
                    true),

                // ── 구역 5: 수술실 (Surgery + Morgue) ─ 결정적 증거 ──────────
                new ClueEntry("clue_shoe_print",
                    "운동화 페인트 자국",
                    "운동화 자국",
                    "수술대 아래 바닥에 찍힌 운동화 밑창 자국. 수술실 직원이 신는 슬리퍼가 아닌 일반 운동화다.",
                    "발자국 조사하기",
                    true),

                new ClueEntry("clue_toe_print",
                    "발가락 페인트 자국",
                    "발가락 자국",
                    "수술대 아래, 발가락 끝으로 버틴 흔적. 오랫동안 엎드려 숨어 있던 사람이 남긴 자국이다.",
                    "흔적 자세히 보기",
                    true),

                new ClueEntry("clue_under_table_dust",
                    "수술대 아래 먼지 흔적",
                    "수술대 아래 흔적",
                    "수술대 아래 먼지가 사람 형태로 쓸린 흔적. 성인 남성 한 명이 장시간 숨어 있었던 것으로 보인다.",
                    "수술대 아래 조사하기",
                    false),

                new ClueEntry("clue_poison_glass",
                    "독약 컵",
                    "유리컵",
                    "수술실 구석에서 발견된 유리컵. 바닥에 독성 잔여물이 남아 있다. 유안나가 마신 컵이다.",
                    "컵 조사하기",
                    true),

                new ClueEntry("clue_yoanna_body",
                    "유안나 시신",
                    "피해자 유안나",
                    "수술대 위에 쓰러진 유안나. 외상은 없으나 입 주변에 미세한 독성 반응이 보인다.",
                    "피해자 살펴보기",
                    false),
            };

            int created = 0;
            foreach (var entry in clues)
            {
                string assetPath = $"{OUTPUT_PATH}/{entry.clueID}.asset";
                if (AssetDatabase.LoadAssetAtPath<ClueData>(assetPath) != null)
                    continue;

                var asset = ScriptableObject.CreateInstance<ClueData>();
                asset.clueID       = entry.clueID;
                asset.displayName  = entry.displayName;
                asset.description  = entry.description;
                asset.hoverLabel   = entry.hoverLabel;
                asset.isCollectable = entry.isCollectable;

                AssetDatabase.CreateAsset(asset, assetPath);
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Room02] ClueData 에셋 {created}개 생성 완료. (이미 존재하는 항목은 건너뜀)");
            EditorUtility.DisplayDialog("완료", $"ClueData 에셋 {created}개 생성 완료!", "확인");
        }

        private struct ClueEntry
        {
            public string clueID;
            public string displayName;
            public string shortName;
            public string description;
            public string hoverLabel;
            public bool   isCollectable;

            public ClueEntry(string id, string display, string shortName, string desc, string hover, bool collect)
            {
                clueID       = id;
                displayName  = display;
                this.shortName = shortName;
                description  = desc;
                hoverLabel   = hover;
                isCollectable = collect;
            }
        }
    }
}
