using UnityEngine;
using Unity.Cinemachine;

public class BancadaCozinha : MonoBehaviour
{
    [Header("Configurações do Diálogo")]
    public DialogueData dialogueData; // Arraste o 'dialogo_fa' aqui!
    private bool dialogoJaOcorreu = false;
    private bool aguardandoFimDoDialogo = false;

    [Header("Câmera da Bancada")]
    public CinemachineCamera cameraBancada;

    [Header("UI e Elementos")]
    public GameObject interactionPrompt; // Ex: "Aperte E"
    public GameObject minimapObjeto;

    [Header("Configuração de Distância")]
    public float interactionRange = 5f; // Distância segura

    private Transform player;
    private bool playerInRange = false;
    private bool emModoCozinha = false;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool inRangeNow = distance <= interactionRange;

        // Verifica se o DialogueManager existe e se o diálogo está aberto na tela
        bool dialogoAtivo = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();

        // Se estávamos esperando o diálogo acabar e agora ele fechou
        if (aguardandoFimDoDialogo && !dialogoAtivo)
        {
            aguardandoFimDoDialogo = false;
            EntrarModoCozinha();
            return;
        }

        // Se NÃO estamos no modo cozinha e NÃO há diálogo ativo
        if (!emModoCozinha && !dialogoAtivo)
        {
            if (inRangeNow && !playerInRange)
            {
                playerInRange = true;
                if (interactionPrompt != null)
                    interactionPrompt.SetActive(true);
            }
            else if (!inRangeNow && playerInRange)
            {
                playerInRange = false;
                if (interactionPrompt != null)
                    interactionPrompt.SetActive(false);
            }

            // Pressionar E para interagir
            if (playerInRange && Input.GetKeyDown(KeyCode.E))
            {
                if (!dialogoJaOcorreu)
                {
                    // Primeira vez: Inicia o diálogo
                    if (interactionPrompt != null)
                        interactionPrompt.SetActive(false);

                    if (DialogueManager.Instance != null && dialogueData != null)
                    {
                        dialogoJaOcorreu = true;
                        aguardandoFimDoDialogo = true;
                        DialogueManager.Instance.StartDialogue(dialogueData);
                    }
                    else
                    {
                        Debug.LogError("DialogueManager não encontrado ou DialogueData está vazio!");
                    }
                }
                else
                {
                    // Vezes seguintes: Vai direto para o modo cozinha
                    EntrarModoCozinha();
                }
            }
        }
        else if (emModoCozinha)
        {
            // Se estiver no modo cozinha e apertar E, sai dele
            if (Input.GetKeyDown(KeyCode.E))
            {
                SairModoCozinha();
            }
        }
    }

    void EntrarModoCozinha()
    {
        emModoCozinha = true;

        if (cameraBancada != null)
            cameraBancada.Priority = 20; // Ativa a câmera da bancada

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (minimapObjeto != null)
            minimapObjeto.SetActive(false);

        if (player != null)
        {
            var movScript = player.GetComponent<movimentacao>();
            if (movScript != null)
                movScript.enabled = false;
        }
    }

    void SairModoCozinha()
    {
        emModoCozinha = false;

        if (cameraBancada != null)
            cameraBancada.Priority = 0; // Devolve a prioridade para a câmera normal

        if (minimapObjeto != null)
            minimapObjeto.SetActive(true);

        if (player != null)
        {
            var movScript = player.GetComponent<movimentacao>();
            if (movScript != null)
                movScript.enabled = true;
        }
    }
}