using UnityEngine;

public class MapController : MonoBehaviour
{
    public Camera cam;
    public float panSpeed = 0.5f;
    public float zoomSpeedTouch = 0.1f; // Untuk pinch zoom
    public float zoomSpeedMouse = 2f;   // Untuk scroll wheel
    public float minZoom = 5f;
    public float maxZoom = 20f;

    private Vector3 lastPanPosition;
    private int panFingerId;
    private bool isPanning;

    private void Update()
    {
#if UNITY_EDITOR
        // Panning pakai mouse (klik kiri tahan + geser)
        if (Input.GetMouseButtonDown(0))
        {
            lastPanPosition = Input.mousePosition;
            isPanning = true;
        }
        else if (Input.GetMouseButton(0) && isPanning)
        {
            PanCamera(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isPanning = false;
        }

        // Zoom pakai scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        ZoomCamera(scroll * zoomSpeedMouse);
#else
        // --- Kontrol di Android (Touch) ---
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastPanPosition = touch.position;
                panFingerId = touch.fingerId;
                isPanning = true;
            }
            else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
            {
                PanCamera(touch.position);
            }
            else if (touch.fingerId == panFingerId && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
            {
                isPanning = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 prevTouch1Pos = touch1.position - touch1.deltaPosition;
            Vector2 prevTouch2Pos = touch2.position - touch2.deltaPosition;

            float prevMagnitude = (prevTouch1Pos - prevTouch2Pos).magnitude;
            float currentMagnitude = (touch1.position - touch2.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            ZoomCamera(difference * zoomSpeedTouch);
        }
#endif
    }

    void PanCamera(Vector3 newPanPosition)
    {
        Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        Vector3 move = new Vector3(offset.x * panSpeed, 0, offset.y * panSpeed);

        transform.Translate(move, Space.World);

        lastPanPosition = newPanPosition;
    }

    void ZoomCamera(float increment)
    {
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - increment, minZoom, maxZoom);
    }
}
