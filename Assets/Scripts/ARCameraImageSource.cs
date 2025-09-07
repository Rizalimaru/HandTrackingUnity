using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARCameraImageSource : MonoBehaviour
{
    [SerializeField] ARCameraBackground arCameraBackground;

    public Texture AsTexture => arCameraBackground.material.mainTexture;
}