// ARRestartController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARSession))]
public class ARRestartController : MonoBehaviour
{
    [Tooltip("Delay sebelum restart (detik). Naikkan kalau perlu)")]
    public float delayBeforeRestart = 0.4f;

    ARSession arSession;
    ARTrackedImageManager trackedImageManager;

    IEnumerator Start()
    {
        arSession = GetComponent<ARSession>();
        trackedImageManager = Object.FindAnyObjectByType<ARTrackedImageManager>();

        // Disable trackedImageManager dulu supaya tidak memproses event saat reset
        if (trackedImageManager != null) trackedImageManager.enabled = false;

        // Tunggu sedikit supaya scene sebelumnya benar2 unload
        yield return new WaitForSeconds(delayBeforeRestart);

        // Stop (disable) session untuk menghentikan subsystems
        if (arSession != null) arSession.enabled = false;

        // Tunggu 1 frame agar subsystems benar-benar berhenti
        yield return null;

        // Pastikan availability + install jika perlu
        yield return ARSession.CheckAvailability();
        if (ARSession.state == ARSessionState.NeedsInstall)
            yield return ARSession.Install();

        // Reset dan start kembali
        arSession.Reset();
        arSession.enabled = true;

        // Tunggu 1 frame lalu enable trackedImageManager kembali
        yield return null;
        if (trackedImageManager != null) trackedImageManager.enabled = true;

        Debug.Log("[ARRestartController] AR restarted and trackedImageManager re-enabled");
    }
}
