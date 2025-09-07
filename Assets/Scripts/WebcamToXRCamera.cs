using UnityEngine;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(Camera))]
public class WebcamToXRCamera : MonoBehaviour
{
    public string webcamName = ""; // Kosong = default webcam
    private WebCamTexture webcamTexture;
    private Material camMaterial;

    void Start()
    {
        webcamTexture = string.IsNullOrEmpty(webcamName)
            ? new WebCamTexture()
            : new WebCamTexture(webcamName);

        webcamTexture.Play();

        camMaterial = new Material(Shader.Find("Unlit/Texture"));
        camMaterial.mainTexture = webcamTexture;

        var cam = GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.targetTexture = null; // Pastikan render ke layar

        // Tambahkan baris ini:
        cam.cullingMask = 0; // Tidak merender objek apapun di scene
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
            Graphics.Blit(webcamTexture, dest, camMaterial);
        else
            Graphics.Blit(src, dest);
    }

    void OnDestroy()
    {
        if (webcamTexture != null)
            webcamTexture.Stop();
    }
}