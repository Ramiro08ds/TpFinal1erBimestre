using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentScript : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;   // puntos de patrulla
    public float arriveThreshold = 0.5f;

    [Header("Detection Settings")]
    public Transform player;           // jugador
    public float viewDistance = 50f;   // distancia máxima de visión
    public float viewAngle = 180f;      // ángulo de visión (cono)
    public float loseTime = 2f;        // tiempo antes de volver al patrullaje

    [Header("Misc")]
    public Animator anim;              // animador del NPC
    public bool debugDraw = true;      // dibujar raycast para debug

    private NavMeshAgent agent;
    private int currentPointIndex = 0;
    private bool chasing = false;
    private float loseTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (anim == null)
            anim = GetComponent<Animator>();
    }

    private void Start()
    {
        if (patrolPoints.Length > 0)
            agent.destination = patrolPoints[currentPointIndex].position;
    }

    private void Update()
    {
        // actualizar animación según velocidad
        if (anim != null)
            anim.SetFloat("Speed", agent.velocity.magnitude);

        if (chasing)
        {
            // perseguir al jugador
            if (player != null)
                agent.destination = player.position;

            if (CanSeePlayer())
            {
                loseTimer = 0f; // sigue viendo al jugador
            }
            else
            {
                // si lo pierde, empieza a contar
                loseTimer += Time.deltaTime;
                if (loseTimer >= loseTime)
                {
                    chasing = false;
                    NextPatrolPoint();
                }
            }

            // si lo alcanza -> derrota
            if (player != null && Vector3.Distance(transform.position, player.position) < 1.5f)
            {
                Debug.Log("Game Over - NPC atrapó al jugador");
                // SceneManager.LoadScene("GameOverScene"); // descomentar si querés cambiar de escena
            }
        }
        else
        {
            // patrullaje normal
            if (!agent.pathPending && agent.remainingDistance <= arriveThreshold)
                NextPatrolPoint();

            // chequea si ve al jugador
            if (CanSeePlayer())
            {
                chasing = true;
                loseTimer = 0f;
            }
        }
    }

    void NextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        agent.destination = patrolPoints[currentPointIndex].position;
    }

    bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.0f; // altura de ojos del NPC
        Vector3 dirToPlayer = (player.position - eyePos).normalized;
        float distToPlayer = Vector3.Distance(eyePos, player.position);

        // dibujar raycast siempre para debug
        if (debugDraw)
            Debug.DrawRay(eyePos, dirToPlayer * viewDistance, Color.yellow);

        // chequear distancia
        if (distToPlayer > viewDistance)
            return false;

        // chequear ángulo
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > viewAngle)
            return false;

        // chequear obstáculos
        if (Physics.Raycast(eyePos, dirToPlayer, out RaycastHit hit, viewDistance))
        {
            if (hit.transform == player || hit.transform.CompareTag("Player"))
                return true;
        }

        return false;
    }
}
