using UnityEngine;
using System.Collections;
using UnityEngine.XR.ARFoundation;

public class AutoResetAR : MonoBehaviour
{
    private void OnEnable()
    {
        // Pastikan ARSession direset setiap kali scene aktif
        var session = GetComponent<ARSession>();
        if (session != null)
        {
            session.Reset();
            Debug.Log("AR Session has been reset.");
        }
    }
}
