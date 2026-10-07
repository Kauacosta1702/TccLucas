using UnityEngine;
using UnityEngine.SceneManagement;

public class EntrarRestaurante1 : MonoBehaviour
{
    public GameObject interactionPrompt; // O Canvas "Aperte E para entrar"
    public float interactionRange = 3f;
    public string Restaurante;
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

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            SceneManager.LoadScene(Restaurante);
        }
    }
}