using UnityEditor;
using UnityEngine;

namespace EscapeRoom.Editor
{
    public static class ClueAssetGenerator
    {
        private const string NormalPath = "Assets/Room02_Operating/Clues/Normal";
        private const string KeyCluePath = "Assets/Room02_Operating/Clues/KeyClue";

        [MenuItem("Tools/Room02/Generate Clues Part1")]
        public static void GenerateCluesPart1()
        {
            EnsureFolders();
            int created = 0;
            int updated = 0;

            foreach (ClueEntry entry in GetPart1Entries())
            {
                CreateOrUpdateAsset(entry, NormalPath, $"{entry.clueID}.asset", ref created, ref updated);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Clues] Part1 ClueData assets generated. Created: {created}, Updated: {updated}");
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Part1 단서 생성 완료", $"Part1 단서 에셋 생성 {created}개, 갱신 {updated}개 완료", "확인");
            }
        }

        [MenuItem("Tools/Room02/Generate Clues Part2")]
        public static void GenerateCluesPart2()
        {
            EnsureFolders();
            int created = 0;
            int updated = 0;

            foreach (ClueEntry entry in GetPart2Entries())
            {
                CreateOrUpdateAsset(entry, NormalPath, $"{entry.clueID}.asset", ref created, ref updated);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Clues] Part2 ClueData assets generated. Created: {created}, Updated: {updated}");
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Part2 단서 생성 완료", $"Part2 단서 에셋 생성 {created}개, 갱신 {updated}개 완료", "확인");
            }
        }

        [MenuItem("Tools/Room02/Generate Clues Part3")]
        public static void GenerateCluesPart3()
        {
            EnsureFolders();
            int created = 0;
            int updated = 0;

            foreach (ClueEntry entry in GetPart3Entries())
            {
                string folder = entry.category == ClueCategory.KeyClue ? KeyCluePath : NormalPath;
                CreateOrUpdateAsset(entry, folder, $"{entry.clueID}.asset", ref created, ref updated);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Clues] Part3 ClueData assets generated. Created: {created}, Updated: {updated}");
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Part3 단서 생성 완료", $"Part3 단서 에셋 생성 {created}개, 갱신 {updated}개 완료", "확인");
            }
        }

        [MenuItem("Tools/Room02/Clues/Generate Story Clue Assets")]
        public static void GenerateStoryClueAssets()
        {
            EnsureFolders();

            int created = 0;
            int updated = 0;

            foreach (ClueEntry entry in GetEntries())
            {
                string folder = entry.category == ClueCategory.KeyClue ? KeyCluePath : NormalPath;
                CreateOrUpdateAsset(entry, folder, $"{entry.fileName}.asset", ref created, ref updated);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Clues] Story ClueData assets generated. Created: {created}, Updated: {updated}");
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("단서 생성 완료", $"단서 에셋 생성 {created}개, 갱신 {updated}개 완료", "확인");
            }
        }

        private static void CreateOrUpdateAsset(ClueEntry entry, string folder, string fileName, ref int created, ref int updated)
        {
            string assetPath = $"{folder}/{fileName}";
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
            return CurrentStoryEntries;
        }

        private static readonly ClueEntry[] CurrentStoryEntries = new[]
        {
            new ClueEntry("normal_cast_notice", "Clue_\uBC30\uC5ED\uC548\uB0B4\uBB38", "배역 안내문",
                "수술실 담당: 진세웅, 봉태현. 환자 역: 유안나.",
                "수첩: 봉태현/진세웅 — 수술실 접근 가능",
                "Hallway", ClueCategory.General, isRequired: true),
            new ClueEntry("normal_memorial_frame", "Clue_\uCD94\uBAA8\uC561\uC790", "하시호 추모 액자",
                "1개월 전 사망. 향년 22세. 동아리 부원 일동.",
                "수첩: 공통 — 한 달 전 부원이 사망했다",
                "Hallway", ClueCategory.General, isRequired: true),
            new ClueEntry("normal_conversation_memo", "Clue_\uB300\uD654\uBA54\uBAA8", "쓰레기통 메모",
                "안나, 네가 한 짓을 잊지 않겠다.",
                "수첩: 공통 — 유안나에게 원한을 품은 인물이 있다",
                "Hallway", ClueCategory.General, isRequired: true),

            new ClueEntry("normal_medical_certificate", "Clue_\uC9C4\uB2E8\uC11C", "하시호 진단서",
                "말기 진단. 담당의: 봉태현.",
                "수첩: 봉태현 — 하시호 담당의였다. 의료 과실 가능성?",
                "Ward", ClueCategory.General, isRequired: true),
            new ClueEntry("normal_ward_calendar", "Clue_\uBCD1\uC2E4\uB2EC\uB825", "병실 낙서",
                "왜 살릴 수 있었는데 살리지 않았어.",
                "수첩: 봉태현 — 누군가 봉태현을 원망하고 있다",
                "Ward", ClueCategory.General, isRequired: true),
            new ClueEntry("clue_hasho_will", "Clue_\uD558\uC2DC\uD638\uC720\uC11C", "하시호 유서 사본",
                "살 이유를 잃었다. 안나가 그걸 빼앗아 갔다.",
                "수첩: 공통 — 유안나가 하시호의 죽음과 연관되어 있다",
                "Ward", ClueCategory.General, isRequired: true),
            new ClueEntry("key_clue_coldest_place", "KeyClue_\uC9C4\uC138\uC6C5\uC758\uCABD\uC9C0", "침대 밑 쪽지",
                "제일 차가운 곳을 찾아라.",
                "수첩: 공통 — 열쇠 힌트 1. 차가운 장소?",
                "Ward", ClueCategory.KeyClue, isRequired: true),

            new ClueEntry("key_clue_temperature_warning", "KeyClue_\uC628\uB3C4\uACBD\uACE0\uC2A4\uD2F0\uCEE4", "냉장 약품함",
                "온도 경고 스티커. 문 모서리에 긁힌 자국이 있다.",
                "수첩: 공통 — 열쇠 힌트 2. 누군가 최근에 열었다",
                "Storage", ClueCategory.KeyClue, isRequired: true),
            new ClueEntry("normal_bong_rebuttal", "Clue_\uBD09\uD0DC\uD604\uC758\uBC18\uBC15\uBB38", "봉태현 메모",
                "세웅아 내가 먼저 수술실 들어갈게. / 답: 괜찮아, 내가 할게.",
                "수첩: 봉태현 — 수술실 입장을 진세웅에게 양보했다. 봉태현은 안에 없었다?",
                "Storage", ClueCategory.General, isRequired: true),
            new ClueEntry("key_clue_fridge_scratches", "KeyClue_\uAE01\uD78C\uC790\uAD6D", "냉장 약품함 내부",
                "열쇠가 들어있다. 차갑다.",
                "수첩: 공통 — 탈출 열쇠 획득",
                "Storage", ClueCategory.KeyClue, isRequired: true),

            new ClueEntry("normal_makeup_toolbox", "Clue_\uBD84\uC7A5\uB3C4\uAD6C\uD568", "진세웅 분장대",
                "붉은 페인트 통이 열려있다. 발가락 모양 자국이 찍혀있다.",
                "수첩: 진세웅 — 현장 페인트 자국과 동일한 페인트",
                "DressingRoom", ClueCategory.General, isRequired: true),
            new ClueEntry("normal_sumi_memo", "Clue_\uBB38\uC218\uBBF8\uC758\uBA54\uBAA8", "문수미 일기장",
                "안나가 하시호한테 그런 짓을 했다면, 진세웅 오빠가 절대 가만있지 않을 것 같다.",
                "수첩: 진세웅 — 유안나에 대한 원한의 주체로 지목됨",
                "DressingRoom", ClueCategory.General, isRequired: true),
            new ClueEntry("clue_makeup_diary", "Clue_\uC9C4\uC138\uC6C5\uC77C\uAE30\uC7A5", "진세웅 일기장",
                "2년을 기다렸다. 오늘이 마지막이다.",
                "수첩: 진세웅 — 계획적 범행 가능성. 사전 준비했다",
                "DressingRoom", ClueCategory.General, isRequired: true),

            new ClueEntry("normal_under_table_space", "Clue_\uC218\uC220\uB300\uD558\uBD80\uACF5\uAC04", "수술대 아래",
                "사람 한 명이 숨을 수 있는 공간. 바닥에 긁힌 자국과 붉은 페인트 자국이 남아있다.",
                "수첩: 진세웅 — 수술대 아래 숨어 알리바이를 만들었다. 결정적 증거",
                "OperatingRoom", ClueCategory.General, isRequired: true),
            new ClueEntry("normal_mirror_message", "Clue_\uAC70\uC6B8\uBA54\uBAA8", "벽 메모",
                "하시호를 위해서. 미안해, 안나. 쓰레기통 메모와 같은 필체다.",
                "수첩: 진세웅 — 쓰레기통 메모 필체와 일치. 범인은 진세웅",
                "OperatingRoom", ClueCategory.General, isRequired: true),
        };

        private static ClueEntry[] GetLegacyEntries()
        {
            return new[]
            {
                new ClueEntry("normal_cast_notice", "Clue_\uBC30\uC5ED\uC548\uB0B4\uBB38", "\uBC30\uC5ED \uC548\uB0B4\uBB38",
                    "\uC816\uC740 \uC885\uC774\uC5D0 \uBC30\uC6B0 \uB2E4\uC12F \uBA85\uC758 \uC774\uB984\uC774 \uCC28\uAC11\uAC8C \uBC88\uC838 \uC788\uB2E4. \uB9C8\uC9C0\uB9C9 \uC904\uC758 \uC9C4\uC138\uC6C5 \uC11C\uBA85\uB9CC \uC720\uB09C\uD788 \uB610\uB837\uD558\uB2E4.",
                    "\uC774 \uC548\uB0B4\uBB38\uC740 \uCD08\uB300\uC7A5\uC774 \uC544\uB2C8\uB2E4. \uC9C4\uC138\uC6C5\uC774 \uC774 \uBC24\uC744 \uCC28\uAC11\uAC8C \uC124\uACC4\uD588\uB2E4.",
                    "Lobby", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_memorial_frame", "Clue_\uCD94\uBAA8\uC561\uC790", "\uCD94\uBAA8 \uC561\uC790",
                    "\uC561\uC790 \uC720\uB9AC \uC548\uCABD\uC5D0 \uC228\uCC98\uB7FC \uAE40\uC774 \uC11C\uB824 \uC788\uB2E4. \u6545 \uD558\uC2DC\uD638, 2\uB144 \uC804 \uC774 \uBCD1\uC6D0 \uC218\uC220\uC2E4\uC5D0\uC11C \uC228\uC9C0\uB2E4.",
                    "2\uB144 \uC804 \uC218\uC220\uC2E4\uC758 \uC8FD\uC74C\uC774 \uC544\uC9C1 \uBC29\uC744 \uB5A0\uB098\uC9C0 \uC54A\uC558\uB2E4.",
                    "Lobby", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_security_log", "Clue_\uACBD\uBE44\uC77C\uC9C0", "\uACBD\uBE44 \uC77C\uC9C0",
                    "22:00 \uC774\uD6C4 \uCD9C\uC785 \uAE08\uC9C0\uB77C\uB294 \uBB38\uC7A5 \uC544\uB798, \uD39C \uB05D\uC774 \uB5A8\uB9B0 \uB4EF \uC789\uD06C\uAC00 \uB04A\uACA8 \uC788\uB2E4.",
                    "\uC9C4\uC138\uC6C5\uC758 \uC54C\uB9AC\uBC14\uC774\uAC00 \uC11C\uB298\uD558\uAC8C \uD754\uB4E4\uB9AC\uAE30 \uC2DC\uC791\uD55C\uB2E4.",
                    "Lobby", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_production_plan", "Clue_\uACF5\uC5F0\uAE30\uD68D\uC11C", "\uACF5\uC5F0 \uAE30\uD68D\uC11C",
                    "\uAE30\uD68D\uC790 \uC9C4\uC138\uC6C5\uC758 \uC11C\uBA85 \uC606 \uC885\uC774\uAC00 \uC190\uB304 \uC790\uB9AC\uB9CC \uCC28\uAC11\uAC8C \uB20C\uB824 \uC788\uB2E4.",
                    "\uACF5\uC5F0\uC740 \uBB34\uB300\uAC00 \uC544\uB2C8\uB77C \uC0AC\uB78C\uC744 \uAC00\uB450\uAE30 \uC704\uD55C \uCC28\uAC00\uC6B4 \uB36B\uC774\uC5C8\uB2E4.",
                    "Lobby", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_torn_letter_a", "Clue_\uCC22\uAE34\uD3B8\uC9C0\uC870\uAC01A", "\uCC22\uAE34 \uD3B8\uC9C0 \uC870\uAC01 A",
                    "\uCC22\uAE34 \uAC00\uC7A5\uC790\uB9AC\uC5D0\uC11C \uC624\uB798\uB41C \uD53C \uB0C4\uC0C8 \uAC19\uC740 \uC789\uD06C\uAC00 \uC62C\uB77C\uC628\uB2E4. \u201C\uB0B4\uAC00 \uBC18\uB4DC\uC2DC\u201D\uAE4C\uC9C0\uB9CC \uB0A8\uC558\uB2E4.",
                    "\uB2E4\uB978 \uC870\uAC01\uC774 \uC5C6\uC73C\uBA74 \uBB38\uC7A5\uC774 \uC228\uC744 \uBA48\uCD98 \uCC44 \uB0A8\uB294\uB2E4.",
                    "Hallway", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_torn_letter_b", "Clue_\uCC22\uAE34\uD3B8\uC9C0\uC870\uAC01B", "\uCC22\uAE34 \uD3B8\uC9C0 \uC870\uAC01 B",
                    "\uB098\uBA38\uC9C0 \uBC18\uCABD\uC5D0\uB294 \uB5A8\uB9AC\uB294 \uD544\uCCB4\uB85C \u201C\uBCF5\uC218\uD558\uACA0\uB2E4 \u2014 \uC138\uC6C5\u201D\uC774\uB77C\uACE0 \uC801\uD600 \uC788\uB2E4.",
                    "\uB450 \uC870\uAC01\uC744 \uBD99\uC774\uBA74 \uC9C4\uC138\uC6C5\uC758 \uBD84\uB178\uAC00 \uBAA9\uB35C\uBBF8\uAE4C\uC9C0 \uB530\uB77C\uBD99\uB294\uB2E4.",
                    "Hallway", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_yoanna_memo", "Clue_\uBA54\uBAA8\uC9C0", "\uBA54\uBAA8\uC9C0",
                    "\uC885\uC774\uAC00 \uCD95\uCD95\uD558\uB2E4. \u201C\uBD09\uD0DC\uD604, \uB2F9\uC2E0 \uADF8\uB0A0 \uC5B4\uB514 \uC788\uC5C8\uC5B4?\u201D \uC720\uC548\uB098\uC758 \uAE00\uC528\uAC00 \uB05D\uC5D0\uC11C \uBB34\uB108\uC9C4\uB2E4.",
                    "\uBD09\uD0DC\uD604\uC744 \uD5A5\uD55C \uC758\uC2EC\uC774 \uC5B4\uB460 \uC18D\uC73C\uB85C \uD50C\uB808\uC774\uC5B4\uB97C \uB04C\uACE0 \uAC04\uB2E4.",
                    "Hallway", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_cctv_notice", "Clue_\uB300\uAE30\uC2E4CCTV\uC548\uB0B4\uBB38", "\uB300\uAE30\uC2E4 CCTV \uC548\uB0B4\uBB38",
                    "\u201C\uCE74\uBA54\uB77C \uACE0\uC7A5 \uC911\u201D \uAE00\uC528\uAC00 \uC190\uD1B1\uC73C\uB85C \uAE01\uD78C \uAC83\uCC98\uB7FC \uAC70\uCE60\uB2E4.",
                    "\uB204\uAD70\uAC00 \uBCF4\uB294 \uB208\uC744 \uBA3C\uC800 \uC5B4\uB460 \uC18D\uC5D0 \uBB3B\uC5B4 \uB450\uC5C8\uB2E4.",
                    "Hallway", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_sumi_memo", "Clue_\uBB38\uC218\uBBF8\uC758\uBA54\uBAA8", "\uBB38\uC218\uBBF8\uC758 \uBA54\uBAA8",
                    "\uAE00\uC528 \uB05D\uC774 \uB5A8\uB9B0\uB2E4. \u201C\uC138\uC6C5\uC774\uAC00 \uC774\uC0C1\uD574. \uBB54\uAC00 \uACC4\uD68D\uD558\uACE0 \uC788\uB294 \uAC83 \uAC19\uC544.\u201D",
                    "\uAC00\uAE4C\uC6B4 \uC0AC\uB78C\uB3C4 \uADF8\uC758 \uC18D\uC0AD\uC784\uC744 \uB4E4\uC740 \uAC83\uCC98\uB7FC \uBD88\uC548\uC744 \uB0A8\uACBC\uB2E4.",
                    "Hallway", ClueCategory.General, isRequired: false),
                new ClueEntry("clue_hasho_will", "Clue_\uD558\uC2DC\uD638\uC720\uC11C", "\uD558\uC2DC\uD638 \uC720\uC11C",
                    "\uBB38\uC7A5 \uC0AC\uC774\uB9C8\uB2E4 \uC228\uC774 \uB04A\uAE34 \uB4EF\uD558\uB2E4. \u201C\uB0B4 \uC8FD\uC74C\uC740 \uC758\uB8CC \uACFC\uC2E4\uC774 \uC544\uB2C8\uC57C. \uB098\uB294 \uC0B4\uD574\uB2F9\uD55C \uAC70\uC57C.\u201D",
                    "\uC774 \uBC29\uC740 \uC0AC\uACE0\uAC00 \uC544\uB2C8\uB77C \uC0B4\uC778\uC758 \uB0C4\uC0C8\uB97C \uB0B8\uB2E4.",
                    "Ward", ClueCategory.General, isRequired: true),
                new ClueEntry("normal_medical_certificate", "Clue_\uC9C4\uB2E8\uC11C", "\uC9C4\uB2E8\uC11C",
                    "\uB2F4\uB2F9\uC758 \uBD09\uD0DC\uD604\uC758 \uC774\uB984 \uC704\uB85C \uC789\uD06C\uAC00 \uCC3D\uBC31\uD558\uAC8C \uBC88\uC838 \uC788\uB2E4. \uC0AC\uC778\uC740 \uC218\uC220 \uC911 \uC2EC\uC815\uC9C0.",
                    "\uBD09\uD0DC\uD604 \uCABD\uC73C\uB85C \uD53C \uBB3B\uC740 \uC190\uAC00\uB77D\uC774 \uD5A5\uD558\uC9C0\uB9CC, \uB108\uBB34 \uAE54\uB054\uD574\uC11C \uB354 \uC218\uC0C1\uD558\uB2E4.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_conversation_memo", "Clue_\uB300\uD654\uBA54\uBAA8", "\uB300\uD654 \uBA54\uBAA8",
                    "\uC720\uC548\uB098\uC758 \uAE00\uC528\uAC00 \uB0A0\uCE74\uB86D\uAC8C \uCC22\uACA8 \uC788\uB2E4. \u201C\uD0DC\uD604 \uC528, \uB2F9\uC2E0\uC774 \uD558\uC2DC\uD638\uB97C \uC8FD\uC778 \uAC70 \uC54C\uC544.\u201D",
                    "\uC758\uC2EC\uC774 \uBD09\uD0DC\uD604\uC5D0\uAC8C \uB2EC\uB77C\uBD99\uC9C0\uB9CC, \uBB38\uC7A5 \uB05D\uC758 \uCC28\uAC00\uC6C0\uC774 \uB2E4\uB978 \uC0AC\uB78C\uC744 \uAC00\uB9AC\uD0A8\uB2E4.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_bong_rebuttal", "Clue_\uBD09\uD0DC\uD604\uC758\uBC18\uBC15\uBB38", "\uBD09\uD0DC\uD604\uC758 \uBC18\uBC15\uBB38",
                    "\uC885\uC774 \uBAA8\uC11C\uB9AC\uAC00 \uB5A8\uB9AC\uACE0 \uAD6C\uACA8\uC838 \uC788\uB2E4. \u201C\uB098\uB294 \uCD5C\uC120\uC744 \uB2E4\uD588\uB2E4. \uC8FD\uC74C\uC740 \uBD88\uAC00\uD56D\uB825\uC774\uC5C8\uB2E4.\u201D",
                    "\uADF8\uC758 \uBCC0\uBA85 \uC0AC\uC774\uB85C \uB2E4\uB978 \uBC1C\uC18C\uB9AC\uAC00 \uC11C\uB298\uD558\uAC8C \uC2A4\uCE5C\uB2E4.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_ward_calendar", "Clue_\uBCD1\uC2E4\uB2EC\uB825", "\uBCD1\uC2E4 \uB2EC\uB825",
                    "\uC624\uB298 \uB0A0\uC9DC\uC758 \uBCC4\uD45C\uAC00 \uD53C\uCC98\uB7FC \uC9C4\uD558\uAC8C \uB20C\uB824 \uC788\uB2E4. 2\uB144 \uC804 \uC218\uC220\uC77C\uC5D0\uB3C4 \uAC19\uC740 \uD45C\uC2DC\uAC00 \uC788\uB2E4.",
                    "2\uB144 \uB3D9\uC548 \uBA48\uCD98 \uC2DC\uAC04\uC774 \uC774 \uBC24\uC5D0 \uB2E4\uC2DC \uC228\uC744 \uC270\uB2E4.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_oh_threat_memo", "Clue_\uC624\uC138\uC9C4\uD611\uBC15\uBA54\uBAA8", "\uC624\uC138\uC9C4 \uD611\uBC15 \uBA54\uBAA8",
                    "\uD611\uBC15 \uBB38\uC7A5 \uB4A4\uCABD\uC5D0 \uC190\uD1B1\uC73C\uB85C \uAE01\uC740 \uC790\uAD6D\uC774 \uB0A8\uC544 \uC788\uB2E4. \u201C\uAE30\uB85D\uC744 \uC9C0\uC6B0\uC9C0 \uC54A\uC73C\uBA74 \uB098\uB3C4 \uC8FD\uC778\uB2E4\uACE0 \uD588\uB2E4.\u201D",
                    "\uC624\uC138\uC9C4\uC740 \uACF5\uBC94\uBCF4\uB2E4 \uBA3C\uC800 \uAC81\uC5D0 \uC9C8\uB9B0 \uD53C\uD574\uC790\uC758 \uB0C4\uC0C8\uAC00 \uB09C\uB2E4.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_deleted_entry_trace", "Clue_\uCD9C\uC785\uAE30\uB85D\uC0AD\uC81C\uD754\uC801", "\uCD9C\uC785 \uAE30\uB85D \uC0AD\uC81C \uD754\uC801",
                    "\uC0AD\uC81C\uB41C \uCE78\uB9CC \uCC3D\uBC31\uD558\uAC8C \uBE44\uC5B4 \uC788\uB2E4. \uC624\uC138\uC9C4\uC758 \uC774\uB984\uC774 \uC9C0\uC6CC\uC9C4 \uC790\uB9AC\uC5D0\uC11C \uC789\uD06C\uAC00 \uBC88\uC9C4\uB2E4.",
                    "\uAE30\uB85D\uC744 \uC0BC\uD0A8 \uC5B4\uB460 \uB4A4\uC5D0 \uC9C4\uC138\uC6C5\uC758 \uC190\uC774 \uB0A8\uC544 \uC788\uB2E4.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_poison_ampoule", "Clue_\uB3C5\uC57D\uC570\uD50C", "\uB3C5\uC57D \uC570\uD50C",
                    "\uCC28\uAC11\uACE0 \uB77C\uBCA8 \uC5C6\uB294 \uC57D\uBCD1\uC774\uB2E4. \uC190\uC5D0 \uB2FF\uC790 \uAE08\uC18D \uB0C4\uC0C8\uAC00 \uC62C\uB77C\uC628\uB2E4.",
                    "\uC9C4\uC138\uC6C5\uC774 \uC0AC\uC804\uC5D0 \uBE7C\uB3CC\uB9B0 \uB3C4\uAD6C\uAC00 \uC774\uC81C \uC190\uBC14\uB2E5 \uC704\uC5D0\uC11C \uC2DD\uC5B4 \uAC04\uB2E4.",
                    "Storage", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_hidden_camera", "Clue_\uC18C\uD615\uCE74\uBA54\uB77C", "\uC18C\uD615 \uCE74\uBA54\uB77C",
                    "\uC791\uC740 \uB80C\uC988\uAC00 \uC5B4\uB460 \uC18D\uC5D0\uC11C \uC544\uC9C1\uB3C4 \uBCF4\uACE0 \uC788\uB294 \uAC83\uCC98\uB7FC \uBC18\uC9DD\uC778\uB2E4. \uC800\uC7A5 \uC601\uC0C1\uC740 \uC9C0\uC6CC\uC838 \uC788\uB2E4.",
                    "\uBCF4\uAE30 \uC704\uD574 \uB454 \uB208\uC774 \uC544\uB2C8\uB77C, \uBC94\uD589 \uB4A4 \uC228\uAE30 \uC704\uD574 \uB454 \uB208\uC774\uB2E4.",
                    "Storage", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_jin_sneakers", "Clue_\uC6B4\uB3D9\uD654", "\uC6B4\uB3D9\uD654",
                    "\uBC11\uCC3D\uC758 \uD770 \uD398\uC778\uD2B8\uAC00 \uB9C8\uB978 \uD53C\uCC98\uB7FC \uBC1C\uAC00\uB77D \uCABD\uC5D0\uB9CC \uC5C9\uACA8 \uC788\uB2E4.",
                    "\uB204\uAD70\uAC00 \uBC1C\uB05D\uC73C\uB85C \uAE30\uC5B4 \uC218\uC220\uB300 \uC544\uB798 \uC228\uC5B4 \uC228\uC744 \uC8FD\uC600\uB2E4.",
                    "Storage", ClueCategory.General, isRequired: true),
                new ClueEntry("normal_gloves", "Clue_\uC7A5\uAC11", "\uC7A5\uAC11",
                    "\uC190\uAC00\uB77D \uB05D\uC740 \uAE68\uB057\uD55C\uB370 \uC548\uCABD\uC774 \uCD95\uCD95\uD558\uB2E4. \uC624\uB798 \uC950\uACE0 \uC788\uB358 \uBD88\uC548\uC774 \uB0A8\uC544 \uC788\uB2E4.",
                    "\uC9C0\uBB38 \uB300\uC2E0 \uCC28\uAC00\uC6B4 \uACC4\uD68D\uC774 \uB0A8\uC558\uB2E4.",
                    "Storage", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_locker_document", "Clue_\uC7A0\uAE34\uC0AC\uBB3C\uD568\uB0B4\uBD80\uC11C\uB958", "\uC7A0\uAE34 \uC0AC\uBB3C\uD568 \uB0B4\uBD80 \uC11C\uB958",
                    "\uC11C\uB958 \uC0AC\uC774\uC5D0\uC11C \uBA3C\uC9C0\uC640 \uC57D\uD488 \uB0C4\uC0C8\uAC00 \uC11E\uC5EC \uC62C\uB77C\uC628\uB2E4. \uC9C4\uC138\uC6C5\uC758 \uB9AC\uD5C8\uC124 \uC2A4\uCF00\uC904\uD45C\uB2E4.",
                    "\uB9AC\uD5C8\uC124\uC740 \uC5F0\uC2B5\uC774 \uC544\uB2C8\uB77C \uC0B4\uC778\uC744 \uBC18\uBCF5\uD574\uC11C \uB9DE\uCD98 \uD754\uC801\uC774\uC5C8\uB2E4.",
                    "Storage", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_paint_footprints", "Clue_\uBC14\uB2E5\uD398\uC778\uD2B8\uC790\uAD6D", "\uBC14\uB2E5 \uD398\uC778\uD2B8 \uC790\uAD6D",
                    "\uD770 \uBC1C\uC790\uAD6D\uC774 \uC218\uC220\uB300 \uCABD\uC73C\uB85C \uB5A8\uB9AC\uB4EF \uC774\uC5B4\uC9C4\uB2E4. \uD55C \uAC78\uC74C\uB9C8\uB2E4 \uBA48\uCE6B\uD55C \uD754\uC801\uC774 \uC788\uB2E4.",
                    "\uC6B4\uB3D9\uD654\uC640 \uBC1C\uC790\uAD6D\uC774 \uAC19\uC740 \uBC29\uD5A5\uC73C\uB85C \uC228\uC744 \uC8FD\uC778\uB2E4.",
                    "DressingRoom", ClueCategory.General, isRequired: true),
                new ClueEntry("clue_makeup_diary", "Clue_\uC9C4\uC138\uC6C5\uC77C\uAE30\uC7A5", "\uC9C4\uC138\uC6C5 \uC77C\uAE30\uC7A5",
                    "\uC77C\uAE30\uC7A5 \uC548\uCABD\uC5D0 \uB20C\uB9B0 \uAE00\uC528\uAC00 \uB0A0\uCE74\uB86D\uB2E4. \u201C\uC720\uC548\uB098\uAC00 \uBAA8\uB4E0 \uAC78 \uB9DD\uCCD0\uB1A8\uC5B4.\u201D",
                    "\uBCF5\uC218\uC758 \uBAA9\uC18C\uB9AC\uAC00 \uB354 \uC774\uC0C1 \uC18D\uC0AD\uC784\uC73C\uB85C \uC228\uC9C0 \uC54A\uB294\uB2E4.",
                    "DressingRoom", ClueCategory.General, isRequired: true),
                new ClueEntry("normal_mirror_message", "Clue_\uAC70\uC6B8\uBA54\uBAA8", "\uAC70\uC6B8 \uBA54\uBAA8",
                    "\uB9BD\uC2A4\uD2F1 \uAE00\uC790\uAC00 \uD53C\uCC98\uB7FC \uBC88\uC838 \uC788\uB2E4. \u201C\uBD10, \uACB0\uAD6D \uB124 \uCC28\uB840\uC57C \u2014 \uC138\uC6C5.\u201D",
                    "\uC720\uC548\uB098\uC758 \uC774\uB984 \uB4A4\uB85C \uC138\uC6C5\uC758 \uC228\uC18C\uB9AC\uAC00 \uBD99\uC5B4 \uC788\uB2E4.",
                    "DressingRoom", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_makeup_toolbox", "Clue_\uBD84\uC7A5\uB3C4\uAD6C\uD568", "\uBD84\uC7A5 \uB3C4\uAD6C\uD568",
                    "\uB69C\uAED1 \uC5F4\uB9B0 \uD29C\uBE0C\uC5D0\uC11C \uCD95\uCD95\uD55C \uD398\uC778\uD2B8 \uB0C4\uC0C8\uAC00 \uB09C\uB2E4. \uD770\uC0C9\uB9CC \uC720\uB09C\uD788 \uB9CE\uC774 \uBE44\uC5C8\uB2E4.",
                    "\uC6B4\uB3D9\uD654\uC640 \uBC1C\uC790\uAD6D\uC744 \uBB36\uB294 \uCC28\uAC00\uC6B4 \uC5F0\uACB0\uACE0\uB9AC\uB2E4.",
                    "DressingRoom", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_under_table_space", "Clue_\uC218\uC220\uB300\uD558\uBD80\uACF5\uAC04", "\uC218\uC220\uB300 \uD558\uBD80 \uACF5\uAC04",
                    "\uC218\uC220\uB300 \uC544\uB798 \uACF5\uAE30\uAC00 \uCC28\uAC11\uAC8C \uACE0\uC5EC \uC788\uB2E4. \uC131\uC778 \uD55C \uBA85\uC774 \uC6C5\uD06C\uB9B4 \uB9CC\uD07C\uC758 \uD2C8\uC774\uB2E4.",
                    "\uB204\uAD70\uAC00 \uC5EC\uAE30\uC11C \uC228\uC744 \uC8FD\uC774\uACE0 \uBC1C\uBC84\uB465\uC744 \uC5F0\uAE30\uD588\uB2E4.",
                    "OperatingRoom", ClueCategory.General, isRequired: true),
                new ClueEntry("normal_yoanna_relic", "Clue_\uC720\uC548\uB098\uC758\uC720\uD488", "\uC720\uC548\uB098\uC758 \uC720\uD488",
                    "\uBE48 \uBCD1 \uC785\uAD6C\uC5D0 \uCC28\uAC00\uC6B4 \uC57D \uB0C4\uC0C8\uAC00 \uB0A8\uC544 \uC788\uB2E4. \uB3C5\uC57D \uC570\uD50C\uACFC \uAC19\uC740 \uD754\uC801\uC774\uB2E4.",
                    "\uB3C5\uC774 \uC2E4\uC81C\uB85C \uC720\uC548\uB098\uC5D0\uAC8C \uB2FF\uC558\uB2E4\uB294 \uB9C8\uC9C0\uB9C9 \uD754\uC801\uC774\uB2E4.",
                    "OperatingRoom", ClueCategory.General, isRequired: false),
                new ClueEntry("normal_nurse_inventory_log", "Clue_\uAC04\uD638\uC0AC\uC2E4\uC7AC\uACE0\uC77C\uC9C0", "\uAC04\uD638\uC0AC\uC2E4 \uC7AC\uACE0 \uC77C\uC9C0",
                    "\uBD80\uC871 \uC22B\uC790 \uD558\uB098\uAC00 \uCC3D\uBC31\uD558\uAC8C \uB5A0 \uC788\uB2E4. \uB2F9\uC77C \uB9C8\uCDE8\uC57D 1\uBCD1\uC774 \uC0AC\uB77C\uC84C\uB2E4.",
                    "\uC0AC\uB77C\uC9C4 \uBE48\uCE78\uC5D0\uC11C \uD53C \uB0C4\uC0C8\uAC00 \uC62C\uB77C\uC628\uB2E4. \uC570\uD50C\uC758 \uCD9C\uCC98\uAC00 \uC5F4\uB9B0\uB2E4.",
                    "OperatingRoom", ClueCategory.General, isRequired: false),
                new ClueEntry("key_clue_coldest_place", "KeyClue_\uC9C4\uC138\uC6C5\uC758\uCABD\uC9C0", "\uC9C4\uC138\uC6C5\uC758 \uCABD\uC9C0",
                    "\uC885\uC774\uAC00 \uC774\uC0C1\uD558\uAC8C \uCC28\uAC11\uB2E4. \u201C\uC544\uBB34\uB3C4 \uCC3E\uC9C0 \uBABB\uD560 \uACF3\uC5D0 \uB480\uB2E4. \uC81C\uC77C \uCC28\uAC00\uC6B4 \uACF3.\u201D",
                    "\uC5F4\uC1E0\uB294 \uAC00\uC7A5 \uCC28\uAC00\uC6B4 \uACF3\uC5D0\uC11C \uAE30\uB2E4\uB9B0\uB2E4.",
                    "Ward", ClueCategory.KeyClue, isRequired: true),
                new ClueEntry("key_clue_temperature_warning", "KeyClue_\uC628\uB3C4\uACBD\uACE0\uC2A4\uD2F0\uCEE4", "\uC628\uB3C4 \uACBD\uACE0 \uC2A4\uD2F0\uCEE4",
                    "4\uB3C4 \uC774\uD558 \uBCF4\uAD00 \uACBD\uACE0\uBB38\uC774 \uC11C\uB298\uD558\uAC8C \uBD99\uC5B4 \uC788\uB2E4. \uAC00\uC7A5\uC790\uB9AC\uC5D0\uB294 \uC131\uC5D0 \uAC19\uC740 \uC5BC\uB8E9\uC774 \uB9D0\uB77C \uC788\uB2E4.",
                    "\uCC28\uAC00\uC6B4 \uC57D\uD488\uD568\uC774 \uB2E4\uC74C \uC228\uC740 \uC7A5\uC18C\uB2E4.",
                    "Storage", ClueCategory.KeyClue, isRequired: true),
                new ClueEntry("key_clue_fridge_scratches", "KeyClue_\uAE01\uD78C\uC790\uAD6D", "\uAE01\uD78C \uC790\uAD6D",
                    "\uB0C9\uC7A5 \uC57D\uD488\uD568 \uBB38 \uD45C\uBA74\uC5D0 \uAE01\uD78C \uC120\uC774 \uBE7D\uBE7D\uD558\uB2E4. \uC548\uCABD\uC5D0\uC11C \uB204\uAC00 \uB098\uAC00\uB824 \uD55C \uAC83\uCC98\uB7FC \uBCF4\uC778\uB2E4.",
                    "\uC5F4\uC1E0\uAC00 \uC548\uCABD\uC5D0\uC11C \uCC28\uAC11\uAC8C \uC7A0\uB4E4\uC5B4 \uC788\uB2E4.",
                    "Storage", ClueCategory.KeyClue, isRequired: true),
            };
        }

        internal static ClueEntry[] GetPart1Entries()
        {
            return new[]
            {
                new ClueEntry("cast_notice", "cast_notice", "배역 안내문",
                    "오늘 공연 참여자 명단. 기획자 란에 진세웅 서명.",
                    "공연 자체를 진세웅이 기획했다. 피해자를 이 장소에 불러모은 것이 계획의 일부.",
                    "Lobby", ClueCategory.General, isRequired: false),
                new ClueEntry("memorial_frame", "memorial_frame", "추모 액자",
                    "故 하시호 — 2년 전 이 병원 수술실에서 숨지다.",
                    "사건의 발단. 하시호의 죽음이 이번 사건의 근본 원인이다.",
                    "Lobby", ClueCategory.General, isRequired: false),
                new ClueEntry("security_log", "security_log", "경비 일지",
                    "수술실 구역 22:00 이후 출입 금지. 오늘 날짜 기재.",
                    "진세웅이 22:00 이후 수술실 구역에 있었다는 증거와 충돌. 알리바이 붕괴 복선.",
                    "Lobby", ClueCategory.General, isRequired: false),
                new ClueEntry("event_plan", "event_plan", "공연 기획서",
                    "오늘 공연 시나리오 개요. 기획 총책임자: 진세웅 서명.",
                    "진세웅이 이 자리 전체를 설계했음을 확정한다.",
                    "Lobby", ClueCategory.General, isRequired: false),
                new ClueEntry("torn_letter_a", "torn_letter_a", "찢긴 편지 조각 A",
                    "반쪽짜리 편지. 내가 반드시 까지만 읽힌다.",
                    "조각 B와 합쳐야 전문 완성.",
                    "Hallway", ClueCategory.General, isRequired: false),
                new ClueEntry("torn_letter_b", "torn_letter_b", "찢긴 편지 조각 B",
                    "편지 나머지 반쪽. 복수하겠다 — 세웅.",
                    "A+B 합치면 진세웅의 복수 동기가 본인 필체로 확정된다.",
                    "Hallway", ClueCategory.General, isRequired: true),
                new ClueEntry("yoanna_note", "yoanna_note", "메모지",
                    "유안나 필체. 봉태현, 당신 그날 어디 있었어?",
                    "봉태현 의심 유도. 미스디렉션 핵심 단서.",
                    "Hallway", ClueCategory.General, isRequired: false),
                new ClueEntry("cctv_memo", "cctv_memo", "대기실 CCTV 안내문",
                    "카메라 고장 중 — 수리 예정.",
                    "진세웅이 의도적으로 CCTV를 고장낸 것. 사전 준비 증거.",
                    "Hallway", ClueCategory.General, isRequired: false),
                new ClueEntry("sumi_memo", "sumi_memo", "문수미의 메모",
                    "문수미 필체. 세웅이가 이상해. 뭔가 계획하고 있는 것 같아.",
                    "장기 계획의 복선. 가까운 사람도 눈치챘다.",
                    "Hallway", ClueCategory.General, isRequired: false),
            };
        }

        internal static ClueEntry[] GetPart2Entries()
        {
            return new[]
            {
                new ClueEntry("hasho_will", "hasho_will", "하시호 유서",
                    "하시호 본인 필체. 내 죽음은 의료 과실이 아니야. 나는 살해당한 거야.",
                    "스토리 최대 전환점. 공식 사인이 살인으로 뒤바뀐다.",
                    "Ward", ClueCategory.General, isRequired: true),
                new ClueEntry("medical_certificate", "medical_certificate", "진단서",
                    "하시호 사인: 수술 중 심정지. 담당의: 봉태현.",
                    "봉태현 미스디렉션 강화.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("conversation_memo", "conversation_memo", "대화 메모",
                    "유안나 필체. 태현 씨, 당신이 하시호를 죽인 거 알아.",
                    "봉태현 미스디렉션 최고조.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("bong_rebuttal", "bong_rebuttal", "봉태현의 반박문",
                    "봉태현 자필. 나는 최선을 다했다. 하시호의 죽음은 불가항력이었다.",
                    "봉태현이 범인이 아닐 수 있다는 첫 균열.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("ward_calendar", "ward_calendar", "병실 달력",
                    "2년 전 날짜에 하시호 수술일, 오늘 날짜에 별표.",
                    "2년 전부터 오늘을 복수의 날로 정해뒀다. 장기 계획 확정.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("oseojin_memo", "oseojin_memo", "오세진 협박 메모",
                    "오세진 필체. 그 남자한테 협박당했다. 기록을 지우지 않으면 나도 죽인다고 했다.",
                    "오세진은 공범이 아닌 피해자. 협박한 그 남자가 진세웅임을 암시.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("record_deletion", "record_deletion", "출입 기록 삭제 흔적",
                    "2년 전 수술일 당직 기록 삭제 흔적. 담당자 란에 오세진 이름 희미하게 남아있음.",
                    "진세웅이 2년 전부터 증거를 지워왔다는 것.",
                    "Ward", ClueCategory.General, isRequired: false),
                new ClueEntry("poison_ampoule", "poison_ampoule", "독약 앰플",
                    "라벨 없는 투명 약병.",
                    "진세웅이 사전에 빼돌린 범행 도구. 재고 일지와 연결.",
                    "Storage", ClueCategory.General, isRequired: true),
                new ClueEntry("hidden_camera", "hidden_camera", "소형 카메라",
                    "수술실 방향 고정 거치. 저장 영상 전부 삭제.",
                    "범행 흔적을 지우기 위한 사전 조작 증거.",
                    "Storage", ClueCategory.General, isRequired: false),
                new ClueEntry("jin_sneakers", "jin_sneakers", "운동화",
                    "밑창 흰 페인트가 발가락 쪽에만 묻어있다.",
                    "핵심 물증 A. 엎드린 자세로 수술대 아래 숨은 증거.",
                    "Storage", ClueCategory.General, isRequired: true),
                new ClueEntry("gloves", "gloves", "장갑",
                    "독약 앰플 옆 장갑. 손가락 끝 페인트 흔적 없음.",
                    "장갑 착용으로 지문 없음.",
                    "Storage", ClueCategory.General, isRequired: false),
                new ClueEntry("locked_locker", "locked_locker", "잠긴 사물함 내부 서류",
                    "코드 입력 해제 후 획득. 진세웅의 리허설 스케줄표.",
                    "수술실에서 수차례 리허설했다는 증거.",
                    "Storage", ClueCategory.General, isRequired: false),
            };
        }

        internal static ClueEntry[] GetPart3Entries()
        {
            return new[]
            {
                new ClueEntry("paint_footprints", "paint_footprints", "바닥 페인트 자국",
                    "분장실 바닥 흰 페인트 발자국. 수술대 방향으로 이어진다.",
                    "핵심 물증 B. 운동화 페인트와 방향 일치. 이동 동선 물리적 증명.",
                    "DressingRoom", ClueCategory.General, isRequired: true),
                new ClueEntry("makeup_diary", "makeup_diary", "진세웅 일기장",
                    "진세웅 필체. 유안나가 모든 걸 망쳐놨어. 하시호 형은 그 때문에 죽은 거야.",
                    "미스디렉션 완전 붕괴. 진세웅의 동기와 범행 의지 확정.",
                    "DressingRoom", ClueCategory.General, isRequired: true),
                new ClueEntry("mirror_message", "mirror_message", "거울 메모",
                    "거울에 립스틱으로 쓰인 글씨. 봐, 결국 네 차례야 — 세웅.",
                    "유안나를 특정 타겟으로 삼았음 확정.",
                    "DressingRoom", ClueCategory.General, isRequired: false),
                new ClueEntry("paint_toolbox", "paint_toolbox", "분장 도구함",
                    "분장 도구 사이 흰 페인트 튜브. 뚜껑 열림, 사용 흔적 있음.",
                    "운동화 페인트, 바닥 발자국의 출처 확정. 물증 체인 완성.",
                    "DressingRoom", ClueCategory.General, isRequired: false),
                new ClueEntry("under_table_space", "under_table_space", "수술대 하부 공간",
                    "수술대 아래 성인 한 명이 숨을 수 있는 공간. 페인트 자국 방향과 일치하는 흔적.",
                    "핵심 물증 C. 수술대 아래 숨어 발버둥 연기를 했다는 것이 공간적으로 증명된다.",
                    "OperatingRoom", ClueCategory.General, isRequired: true),
                new ClueEntry("yoanna_relic", "yoanna_relic", "유안나의 유품",
                    "수술대 옆 빈 약병. 독약 앰플과 동일 성분 흔적.",
                    "독약이 유안나에게 실제 사용됐음 확인. 앰플부터 사용까지 전 경로 완성.",
                    "OperatingRoom", ClueCategory.General, isRequired: false),
                new ClueEntry("nurse_log", "nurse_log", "간호사실 재고 일지",
                    "당일 마취약 1병 원인 불명 부족.",
                    "독약 앰플 출처를 공식 기록으로 뒷받침.",
                    "OperatingRoom", ClueCategory.General, isRequired: false),
                new ClueEntry("key_hint_note", "key_hint_note", "진세웅의 쪽지",
                    "병실 침대 밑 쪽지. 아무도 찾지 못할 곳에 뒀다. 제일 차가운 곳.",
                    "탈출 열쇠가 차가운 장소에 있다는 첫 번째 힌트.",
                    "Ward", ClueCategory.KeyClue, isRequired: true),
                new ClueEntry("key_hint_sticker", "key_hint_sticker", "온도 경고 스티커",
                    "냉장 약품함 문 스티커. 내용물 주의 — 4도 이하 보관.",
                    "제일 차가운 곳이 냉장 약품함임을 가리키는 두 번째 힌트.",
                    "Storage", ClueCategory.KeyClue, isRequired: true),
                new ClueEntry("key_hint_scratch", "key_hint_scratch", "긁힌 자국",
                    "냉장 약품함 문 표면 긁힌 흔적. 자물쇠 없음.",
                    "세 단서를 모두 모으면 냉장 약품함에서 탈출 열쇠 획득 가능.",
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
