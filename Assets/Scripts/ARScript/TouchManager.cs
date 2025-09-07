using UnityEngine;

public class TouchManagers : MonoBehaviour
{
    Camera arCamera;

    void Start()
    {
        arCamera = Camera.main;
    }

    void Update()
    {
        // Cek sentuhan layar
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = arCamera.ScreenPointToRay(touch.position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Mainkan suara not
                AudioSource audioSrc = hit.transform.GetComponent<AudioSource>();
                if (audioSrc != null)
                {
                    audioSrc.Play();
                }

                // Ubah warna sementara untuk efek ditekan
                Renderer rend = hit.transform.GetComponent<Renderer>();
                if (rend != null)
                {
                    Color originalColor = rend.material.color;
                    rend.material.color = Color.red;
                    StartCoroutine(RevertColorAfterDelay(rend, originalColor, 0.2f));
                }
            }
        }
    }

    System.Collections.IEnumerator RevertColorAfterDelay(Renderer rend, Color originalColor, float delay)
    {
        yield return new WaitForSeconds(delay);
        rend.material.color = originalColor;
    }
}
