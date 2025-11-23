using System;
using UnityEngine;

public static class GameEvents
{
    // Eventos de movimiento y acciones
    public static Action<float> OnMove; // Valor entre -1 y 1
    public static Action OnJump;
    public static Action OnAttack;
    public static Action OnInteract;
    public static Action OnUsePowerUp;
    public static Action OnPause;
}