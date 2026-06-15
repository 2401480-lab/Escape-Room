#if UNITY_EDITOR
using UnityEngine;

namespace EscapeRoom
{
    public class EscapeKeyDebugGrant : MonoBehaviour
    {
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.F9))
            {
                return;
            }

            EscapeKeyState.GrantKey();
            Debug.Log("[EscapeKeyDebugGrant] Test key granted.");
        }
    }
}
#endif
