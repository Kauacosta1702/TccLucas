using UnityEngine;

public class MiniMissao : MonoBehaviour
{
    public Transform target;      // O ponto da missão no mundo 3D
    public Transform player;      // O jogador
    public RectTransform iconRect; // O RectTransform desse próprio ícone (arraste o mesmo objeto)

    public float mapScale = 1f;   // Ajusta a escala entre mundo 3D e minimapa

    void LateUpdate()
    {
        if (target == null || player == null) return;

        // Calcula a diferença de posição entre o alvo e o jogador, no plano XZ
        Vector3 offset = target.position - player.position;

        // Converte pra posição 2D no minimapa (X do mundo = X do mapa, Z do mundo = Y do mapa)
        Vector2 mapPosition = new Vector2(offset.x, offset.z) * mapScale;

        iconRect.anchoredPosition = mapPosition;
    }
}