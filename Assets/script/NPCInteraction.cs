using Cozinha;
using Unity.Cinemachine;
using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Header("Diálogo")]
    public DialogueData dialogueData;
    public GameObject interactionPrompt;
    public float interactionRange = 3f;

    [Header("Modo Cozinha (opcional)")]
    [Tooltip("Marque para abrir a cozinha depois do diálogo. Desmarcado, o NPC só conversa.")]
    public bool abrirCozinhaAposDialogo = false;
    [Tooltip("Marcado: o diálogo só acontece na primeira vez. Depois o E vai direto para a cozinha.")]
    public bool dialogoSoUmaVez = true;
    public CinemachineCamera cameraBancada;
    public CozinhaManager cozinhaManager;

    private Transform player;
    private bool playerInRange = false;
    private bool aguardandoFimDoDialogo = false;
    private bool dialogoJaOcorreu = false;
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

        bool dialogoAtivo = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();

        // Esperava o diálogo acabar para abrir a cozinha
        if (aguardandoFimDoDialogo && !dialogoAtivo)
        {
            aguardandoFimDoDialogo = false;
            EntrarModoCozinha();
            return;
        }

        // Dentro da cozinha: E sai
        if (emModoCozinha)
        {
            if (Input.GetKeyDown(KeyCode.E))
                SairModoCozinha();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        bool inRangeNow = distance <= interactionRange;

        // Detecta quando entra ou sai do alcance
        if (inRangeNow && !playerInRange)
        {
            playerInRange = true;
            if (interactionPrompt != null && !dialogoAtivo)
                interactionPrompt.SetActive(true);
        }
        else if (!inRangeNow && playerInRange)
        {
            playerInRange = false;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }

        if (playerInRange && !dialogoAtivo && Input.GetKeyDown(KeyCode.E))
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            // Já falou antes e só quer cozinhar: vai direto
            if (abrirCozinhaAposDialogo && dialogoSoUmaVez && dialogoJaOcorreu)
            {
                EntrarModoCozinha();
                return;
            }

            if (DialogueManager.Instance == null || dialogueData == null)
            {
                Debug.LogError("DialogueManager não encontrado ou DialogueData está vazio!", this);
                return;
            }

            dialogoJaOcorreu = true;
            aguardandoFimDoDialogo = abrirCozinhaAposDialogo;
            DialogueManager.Instance.StartDialogue(dialogueData);
        }
    }

    void EntrarModoCozinha()
    {
        emModoCozinha = true;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (cameraBancada != null)
            cameraBancada.Priority = 20;

        SetMovimento(false);

        // Mostra o aviso "Aperte R para começar a cozinhar"
        if (cozinhaManager != null)
            cozinhaManager.AbrirMesa();
    }

    void SairModoCozinha()
    {
        emModoCozinha = false;

        if (cozinhaManager != null)
            cozinhaManager.Sair();

        if (cameraBancada != null)
            cameraBancada.Priority = 0;

        SetMovimento(true);
    }

    void SetMovimento(bool ligado)
    {
        if (player == null) return;
        var movScript = player.GetComponent<movimentacao>();
        if (movScript != null)
            movScript.enabled = ligado;
    }
}