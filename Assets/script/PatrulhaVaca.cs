using UnityEngine;
using UnityEngine.AI;

public class PatrulhaVaca : MonoBehaviour
{
    [Header("Configurações de Trajeto")]
    public Transform[] pontos;
    public float tempoParada = 3f;

    private NavMeshAgent agente;
    private Animator anim;
    private int indiceAtual = 0;
    private float cronometro = 0f;
    private bool esperando = false;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>(); // Busca o Animator dentro de SK_Cow

        if (pontos.Length > 0)
        {
            agente.SetDestination(pontos[indiceAtual].position);
        }
    }

    void Update()
    {
        if (pontos.Length == 0) return;

        // Calcula a distância real até o destino
        float distancia = Vector3.Distance(transform.position, pontos[indiceAtual].position);

        if (distancia <= agente.stoppingDistance + 0.5f)
        {
            // Chegou no ponto
            agente.isStopped = true;
            anim.SetBool("Andando", false); // Garante que muda para Idle

            if (!esperando)
            {
                esperando = true;
                cronometro = 0f;
            }

            cronometro += Time.deltaTime;

            if (cronometro >= tempoParada)
            {
                esperando = false;
                indiceAtual = (indiceAtual + 1) % pontos.Length;
                agente.isStopped = false;
                agente.SetDestination(pontos[indiceAtual].position);
            }
        }
        else
        {
            // Está andando
            agente.isStopped = false;
            anim.SetBool("Andando", true); // Garante que muda para Walk
        }
    }
}