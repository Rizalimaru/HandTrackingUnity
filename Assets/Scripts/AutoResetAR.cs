using UnityEngine;
using System.Collections;
using UnityEngine.XR.ARFoundation;

public class AutoResetAR : MonoBehaviour
{

    public GameObject arSessioin;
    IEnumerator Start()
    {
        if (arSessioin != null)
        {
            yield return null;
            arSessioin.SetActive(false);
            yield return new WaitForSeconds(0.5f);
            arSessioin.SetActive(true);
        }
    }

}

