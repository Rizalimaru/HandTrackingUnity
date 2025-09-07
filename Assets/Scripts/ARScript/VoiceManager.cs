using TMPro;
using UnityEngine;
using KKSpeech;
using UnityEngine.UI;
using UnityEngine.Android;

public class VoiceManager : MonoBehaviour
{
    public Button startButton;
    public TextMeshProUGUI resultText;

    void Start()
    {
        // 1. Cari listener yang ada di scene
        SpeechRecognizerListener listener = FindFirstObjectByType<SpeechRecognizerListener>();

        // 2. Daftarkan fungsi untuk menerima hasil
        listener.onFinalResults.AddListener(OnFinalResult);

        // 3. Tambahkan fungsi ke tombol
        startButton.onClick.AddListener(OnStartRecording);

        //Meminta izin penggunaan michrophone
        RequestMicrophonePermission();


    }

    // Fungsi yang dipanggil saat tombol ditekan
    public void OnStartRecording()
    {
        // Cek apakah rekaman sedang berjalan atau tidak
        if (SpeechRecognizer.IsRecording())
        {
            SpeechRecognizer.StopIfRecording();
        }
        else
        {
            resultText.text = "Mendengarkan...";
            // Mulai merekam suara
            SpeechRecognizer.StartRecording(true); // 'true' untuk mendapatkan hasil parsial
        }
    }

    // Fungsi ini akan dipanggil oleh listener saat ada hasil akhir
    public void OnFinalResult(string result)
    {
        resultText.text = result;
        FindFirstObjectByType<GameManager>().CheckAnswer(result);
    }

    public void RequestMicrophonePermission()
    {
#if PLATFORM_ANDROID
        // Cek apakah pengguna SUDAH memberikan izin sebelumnya
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            // Jika belum, panggil fungsi untuk MENAMPILKAN NOTIFIKASI izin
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif
    }

    
}
