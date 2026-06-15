using UnityEngine;

namespace EscapeRoom
{
    public static class EscapeKeyState
    {
        public static bool HasKey { get; private set; }

        public static void GrantKey()
        {
            HasKey = true;
        }

        public static void ClearKey()
        {
            HasKey = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntimeState()
        {
            ClearKey();
        }
    }
}
