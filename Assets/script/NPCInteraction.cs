using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public DialogueData dialogueData;
    public GameObject interactionPrompt;
    public float interactionRange = 3f;

    private Transform player;
    private bool playerInRange = false;

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

        // Detecta quando entra ou sai do alcance
        if (inRangeNow && !playerInRange)
        {
            playerInRange = true;
            if (interactionPrompt != null && !DialogueManager.Instance.IsDialogueActive())
                interactionPrompt.SetActive(true);
        }
        else if (!inRangeNow && playerInRange)
        {
            playerInRange = false;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !DialogueManager.Instance.IsDialogueActive())
        {
            DialogueManager.Instance.StartDialogue(dialogueData);

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }
}