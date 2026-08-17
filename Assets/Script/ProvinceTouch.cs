using UnityEngine;

public class ProvinceTouch : MonoBehaviour
{
    public Color touchColor = Color.red;
    private Color originalColor;
    public GameObject pointer;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
        pointer.SetActive(false);
    }

    public void OnTouch()
    {
        rend.material.color = touchColor;
        pointer.SetActive(true);
        pointer.transform.position = new Vector3(transform.position.x, pointer.transform.position.y, transform.position.z);
    }

    public void OnRelease()
    {
        rend.material.color = originalColor;
    }
}
