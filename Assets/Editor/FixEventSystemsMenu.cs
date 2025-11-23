using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.EventSystems;

public static class FixEventSystemsMenu
{
    [MenuItem("Tools/UI/Fix Duplicate EventSystems in Scene")]
    public static void FixEventSystems()
    {
        var all = Object.FindObjectsOfType<EventSystem>();
        if (all == null || all.Length <= 1)
        {
            EditorUtility.DisplayDialog("Fix EventSystems", "No se encontraron EventSystems duplicados.", "OK");
            return;
        }

        // Mantener el primero activo y desactivar el resto
        int kept = 0;
        for (int i = 0; i < all.Length; i++)
        {
            var es = all[i];
            if (kept == 0 && es.gameObject.activeInHierarchy)
            {
                kept = 1;
                continue;
            }
            Undo.RecordObject(es.gameObject, "Disable extra EventSystem");
            es.gameObject.SetActive(false);
        }

        Debug.Log($"FixEventSystems: Encontrados {all.Length} EventSystems. Se mantuvo 1 y se desactivaron {all.Length-1}.");
        EditorUtility.DisplayDialog("Fix EventSystems", $"Se mantuvo 1 EventSystem y se desactivaron {all.Length-1} objetos.", "OK");
    }
}
#endif
