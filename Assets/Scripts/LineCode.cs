using UnityEngine;

public class LineCode : MonoBehaviour
{
    LineRenderer lineRenderer;
    public Transform origin;
    public Transform target;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    void Update()
    {
        lineRenderer.SetPosition(0, origin.position);
        lineRenderer.SetPosition(1, target.position);
    }
}
