using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public static class ColliderFixerMenu
{
    [MenuItem("Tools/Collider Fixer/Fix Platform Colliders in Scene")]
    public static void FixAllPlatformColliders()
    {
        var all = Object.FindObjectsOfType<GameObject>();
        int fixedCount = 0;
        foreach (var go in all)
        {
            if (go.name.ToLower().Contains("platform") || go.CompareTag("Platform"))
            {
                var bc = go.GetComponent<BoxCollider>();
                if (bc == null) continue;
                var mf = go.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    var b = mf.sharedMesh.bounds;
                    Vector3 worldSize = Vector3.Scale(b.size, go.transform.lossyScale);
                    bc.size = new Vector3(
                        Mathf.Max(worldSize.x, 0.5f) / go.transform.lossyScale.x,
                        Mathf.Max(worldSize.y, 0.1f) / go.transform.lossyScale.y,
                        Mathf.Max(worldSize.z, 0.5f) / go.transform.lossyScale.z
                    );
                    bc.center = b.center;
                    EditorUtility.SetDirty(go);
                    fixedCount++;
                }
                else
                {
                    if (bc.size.magnitude < 0.01f)
                    {
                        bc.size = new Vector3(1f, 0.2f, 1f);
                        bc.center = Vector3.up * 0.1f;
                        EditorUtility.SetDirty(go);
                        fixedCount++;
                    }
                }
            }
        }
        Debug.Log($"ColliderFixer: fixed {fixedCount} platform colliders in scene.");
    }
}
#endif
