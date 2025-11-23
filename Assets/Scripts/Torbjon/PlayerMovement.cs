using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float gravity = 20f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 moveDirection = Vector3.zero;
    private float currentInput = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        GameEvents.OnMove += HandleMove;
        GameEvents.OnJump += HandleJump;
    }

    private void OnDisable()
    {
        GameEvents.OnMove -= HandleMove;
        GameEvents.OnJump -= HandleJump;
    }

    private void HandleMove(float input)
    {
        currentInput = input;
    }

    private void HandleJump()
    {
        if (controller.isGrounded)
        {
            moveDirection.y = jumpForce;
            // Activamos animación de salto
            if (animator) animator.SetBool("IsJumping", true);
        }
    }

    void Update()
    {
        // 1. Corrección de Carril Z
        if (transform.position.z != 0)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        }

        // 2. Guardar velocidad vertical previa
        float yStore = moveDirection.y;

        // 3. Movimiento Horizontal
        moveDirection = new Vector3(currentInput * moveSpeed, yStore, 0);

        // 4. Rotación
        if (currentInput != 0)
        {
            transform.forward = new Vector3(currentInput, 0, 0);
        }

        // Animación de correr
        if (animator) animator.SetFloat("Speed", Mathf.Abs(currentInput));

        // 5. LÓGICA CORREGIDA DE ATERRIZAJE
        if (controller.isGrounded)
        {
            // SI LA VELOCIDAD ES NEGATIVA O CERO (Estamos bajando o quietos)
            // ENTONCES sí aterrizamos y apagamos la animación de salto.
            // (Esto evita que se apague justo cuando despegamos con velocidad positiva)
            if (moveDirection.y <= 0)
            {
                if (animator) animator.SetBool("IsJumping", false);
                moveDirection.y = -2f; // Pegar al suelo
            }
        }
        else
        {
            // Gravedad (Solo si estamos en el aire)
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // 6. Mover
        controller.Move(moveDirection * Time.deltaTime);
    }
}