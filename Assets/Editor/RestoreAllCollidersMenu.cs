using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public static class RestoreAllCollidersMenu
{
    [MenuItem("Tools/Player Fixer/Restore All Player+Enemy Colliders")]
    public static void RestoreAll()
    {
        var all = Object.FindObjectsOfType<GameObject>(true);
        int restored = 0;
        foreach (var go in all)
        {
            if (go.CompareTag("Player") || go.CompareTag("Enemy") || go.name.ToLower().Contains("player") || go.name.ToLower().Contains("enemy"))
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
                }

                EditorUtility.SetDirty(go);
            }
        }

        EditorUtility.DisplayDialog("Restore Colliders", $"Restaurados {restored} colliders en Player/Enemy encontrados.", "OK");
    }
}
#endif
