using UnityEngine;

public class movimentacao : MonoBehaviour
{
    public float velocidade = 5.0f;
    public float velocidadeRotacao = 10f;

    private Rigidbody rb;
    private Transform cameraTransform;
    private Vector3 direcaoMovimento;

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
        // Se o diálogo estiver ativo, zera a movimentação
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
        {
            direcaoMovimento = Vector3.zero;
            return;
        }

        float movimentoHorizontal = Input.GetAxis("Horizontal");
        float movimentoVertical = Input.GetAxis("Vertical");

        if (cameraTransform != null)
        {
            Vector3 anguloCamera = new Vector3(0, cameraTransform.eulerAngles.y, 0);
            Quaternion rotacaoCamera = Quaternion.Euler(anguloCamera);

            Vector3 frenteCamera = rotacaoCamera * Vector3.forward;
            Vector3 direitaCamera = rotacaoCamera * Vector3.right;

            direcaoMovimento = (frenteCamera * movimentoVertical + direitaCamera * movimentoHorizontal).normalized;
        }

        // Rotação visual suave
        Quaternion rotacaoAlvo = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, Time.deltaTime * velocidadeRotacao);
    }

    void FixedUpdate()
    {
        // Movimento baseado em velocity para eliminar o tremor (jitter)
        Vector3 velocidadeDesejada = direcaoMovimento * velocidade;

        // Mantém a velocidade vertical atual do Rigidbody (para a gravidade continuar funcionando)
        velocidadeDesejada.y = rb.linearVelocity.y;

        rb.linearVelocity = velocidadeDesejada;
    }
}