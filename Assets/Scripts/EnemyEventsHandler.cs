using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyEventsHandler : MonoBehaviour
{
    public AudioClip stompClip;
    public AudioClip hitClip;
    public int stompPoints = 100;

    AudioSource src;

    void Awake()
    {
        src = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        EnemySimpleMove.OnEnemyStomped += HandleStomp;
        EnemySimpleMove.OnEnemyHitPlayer += HandleHitPlayer;
    }

    void OnDisable()
    {
        EnemySimpleMove.OnEnemyStomped -= HandleStomp;
        EnemySimpleMove.OnEnemyHitPlayer -= HandleHitPlayer;
    }

    void HandleStomp(EnemySimpleMove enemy, GameObject player)
    {
        // reproducir sonido en la posición del enemigo
        if (stompClip != null && src != null)
            src.PlayOneShot(stompClip);

        // sumar puntos
        ScoreManager.AddPoints(stompPoints);
    }

    void HandleHitPlayer(EnemySimpleMove enemy, GameObject player)
    {
        if (hitClip != null && src != null)
            src.PlayOneShot(hitClip);

        // aquí podrías reducir vida del jugador, mostrar efecto, etc.
        Debug.Log($"Enemy hit player: {enemy.name}");
    }
}
