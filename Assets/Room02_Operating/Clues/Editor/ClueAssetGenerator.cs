using UnityEditor;
using UnityEngine;

namespace EscapeRoom.Editor
{
    public static class ClueAssetGenerator
    {
        private const string NormalPath = "Assets/Room02_Operating/Clues/Normal";
        private const string KeyCluePath = "Assets/Room02_Operating/Clues/KeyClue";

        [MenuItem("Tools/Room02/Clues/Generate Story Clue Assets")]
        public static void GenerateStoryClueAssets()
        {
            EnsureFolders();

            int created = 0;
            int updated = 0;

            foreach (ClueEntry entry in GetEntries())
            {
                string folder = entry.category == ClueCategory.KeyClue ? KeyCluePath : NormalPath;
                string assetPath = $"{folder}/{entry.fileName}.asset";
                ClueData asset = AssetDatabase.LoadAssetAtPath<ClueData>(assetPath);

                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<ClueData>();
                    AssetDatabase.CreateAsset(asset, assetPath);
                    created++;
                }
                else
                {
                    updated++;
                }

                asset.clueID = entry.clueID;
                asset.clueName = entry.clueName;
                asset.description = entry.description;
                asset.meaning = entry.meaning;
                asset.areaName = entry.zone;
                asset.category = entry.category;
                asset.isRequired = entry.isRequired;
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Clues] Story ClueData assets generated. Created: {created}, Updated: {updated}");
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("단서 생성 완료", $"단서 에셋 생성 {created}개, 갱신 {updated}개 완료", "확인");
            }
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Room02_Operating/Clues"))
            {
                AssetDatabase.CreateFolder("Assets/Room02_Operating", "Clues");
            }

            if (!AssetDatabase.IsValidFolder(NormalPath))
            {
                AssetDatabase.CreateFolder("Assets/Room02_Operating/Clues", "Normal");
            }

            if (!AssetDatabase.IsValidFolder(KeyCluePath))
            {
                AssetDatabase.CreateFolder("Assets/Room02_Operating/Clues", "KeyClue");
            }
        }

        internal static ClueEntry[] GetEntries()
        {
            return new[]
            {
                new ClueEntry("normal_cast_notice", "Clue_배역안내문", "배역 안내문",
                    "오늘 공연 참여자 5명 명단. 유안나, 진세웅, 봉태현, 문수미, 오세진.",
                    "등장인물 파악. 하시호 이름이 없다는 게 나중에 의미를 가짐.",
                    "Lobby", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_memorial_frame", "Clue_추모액자", "추모 액자",
                    "故 하시호 — 2년 전 이 병원 수술실에서 숨지다.",
                    "사건의 발단. 하시호의 죽음이 이번 사건과 연결되는 복선.",
                    "Lobby", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_security_log", "Clue_경비일지", "경비 일지",
                    "수술실 구역 22:00 이후 출입 금지. 오늘 날짜 기재.",
                    "진세웅의 알리바이와 나중에 모순 발생.",
                    "Lobby", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_production_plan", "Clue_공연기획서", "공연 기획서",
                    "오늘 체험 공연의 시나리오 개요. 기획자 이름: 진세웅.",
                    "진세웅이 이 공연을 직접 기획했음을 암시.",
                    "Lobby", ClueCategory.General, isRequired: false),

                new ClueEntry("normal_torn_letter_a", "Clue_찢긴편지조각A", "찢긴 편지 조각 A",
                    "반쪽짜리 편지. '내가 반드시'까지만 읽힘.",
                    "편지 조각 B와 합쳐야 내용 완성.",
                    "Hallway", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_torn_letter_b", "Clue_찢긴편지조각B", "찢긴 편지 조각 B",
                    "나머지 반쪽. '복수하겠다 — 세웅'.",
                    "A+B 합치면 진세웅의 복수 동기 확정.",
                    "Hallway", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_yoanna_memo", "Clue_메모지", "메모지",
                    "'봉태현, 당신 그날 어디 있었어?' — 유안나 필체.",
                    "봉태현 의심 유도. 미스디렉션.",
                    "Hallway", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_cctv_notice", "Clue_대기실CCTV안내문", "대기실 CCTV 안내문",
                    "'카메라 고장 중 — 수리 예정'.",
                    "진세웅이 사전에 CCTV를 고장낸 것임을 나중에 알게 됨.",
                    "Hallway", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_sumi_memo", "Clue_문수미의메모", "문수미의 메모",
                    "'세웅이가 이상해. 뭔가 계획하고 있는 것 같아 — 수미'.",
                    "진세웅 의심 복선. 주변인도 눈치챘음.",
                    "Hallway", ClueCategory.General, isRequired: false),

                new ClueEntry("clue_hasho_will", "Clue_하시호유서", "하시호 유서",
                    "'내 죽음은 의료 과실이 아니야. 나는 살해당한 거야.'",
                    "사건을 단순 사고에서 살인으로 재정의하는 전환점.",
                    "Ward", ClueCategory.General, isRequired: true),
                new ClueEntry("normal_medical_certificate", "Clue_진단서", "진단서",
                    "하시호 사인: 수술 중 심정지. 담당의: 봉태현.",
                    "봉태현 의심 강화. 미스디렉션 핵심.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_conversation_memo", "Clue_대화메모", "대화 메모",
                    "'태현 씨, 당신이 하시호를 죽인 거 알아' — 유안나.",
                    "봉태현 미스디렉션 강화.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_bong_rebuttal", "Clue_봉태현의반박문", "봉태현의 반박문",
                    "'나는 최선을 다했다. 하시호의 죽음은 불가항력이었다.'",
                    "봉태현이 범인이 아님을 암시하는 복선.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_ward_calendar", "Clue_병실달력", "병실 달력",
                    "2년 전 날짜에 하시호 수술일 표시, 오늘 날짜에 별표.",
                    "진세웅이 2년을 기다려온 복수임을 암시.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_oh_threat_memo", "Clue_오세진협박메모", "오세진 협박 메모",
                    "'그 남자한테 협박당했다. 기록을 지우지 않으면 나도 죽인다고 했다.'",
                    "오세진은 공범이 아닌 피해자. 진세웅이 협박한 것.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_deleted_entry_trace", "Clue_출입기록삭제흔적", "출입 기록 삭제 흔적",
                    "2년 전 수술일 당직 기록이 지워진 흔적. 담당: 오세진.",
                    "오세진이 진세웅에게 협박당해 기록을 삭제했음 확정.",
                    "Ward", ClueCategory.General, isRequired: false),

                new ClueEntry("normal_poison_ampoule", "Clue_독약앰플", "독약 앰플",
                    "라벨 없는 약병. 간호사실 재고 부족 기록과 연결.",
                    "범행 도구.",
                    "Storage", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_hidden_camera", "Clue_소형카메라", "소형 카메라",
                    "수술실 방향 고정 거치. 저장 영상 삭제됨.",
                    "진세웅의 사전 계획 증거.",
                    "Storage", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_jin_sneakers", "Clue_운동화", "운동화",
                    "밑창 흰 페인트. 발가락 쪽에만 묻어 있음.",
                    "핵심 물증 A. 수술대 아래 숨었다는 증거.",
                    "Storage", ClueCategory.General, isRequired: true),
                new ClueEntry("normal_gloves", "Clue_장갑", "장갑",
                    "독약 앰플 옆에 보관. 손가락 끝 페인트 없음.",
                    "장갑 착용으로 지문 없음.",
                    "Storage", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_locker_document", "Clue_잠긴사물함내부서류", "잠긴 사물함 내부 서류",
                    "진세웅의 리허설 스케줄표. LockSystem으로 잠긴 사물함 해제 후 획득.",
                    "진세웅의 치밀한 사전 준비 확정.",
                    "Storage", ClueCategory.General, isRequired: false),

                new ClueEntry("normal_paint_footprints", "Clue_바닥페인트자국", "바닥 페인트 자국",
                    "수술대 방향으로 이어진 흰 페인트 발자국.",
                    "핵심 물증 B. 운동화와 연결.",
                    "DressingRoom", ClueCategory.General, isRequired: true),
                new ClueEntry("clue_makeup_diary", "Clue_진세웅일기장", "진세웅 일기장",
                    "'유안나가 모든 걸 망쳐놨어. 하시호 형은 그 때문에 죽은 거야.'",
                    "범행 동기 확정. 미스디렉션 붕괴 시점.",
                    "DressingRoom", ClueCategory.General, isRequired: true),
                new ClueEntry("normal_mirror_message", "Clue_거울메모", "거울 메모",
                    "'봐, 결국 네 차례야 — 세웅.' 립스틱으로 쓰여 있음.",
                    "진세웅이 유안나에게 보낸 협박. 범인 특정.",
                    "DressingRoom", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_makeup_toolbox", "Clue_분장도구함", "분장 도구함",
                    "내부에 흰 페인트 튜브. 운동화, 발자국과 연결.",
                    "물증 연결 고리 완성.",
                    "DressingRoom", ClueCategory.General, isRequired: false),

                new ClueEntry("normal_under_table_space", "Clue_수술대하부공간", "수술대 하부 공간",
                    "성인 1명이 숨을 수 있는 크기. 페인트 자국 방향과 일치.",
                    "진세웅이 여기 숨어 발버둥 연기를 한 것 확정.",
                    "OperatingRoom", ClueCategory.General, isRequired: true),
                new ClueEntry("normal_yoanna_relic", "Clue_유안나의유품", "유안나의 유품",
                    "독약 앰플과 동일 성분 빈 병. 수술대 옆에 놓여 있음.",
                    "범행 완성 증거.",
                    "OperatingRoom", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_nurse_inventory_log", "Clue_간호사실재고일지", "간호사실 재고 일지",
                    "당일 마취약 1병 부족 기록.",
                    "독약 앰플과 연결. 진세웅이 사전에 빼돌린 것.",
                    "OperatingRoom", ClueCategory.General, isRequired: false),

                new ClueEntry("key_clue_coldest_place", "KeyClue_진세웅의쪽지", "진세웅의 쪽지",
                    "'아무도 찾지 못할 곳에 뒀다. 제일 차가운 곳.'",
                    "탈출 열쇠가 차가운 곳에 숨겨져 있음.",
                    "Ward", ClueCategory.KeyClue, isRequired: true),
                new ClueEntry("key_clue_temperature_warning", "KeyClue_온도경고스티커", "온도 경고 스티커",
                    "'내용물 주의 — 4도 이하 보관.' 냉장 약품함에 붙어 있음.",
                    "냉장 약품함을 가리킴.",
                    "Storage", ClueCategory.KeyClue, isRequired: true),
                new ClueEntry("key_clue_fridge_scratches", "KeyClue_긁힌자국", "긁힌 자국",
                    "냉장 약품함 문에 열쇠로 긁힌 흔적. 자물쇠 없이 열림.",
                    "탈출 열쇠가 이 안에 있음 확정.",
                    "Storage", ClueCategory.KeyClue, isRequired: true),
            };
        }

        internal readonly struct ClueEntry
        {
            public readonly string clueID;
            public readonly string fileName;
            public readonly string clueName;
            public readonly string description;
            public readonly string meaning;
            public readonly string zone;
            public readonly ClueCategory category;
            public readonly bool isRequired;

            public ClueEntry(
                string clueID,
                string fileName,
                string clueName,
                string description,
                string meaning,
                string zone,
                ClueCategory category,
                bool isRequired)
            {
                this.clueID = clueID;
                this.fileName = fileName;
                this.clueName = clueName;
                this.description = description;
                this.meaning = meaning;
                this.zone = zone;
                this.category = category;
                this.isRequired = isRequired;
            }
        }
    }
}
