using UnityEngine;

public class ReiniciarPosicao : MonoBehaviour
{
    public Transform pontoInicial;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se é o jogador pela Tag ou pelo nome do objeto
        if (other.CompareTag("Player") || other.gameObject.name == "Lucas")
        {
            // Se o personagem usa CharacterController, precisa desligar antes de teleportar
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
            }

            // Teleporta para o ponto inicial
            other.transform.position = pontoInicial.position;

            // Religar o CharacterController
            if (cc != null)
            {
                cc.enabled = true;
            }

            // Se usar Rigidbody em vez de CharacterController, zera a velocidade de queda
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
}