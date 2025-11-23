using UnityEngine;

[ExecuteAlways]
public class ColliderAutoFixer : MonoBehaviour
{
    // Ajusta automáticamente un BoxCollider basado en el MeshFilter si detecta tamaños absurdos.
    void OnValidate()
    {
        FixColliderIfNeeded();
    }

    void Awake()
    {
        FixColliderIfNeeded();
    }

    void FixColliderIfNeeded()
    {
        var bc = GetComponent<BoxCollider>();
        if (bc == null) return;

        var mf = GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            var meshBounds = mf.sharedMesh.bounds;
            // Calcular tamaño mundial aproximado
            Vector3 worldSize = Vector3.Scale(meshBounds.size, transform.lossyScale);

            // Si el collider es demasiado pequeño en al menos un eje, lo ajustamos
            if (worldSize.x < 0.2f || worldSize.y < 0.05f || worldSize.z < 0.2f)
            {
                Vector3 newSize = new Vector3(
                    Mathf.Max(worldSize.x, 0.5f) / transform.lossyScale.x,
                    Mathf.Max(worldSize.y, 0.1f) / transform.lossyScale.y,
                    Mathf.Max(worldSize.z, 0.5f) / transform.lossyScale.z
                );

                bc.size = newSize;
                bc.center = meshBounds.center;
                Debug.Log($"ColliderAutoFixer: Ajustado BoxCollider en '{name}' (size={bc.size}, center={bc.center})");
            }
        }
        else
        {
            // Fallback: si el collider es prácticamente nulo, le damos un tamaño razonable
            if (bc.size.magnitude < 0.01f)
            {
                bc.size = new Vector3(1f, 0.2f, 1f);
                bc.center = Vector3.up * 0.1f;
                Debug.Log($"ColliderAutoFixer: Establecido BoxCollider por defecto en '{name}'");
            }
        }
    }
}
