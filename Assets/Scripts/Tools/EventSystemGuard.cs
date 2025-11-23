using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
public class EventSystemGuard : MonoBehaviour
{
    // Si está activo, este objeto se encargará de mantener exactamente un EventSystem activo en la escena.
    void OnEnable()
    {
        EnsureSingleEventSystem();
    }

    void Update()
    {
        // En modo Editor queremos vigilar cambios frecuentes; en Play basta con comprobar menos.
        if (!Application.isPlaying)
            EnsureSingleEventSystem();
    }

    void EnsureSingleEventSystem()
    {
        var all = Object.FindObjectsOfType<EventSystem>(true);
        if (all == null || all.Length <= 1) return;

        // Encontrar uno preferido: el que está activo en jerarquía, si existe
        EventSystem keeper = null;
        foreach (var es in all)
        {
            if (es.gameObject.activeInHierarchy)
            {
                keeper = es;
                break;
            }
        }

        // Si ninguno está activo, elegimos el primero como keeper y lo activamos
        if (keeper == null)
        {
            keeper = all[0];
            keeper.gameObject.SetActive(true);
        }

        // Desactivar todos los demás EventSystems
        int disabled = 0;
        foreach (var es in all)
        {
            if (es == keeper) continue;
            if (es.gameObject.activeInHierarchy)
            {
                es.gameObject.SetActive(false);
                disabled++;
            }
        }

        if (disabled > 0)
            Debug.Log($"EventSystemGuard: Desactivados {disabled} EventSystem(s) redundantes.");
    }
}
