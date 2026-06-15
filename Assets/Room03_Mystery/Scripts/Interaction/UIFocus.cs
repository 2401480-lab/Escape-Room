namespace Room03Mystery
{
    // UI(단서 팝업 등)가 열려 있는 동안 1인칭 조작을 멈추고 커서를 풀기 위한 공용 카운터.
    // 패널을 열 때 Push(), 닫을 때 Pop().
    public static class UIFocus
    {
        public static int Count;

        public static bool Active => Count > 0;
        public static void Push() => Count++;
        public static void Pop() { Count--; if (Count < 0) Count = 0; }
        public static void Reset() => Count = 0;
    }
}
