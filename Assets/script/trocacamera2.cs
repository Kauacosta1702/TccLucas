using UnityEngine;
using Unity.Cinemachine;

public class trocacamera2 : MonoBehaviour
{
    [Header("Diálogo")]
    public DialogueData dialogueData;
    private bool dialogoJaOcorreu = false;
    private bool esperandoFimDialogo = false;

    [Header("Câmera")]
    public CinemachineCamera cameraBancada;

    [Header("UI")]
    public GameObject interactionPrompt;
    public GameObject minimapObjeto;

    [Header("Configurações")]
    public float interactionRange = 3f;

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

        // Verifica se o DialogueManager existe e se o diálogo está rodando na tela
        bool dialogoAtivo = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();

        // Se o jogo estava esperando o diálogo acabar e agora ele ficou falso (acabou)
        if (esperandoFimDialogo && !dialogoAtivo)
        {
            esperandoFimDialogo = false;
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

            // Se apertar E perto da bancada
            if (playerInRange && Input.GetKeyDown(KeyCode.E))
            {
                if (!dialogoJaOcorreu)
                {
                    dialogoJaOcorreu = true;
                    esperandoFimDialogo = true;

                    if (interactionPrompt != null)
                        interactionPrompt.SetActive(false);

                    // Inicia o diálogo com segurança
                    if (DialogueManager.Instance != null)
                    {
                        DialogueManager.Instance.StartDialogue(dialogueData);
                    }
                    else
                    {
                        Debug.LogError("DialogueManager.Instance não foi encontrado! Verifique se há um DialogueManager na cena.");
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
    }

    void SairModoCozinha()
    {
        emModoCozinha = false;

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