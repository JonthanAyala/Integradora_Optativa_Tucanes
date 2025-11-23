using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public static class PlayerRestoreCollidersMenu
{
    [MenuItem("Tools/Player Fixer/Restore Colliders on Selected Player(s)")]
    public static void RestoreColliders()
    {
        var objs = Selection.gameObjects;
        if (objs == null || objs.Length == 0)
        {
            EditorUtility.DisplayDialog("Restore Colliders", "Selecciona el GameObject del Player en el Hierarchy antes de ejecutar.", "OK");
            return;
        }

        int restored = 0;
        foreach (var go in objs)
        {
            var cols = go.GetComponents<Collider>();
            foreach (var c in cols)
            {
                if (!c.enabled)
                {
                    Undo.RecordObject(c, "Restore collider");
                    c.enabled = true;
                    restored++;
                }
            }

            var rb = go.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Undo.RecordObject(rb, "Enable collisions");
                rb.detectCollisions = true;
                // Dejar isKinematic como estaba; si está kinematic, el usuario puede cambiarlo manualmente
            }

            EditorUtility.SetDirty(go);
        }

        EditorUtility.DisplayDialog("Restore Colliders", $"Restaurados {restored} colliders en los objetos seleccionados.", "OK");
    }
}
#endif
