using Cozinha;
using Unity.Cinemachine;
using UnityEngine;

public class BancadaCozinha : MonoBehaviour
{
    [Header("Configurações do Diálogo")]
    public DialogueData dialogueData;
    private bool dialogoJaOcorreu = false;
    private bool aguardandoFimDoDialogo = false;

    [Header("Câmera da Bancada")]
    public CinemachineCamera cameraBancada;

    [Header("UI e Elementos")]
    public GameObject interactionPrompt;
    public GameObject minimapObjeto;

    [Header("Configuração de Distância")]
    public float interactionRange = 5f;

    [Header("Cozinha")]
    public CozinhaManager cozinhaManager;

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

        bool dialogoAtivo = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();

        // Se estávamos esperando o diálogo acabar e agora ele fechou
        if (aguardandoFimDoDialogo && !dialogoAtivo)
        {
            aguardandoFimDoDialogo = false;
            EntrarModoCozinha();
            return;
        }

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
        DiarioProgresso.bloqueado = true; // Diário não abre durante a cozinha

        if (cameraBancada != null)
            cameraBancada.Priority = 20;

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

        // Mostra o aviso "Aperte R para começar a cozinhar"
        if (cozinhaManager != null)
            cozinhaManager.AbrirMesa();
    }

    void SairModoCozinha()
    {
        emModoCozinha = false;
        DiarioProgresso.bloqueado = false;

        // Esconde os painéis e para a cozinha
        if (cozinhaManager != null)
            cozinhaManager.Sair();

        if (cameraBancada != null)
            cameraBancada.Priority = 0;

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