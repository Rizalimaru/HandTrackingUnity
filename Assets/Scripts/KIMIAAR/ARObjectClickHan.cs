using UnityEngine;

public class ARObjectClickHan : MonoBehaviour
{
    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // // Cek kalau object punya komponen AtomBehaviour
                // AtomBehaviour atomBehaviour = hit.collider.GetComponent<AtomBehaviour>();
                // if (atomBehaviour != null)
                // {
                //     atomBehaviour.OnTapped();
                // }

                // Kalau masih mau sistem lama pakai AtomInteraction
                AtomInteraction atomInteraction = hit.collider.GetComponent<AtomInteraction>();
                if (atomInteraction != null)
                {
                    atomInteraction.OnClicked();
                }

                AtomInfo atomInfo = hit.collider.GetComponent<AtomInfo>();
                if (atomInfo != null)
                {
                    atomInfo.ShowInfo();
                }
                        }

                    }
                }
}
