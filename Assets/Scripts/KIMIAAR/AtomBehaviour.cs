using UnityEngine;

public class AtomBehaviour : MonoBehaviour
{
    [Header("Pengaturan Rotasi")]
    [SerializeField] private float rotationSpeed = 50f; // derajat per detik


    private bool isRotating = true;

    [Header("Offset Rotasi")]
    [SerializeField] private Vector3 rotationPivotOffset = new Vector3(0, -0.5f, 0); // Offset ke tengah bawah

    void Update()
    {
        if (isRotating)
        {
            RotateAroundPivot();
        }
    }

    private void RotateAroundPivot()
    {
        // Hitung posisi pivot
        Vector3 pivot = transform.position + rotationPivotOffset;

        // Rotasi di sekitar pivot
        transform.RotateAround(pivot, Vector3.up, rotationSpeed * Time.deltaTime);
    }
}