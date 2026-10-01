using UnityEngine;

public class MapFollow : MonoBehaviour
{
    public Transform player; // Arraste o seu Player aqui no Inspector
    public float altura = 20f; // Altura da câmera em relação ao player

    void LateUpdate()
    {
        if (player != null)
        {
            // Mantém a posição X e Z do player, mas trava a altura (Y) fixa
            Vector3 novaPosicao = player.position;
            novaPosicao.y += altura;
            transform.position = novaPosicao;

            // Mantém a câmera sempre olhando retamente para baixo
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}