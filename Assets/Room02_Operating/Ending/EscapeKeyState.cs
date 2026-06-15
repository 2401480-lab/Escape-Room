using System;

namespace EscapeRoom
{
    public static class EscapeKeyState
    {
        public static event Action<bool> OnKeyChanged;

        public static bool HasKey { get; private set; }

        public static void GrantKey()
        {
            if (HasKey)
            {
                return;
            }

            HasKey = true;
            OnKeyChanged?.Invoke(true);
        }

        public static void RevokeKey()
        {
            if (!HasKey)
            {
                return;
            }

            HasKey = false;
            OnKeyChanged?.Invoke(false);
        }

        public static void Reset()
        {
            HasKey = false;
            OnKeyChanged?.Invoke(false);
        }
    }
}
