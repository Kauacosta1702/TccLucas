using UnityEngine;

public class MapFollow : MonoBehaviour
{
    public Transform player;
    public float altura = 20f; // Altura da câmera em relação ao player

    void LateUpdate()
    {
        if (player != null)
        {
            
            Vector3 novaPosicao = player.position;
            novaPosicao.y += altura;
            transform.position = novaPosicao;

            
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}