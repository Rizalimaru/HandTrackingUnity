using UnityEngine;

public class ProvinceTouch : MonoBehaviour
{
    public Color touchColor = Color.red; // Warna saat disentuh
    private Color originalColor;
    public GameObject pointer;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
        pointer.SetActive(false);
    }

    public void OnTouch() // 🔹 Ganti jadi public
    {
        rend.material.color = touchColor;
        pointer.SetActive(true);
        pointer.transform.position = new Vector3(transform.position.x, pointer.transform.position.y, transform.position.z);
    }

    public void OnRelease() // 🔹 Ganti jadi public
    {
        rend.material.color = originalColor;
    }
}
