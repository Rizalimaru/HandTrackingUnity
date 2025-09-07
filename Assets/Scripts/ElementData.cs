using UnityEngine;
using TMPro;

public class ElementData : MonoBehaviour
{
    public string symbol;
    public string fullName;
    public string description;

    public TextMeshProUGUI symbolText;
    public bool isGrounded;
    public GameObject vfxLight;

    //public Transform targetTransform; // ← Objek yang ingin dijadikan target tampilan

    void Start()
    {
        if (symbolText != null)
            symbolText.text = symbol;


        if (vfxLight != null)
            vfxLight.SetActive(false);
    }

    // void Update()
    // {
    //     if (desckripsiPanel != null && targetTransform != null && desckripsiPanel.activeSelf)
    //     {
    //         Vector3 direction = targetTransform.position - desckripsiPanel.transform.position;
    //         Quaternion lookRotation = Quaternion.LookRotation(direction);

    //         // Tambahkan rotasi 180 derajat di sumbu Y
    //         lookRotation *= Quaternion.Euler(0, 180f, 0);

    //         desckripsiPanel.transform.rotation = lookRotation;
    //     }
    // }


    public override string ToString()
    {
        return $"{fullName} ({symbol})";
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
