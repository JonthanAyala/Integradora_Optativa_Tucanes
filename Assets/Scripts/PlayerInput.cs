using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    void Update()
    {
        // 1. Movimiento (Detecta Flechas o teclas A/D)
        float horizontal = Input.GetAxis("Horizontal");
        GameEvents.OnMove?.Invoke(horizontal);

        // 2. Salto (Detecta tecla Espacio)
        if (Input.GetButtonDown("Jump"))
        {
            GameEvents.OnJump?.Invoke();
        }

        // 3. Ataque Básico (Tecla F)
        if (Input.GetKeyDown(KeyCode.F))
        {
            GameEvents.OnAttack?.Invoke();
        }

        // 4. Recolección / Interacción (Tecla E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            GameEvents.OnInteract?.Invoke();
        }

        // 5. Power-Up (Tecla Q)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GameEvents.OnUsePowerUp?.Invoke();
        }

        // 6. Pausa (Tecla P)
        if (Input.GetKeyDown(KeyCode.P))
        {
            GameEvents.OnPause?.Invoke();
        }
    }
}