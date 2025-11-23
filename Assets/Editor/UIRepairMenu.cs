using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.EventSystems;

public static class UIRepairMenu
{
    [MenuItem("Tools/UI/Restore HUD & Ensure EventSystem")]
    public static void RestoreUIAndEventSystem()
    {
        int canvases = 0;
        foreach (var c in Object.FindObjectsOfType<Canvas>(true))
        {
            if (!c.gameObject.activeInHierarchy)
            {
                Undo.RecordObject(c.gameObject, "Enable Canvas");
                c.gameObject.SetActive(true);
            }
            canvases++;
        }

        // Ensure single EventSystem
        var all = Object.FindObjectsOfType<EventSystem>(true);
        if (all == null || all.Length == 0)
        {
            // Create one
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
            Debug.Log("UIRepair: Created new EventSystem.");
        }
        else if (all.Length > 1)
        {
            // Keep the first active one and disable others
            EventSystem keeper = null;
            foreach (var es in all)
            {
                if (es.gameObject.activeInHierarchy)
                {
                    keeper = es;
                    break;
                }
            }
            if (keeper == null) keeper = all[0];

            for (int i = 0; i < all.Length; i++)
            {
                var es = all[i];
                if (es == keeper) continue;
                if (es.gameObject.activeInHierarchy)
                {
                    Undo.RecordObject(es.gameObject, "Disable extra EventSystem");
                    es.gameObject.SetActive(false);
                }
            }
            Debug.Log($"UIRepair: Kept one EventSystem and disabled {all.Length-1} extras.");
        }

        EditorUtility.DisplayDialog("UI Repair", $"Canvases found: {canvases}. EventSystem ensured.", "OK");
    }
}
#endif
