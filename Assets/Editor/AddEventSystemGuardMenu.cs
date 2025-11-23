using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public static class AddEventSystemGuardMenu
{
    [MenuItem("Tools/UI/Add EventSystemGuard To Scene")]
    public static void AddGuard()
    {
        // Buscar por GameObject llamado 'EventSystemGuard' en la escena
        var go = GameObject.Find("EventSystemGuard");
        if (go != null)
        {
            EditorUtility.DisplayDialog("EventSystemGuard", "Ya existe un GameObject 'EventSystemGuard' en la escena.", "OK");
            Selection.activeGameObject = go;
            return;
        }

        var newGo = new GameObject("EventSystemGuard");
        // Try to add the component by type name; if assembly hasn't compiled yet, user can add manually
        var comp = newGo.AddComponent(System.Type.GetType("EventSystemGuard"));
        if (comp == null)
        {
            Debug.LogWarning("AddEventSystemGuardMenu: No se encontró el tipo 'EventSystemGuard' al crear el GameObject. Añade el componente manualmente si es necesario.");
        }
        Undo.RegisterCreatedObjectUndo(newGo, "Create EventSystemGuard");
        Selection.activeGameObject = newGo;
        EditorUtility.DisplayDialog("EventSystemGuard", "Se creó 'EventSystemGuard' en la escena. Comprueba el Inspector y guarda la escena.", "OK");
    }
}
#endif
