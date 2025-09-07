using UnityEngine;

public class MicCheck : MonoBehaviour
{
    void Start()
    {
        // List semua mic
        foreach (var device in Microphone.devices)
        {
            Debug.Log("Mic ditemukan: " + device);
        }

        // Cek mic default aktif atau tidak
        bool isRecording = Microphone.IsRecording(null);
        Debug.Log("Mic default aktif? " + isRecording);

        // Mulai rekam mic default
        AudioClip clip = Microphone.Start(null, false, 3, 44100);
        Debug.Log("Mulai merekam...");

        // Stop setelah 3 detik
        Invoke("StopMic", 3f);
    }

    void StopMic()
    {
        Microphone.End(null);
        Debug.Log("Mic dihentikan.");
    }
}
