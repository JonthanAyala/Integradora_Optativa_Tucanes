using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public static class PlayerFixerMenu
{
    [MenuItem("Tools/Player Fixer/Fix Selected Player(s)")]
    public static void FixSelectedPlayers()
    {
        var objs = Selection.gameObjects;
        if (objs == null || objs.Length == 0)
        {
            EditorUtility.DisplayDialog("Player Fixer", "Selecciona el GameObject del Player en el Hierarchy antes de ejecutar.", "OK");
            return;
        }

        int fixedCount = 0;
        foreach (var go in objs)
        {
            if (go == null) continue;

            // Añadir CharacterController si falta
            var cc = go.GetComponent<CharacterController>();
            if (cc == null)
            {
                cc = Undo.AddComponent<CharacterController>(go);
                cc.stepOffset = 0.15f;
                cc.skinWidth = 0.08f;
                cc.center = new Vector3(0f, 1f, 0f);
                cc.height = 2f;
                Debug.Log($"PlayerFixer: Añadido CharacterController a '{go.name}'");
            }

            // Desactivar colliders que entren en conflicto (CapsuleCollider, BoxCollider)
            var colliders = go.GetComponents<Collider>();
            foreach (var col in colliders)
            {
                if (col is CharacterController) continue;
                // Preferimos desactivar el collider para evitar eliminar datos del prefab
                Undo.RecordObject(col, "Disable collider");
                col.enabled = false;
                Debug.Log($"PlayerFixer: Desactivado collider {col.GetType().Name} en '{go.name}'");
            }

            // Si tiene Rigidbody, lo ponemos kinematic y deshabilitamos detección de colisiones
            var rb = go.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Undo.RecordObject(rb, "Configure Rigidbody");
                rb.isKinematic = true;
                rb.detectCollisions = false;
                Debug.Log($"PlayerFixer: Rigidbody en '{go.name}' puesto isKinematic=true y detectCollisions=false");
            }

            EditorUtility.SetDirty(go);
            fixedCount++;
        }

        EditorUtility.DisplayDialog("Player Fixer", $"Procesados {fixedCount} GameObject(s). Revisa el Hierarchy y prueba en Play.", "OK");
    }
}
#endif
