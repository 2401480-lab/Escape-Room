using UnityEditor;
using UnityEngine;

namespace Room02Operating
{
    // Unity 상단 메뉴 Tools > Room02 > Generate All Clue Assets 클릭하면 전체 생성/갱신
    public static class ClueDataGenerator
    {
        private const string OutputPath = "Assets/Room02_Operating/ScriptableObjects";

        [MenuItem("Tools/Room02/Generate All Clue Assets")]
        public static void GenerateAll()
        {
            EnsureOutputFolder();

            int created = 0;
            int updated = 0;

            foreach (ClueEntry entry in GetClues())
            {
                string assetPath = $"{OutputPath}/{entry.clueID}.asset";
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
                asset.displayName = entry.displayName;
                asset.description = entry.description;
                asset.hoverLabel = entry.hoverLabel;
                asset.isCollectable = entry.isCollectable;
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Room02] ClueData 에셋 생성 {created}개, 갱신 {updated}개 완료.");
            EditorUtility.DisplayDialog("완료", $"ClueData 에셋 생성 {created}개, 갱신 {updated}개 완료!", "확인");
        }

        private static void EnsureOutputFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Room02_Operating"))
            {
                AssetDatabase.CreateFolder("Assets", "Room02_Operating");
            }

            if (!AssetDatabase.IsValidFolder(OutputPath))
            {
                AssetDatabase.CreateFolder("Assets/Room02_Operating", "ScriptableObjects");
            }
        }

        private static ClueEntry[] GetClues()
        {
            return new[]
            {
                new ClueEntry(
                    "clue_cast_notice",
                    "배역 안내문",
                    "오늘 공연 참여자 명단. 유안나, 진세웅, 봉태현, 문수미, 하시호의 이름이 적혀 있다.",
                    "배역 안내문 읽기",
                    true),
                new ClueEntry(
                    "clue_memorial_frame",
                    "하시호 추모 액자",
                    "하시호의 사진 아래에 '2년 전 이곳에서 수술 중 사망'이라는 문구가 남아 있다.",
                    "추모 액자 살펴보기",
                    true),
                new ClueEntry(
                    "clue_visitor_log",
                    "방문객 기록부",
                    "당일 입장 시간이 적힌 기록부. 진세웅이 다른 사람보다 유독 일찍 도착했다.",
                    "방문객 기록 확인하기",
                    true),
                new ClueEntry(
                    "clue_security_log",
                    "경비 일지",
                    "경비 일지에는 '수술실 구역 22:00 이후 출입 금지'라고 적혀 있다. 진세웅의 알리바이와 충돌한다.",
                    "경비 일지 읽기",
                    true),
                new ClueEntry(
                    "clue_torn_letter_piece_a",
                    "찢긴 편지 조각 A",
                    "찢긴 편지의 앞부분. '내가 반드시...'라는 문장이 보인다.",
                    "편지 조각 살펴보기",
                    true),
                new ClueEntry(
                    "clue_torn_letter_piece_b",
                    "찢긴 편지 조각 B",
                    "찢긴 편지의 뒷부분. 합치면 '내가 반드시 복수하겠다 — 세웅'이 된다.",
                    "편지 조각 살펴보기",
                    true),
                new ClueEntry(
                    "clue_yoanna_note",
                    "유안나의 메모지",
                    "'봉태현, 당신 그날 어디 있었어?' 유안나의 필체로 적혀 있어 봉태현을 의심하게 만든다.",
                    "메모지 읽기",
                    true),
                new ClueEntry(
                    "clue_nurse_log",
                    "간호사실 일지",
                    "당일 마취약 재고가 한 병 부족하다는 기록. 독약이 준비됐다는 복선이다.",
                    "간호사실 일지 확인하기",
                    true),
                new ClueEntry(
                    "clue_cctv_memo",
                    "대기실 CCTV 메모",
                    "'카메라 고장 중 — 수리 예정'이라는 안내문. 누군가 사각지대를 미리 알고 있었다.",
                    "CCTV 메모 읽기",
                    true),
                new ClueEntry(
                    "clue_phone_memo",
                    "공중전화 메모지",
                    "'세웅아, 하시호는 잊어. 이미 지난 일이야 — 수미'라고 적힌 메모다.",
                    "공중전화 메모 읽기",
                    true),
                new ClueEntry(
                    "clue_hasho_will",
                    "하시호 유서",
                    "'내 죽음은 의료 과실이 아니야. 나는 살해당한 거야.' 사건의 의미를 바꾸는 문장이다.",
                    "하시호 유서 읽기",
                    true),
                new ClueEntry(
                    "clue_medical_certificate",
                    "하시호 진단서",
                    "하시호의 사인은 수술 중 심정지. 담당의에는 봉태현의 이름이 적혀 있다.",
                    "진단서 확인하기",
                    true),
                new ClueEntry(
                    "clue_conversation_memo_a",
                    "대화 메모 A",
                    "'태현 씨, 당신이 하시호를 죽인 거 알아. 당신이 실수한 거잖아.' 유안나가 남긴 메모다.",
                    "대화 메모 읽기",
                    true),
                new ClueEntry(
                    "clue_isolation_bloodstain",
                    "격리 병실 혈흔",
                    "소품 박스 뒤에 말라붙은 작은 혈흔이 있다. 오래된 사건의 흔적처럼 보인다.",
                    "혈흔 조사하기",
                    false),
                new ClueEntry(
                    "clue_sumi_memo",
                    "수미의 메모",
                    "'세웅이가 이상해. 뭔가 계획하고 있는 것 같아.' 문수미의 필체다.",
                    "수미의 메모 읽기",
                    true),
                new ClueEntry(
                    "clue_bong_rebuttal",
                    "봉태현의 반박문",
                    "'나는 최선을 다했다. 하시호의 죽음은 불가항력이었다.' 봉태현이 남긴 변호문이다.",
                    "반박문 읽기",
                    true),
                new ClueEntry(
                    "clue_ward_calendar",
                    "병실 달력",
                    "2년 전 날짜에는 '하시호 수술일', 오늘 날짜에는 '오늘'이라고 표시되어 있다.",
                    "병실 달력 확인하기",
                    true),
                new ClueEntry(
                    "clue_poison_ampoule",
                    "독약 앰플",
                    "라벨 없는 약병. 간호사실 일지의 부족한 마취약 기록과 연결된다.",
                    "독약 앰플 조사하기",
                    true),
                new ClueEntry(
                    "clue_hidden_camera",
                    "소형 카메라",
                    "수술실 방향으로 고정된 카메라. 저장된 영상은 삭제되어 있다.",
                    "소형 카메라 확인하기",
                    true),
                new ClueEntry(
                    "clue_jin_sneakers",
                    "진세웅의 운동화",
                    "운동화 밑창 발가락 쪽에만 흰 페인트가 묻어 있다.",
                    "운동화 조사하기",
                    true),
                new ClueEntry(
                    "clue_gloves",
                    "장갑",
                    "독약 앰플 옆에 있던 장갑. 손가락 끝에는 페인트 흔적이 없다.",
                    "장갑 조사하기",
                    true),
                new ClueEntry(
                    "clue_locked_locker",
                    "잠긴 사물함",
                    "비밀번호가 걸린 사물함. 내부에는 진세웅의 리허설 스케줄표가 들어 있다.",
                    "잠긴 사물함 확인하기",
                    true),
                new ClueEntry(
                    "clue_paint_footprints",
                    "바닥 페인트 자국",
                    "분장실에서 수술대 아래 방향으로 이어지는 흰 페인트 발자국이다.",
                    "페인트 발자국 조사하기",
                    true),
                new ClueEntry(
                    "clue_makeup_diary",
                    "분장 일기장",
                    "진세웅의 필체. '유안나가 모든 걸 망쳐놨어. 하시호 형은 그 때문에 죽은 거야.'",
                    "분장 일기장 읽기",
                    true),
                new ClueEntry(
                    "clue_mirror_message",
                    "거울 메모",
                    "립스틱으로 '봐, 결국 네 차례야 — 세웅'이라고 적혀 있다.",
                    "거울 메모 읽기",
                    true),
                new ClueEntry(
                    "clue_paint_toolbox",
                    "분장 도구함",
                    "도구함 안에 흰 페인트 튜브가 있다. 운동화와 발자국의 페인트 색과 같다.",
                    "분장 도구함 열기",
                    true),
                new ClueEntry(
                    "clue_under_table_space",
                    "수술대 하부 공간",
                    "성인 한 명이 숨을 수 있는 공간. 바닥 긁힌 자국과 페인트 방향이 일치한다.",
                    "수술대 아래 조사하기",
                    true),
                new ClueEntry(
                    "clue_yoanna_relic",
                    "유안나의 유품",
                    "독약 앰플과 동일 성분의 빈 병이 수술대 옆에 놓여 있다.",
                    "유안나의 유품 확인하기",
                    true),
            };
        }

        private struct ClueEntry
        {
            public string clueID;
            public string displayName;
            public string description;
            public string hoverLabel;
            public bool isCollectable;

            public ClueEntry(string id, string display, string desc, string hover, bool collect)
            {
                clueID = id;
                displayName = display;
                description = desc;
                hoverLabel = hover;
                isCollectable = collect;
            }
        }
    }
}
