using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        Vector3 direction = cam.position - transform.position;
        direction.y = 0; // Trava a rotação vertical, só gira no eixo Y (horizontal)

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(-direction);
        }
    }
}