using UnityEngine;

public class RaycastDebug : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        if (player == null) return;

        Vector3 eyePos = transform.position + Vector3.up * 1.0f;
        Vector3 dirToPlayer = (player.position - eyePos).normalized;
        float distance = 50f;

        // dibuja el raycast siempre
        Debug.DrawRay(eyePos, dirToPlayer * distance, Color.yellow, 0.1f);
    }
}
