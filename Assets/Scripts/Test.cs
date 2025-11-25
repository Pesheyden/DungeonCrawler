using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform target;

    void OnDrawGizmos()
    {
        if (target == null) return;

        // Drone's local axes
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 up = transform.up;

        // Vector from drone to target
        Vector3 toTarget = target.position - transform.position;

        // Project onto local axes
        float forwardDist = Vector3.Dot(toTarget, forward);
        float rightDist = Vector3.Dot(toTarget, right);
        float upDist = Vector3.Dot(toTarget, up);

        // Draw base line to target
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, target.position);

        // Draw axis-aligned projections
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + forward * forwardDist);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + right * rightDist);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + up * upDist);

        // Optional: label distances
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + forward * forwardDist, $"Forward: {forwardDist:F2}");
        UnityEditor.Handles.Label(transform.position + right * rightDist, $"Right: {rightDist:F2}");
        UnityEditor.Handles.Label(transform.position + up * upDist, $"Up: {upDist:F2}");
#endif
    }
}