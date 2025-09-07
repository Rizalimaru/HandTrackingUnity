using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionScript : MonoBehaviour
{
    public HandTracking handTracking;
    public string handType; // "Left" atau "Right"
    public Transform handTransform;
    public TextMeshProUGUI descriptionText;
    public GameObject descriptionPanel;

    private GameObject grabbedObject = null;

    private string originalTag = "";

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Element") && grabbedObject == null)
        {
            // Ambil komponen ElementData
            ElementData elementData = other.GetComponent<ElementData>();

            // Pastikan elementData tidak null dan sudah grounded
            if (elementData != null && elementData.isGrounded)
            {
                if (IsFist())
                {
                    grabbedObject = other.gameObject;

                    Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.freezeRotation = true; // Mencegah rotasi objek
                    }

                    elementData.vfxLight.SetActive(true);
                    descriptionText.text = grabbedObject.GetComponent<ElementData>().description;
                    descriptionPanel.SetActive(true);
                    //elementData.desckripsiPanel.SetActive(true);
                    Debug.Log($"{handType} hand grabbed: {grabbedObject.name}");
                }
            }
        }
    }


    void Update()
    {
        if (grabbedObject != null)
        {
            if (IsFist())
            {
                Vector3 handPos = handTransform.position;
                Vector3 objPos = grabbedObject.transform.position;
                grabbedObject.transform.position = new Vector3(handPos.x, handPos.y, handPos.z - 1f);
            }
            else
            {
                Debug.Log($"{handType} hand released: {grabbedObject.name}");
                grabbedObject.GetComponent<ElementData>().vfxLight.SetActive(false);;
                descriptionPanel.SetActive(false);
                //grabbedObject.GetComponent<ElementData>().desckripsiPanel.SetActive(false);

                // Kembalikan rotasi Rigidbody jika ada
                Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                if (rb != null) rb.freezeRotation = false;

                grabbedObject = null;
            }
        }
    }

    bool IsFist()
    {
        if (handTracking == null) return false;

        if (handType == "Left")
        {
            Vector3[] landmarks = handTracking.GetLeftLandmarks();
            return handTracking.GetHandPose(landmarks) == "Fist";
        }
        else if (handType == "Right")
        {
            Vector3[] landmarks = handTracking.GetRightLandmarks();
            return handTracking.GetHandPose(landmarks) == "Fist";
        }

        return false;
    }
    
}
