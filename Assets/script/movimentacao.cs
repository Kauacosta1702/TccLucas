using UnityEngine;

public class movimentacao : MonoBehaviour
{
    public float velocidade = 5.0f;
    public float velocidadeRotacao = 10f;

    private Rigidbody rb;
    private Transform cameraTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Se o diálogo estiver ativo, não processa movimento nenhum
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
        {
            return;
        }

        float movimentoHorizontal = Input.GetAxis("Horizontal");
        float movimentoVertical = Input.GetAxis("Vertical");

        Vector3 anguloCamera = new Vector3(0, cameraTransform.eulerAngles.y, 0);
        Quaternion rotacaoCamera = Quaternion.Euler(anguloCamera);

        Vector3 frenteCamera = rotacaoCamera * Vector3.forward;
        Vector3 direitaCamera = rotacaoCamera * Vector3.right;

        Vector3 direcao = (frenteCamera * movimentoVertical + direitaCamera * movimentoHorizontal).normalized;

        if (direcao.magnitude >= 0.1f)
        {
            Vector3 deslocamento = direcao * velocidade * Time.deltaTime;
            rb.MovePosition(transform.position + deslocamento);
        }

        Quaternion rotacaoAlvo = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, Time.deltaTime * velocidadeRotacao);
    }
}