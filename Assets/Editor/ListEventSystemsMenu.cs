using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.EventSystems;

public static class ListEventSystemsMenu
{
    [MenuItem("Tools/UI/List EventSystems in Scene")]
    public static void ListAll()
    {
        var all = Object.FindObjectsOfType<EventSystem>(true);
        if (all == null || all.Length == 0)
        {
            Debug.Log("ListEventSystems: No EventSystems found in scene.");
            EditorUtility.DisplayDialog("List EventSystems", "No EventSystems found in the scene.", "OK");
            return;
        }

        Debug.Log($"ListEventSystems: Found {all.Length} EventSystem(s):");
        foreach (var es in all)
        {
            string path = GetHierarchyPath(es.transform);
            Debug.Log($"- Name: '{es.gameObject.name}' | ActiveInHierarchy: {es.gameObject.activeInHierarchy} | ActiveSelf: {es.gameObject.activeSelf} | Path: {path}");
        }

        EditorUtility.DisplayDialog("List EventSystems", $"Found {all.Length} EventSystem(s). See Console for details.", "OK");
    }

    static string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        var cur = t.parent;
        while (cur != null)
        {
            path = cur.name + "/" + path;
            cur = cur.parent;
        }
        return path;
    }
}
#endif
