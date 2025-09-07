using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject questionPanel;
    public TextMeshProUGUI questionText;
    public GameObject answerButton;
    public GameObject textDebug;
    public GameObject resetButton;
    public Button languageButton;
    public TextMeshProUGUI languageButtonText;

    [Header("VFX")]
    public GameObject correctVFX;
    public GameObject wrongVFX;

    [Header("Note VFX")]
    public GameObject notePrefab;
    public Color[] instrumentColors;

    [System.Serializable]
    public class MusicAnswer
    {
        public string instrumentName;
        public string correctAnswer;
        public string correctAnswerEnglish;
        public AudioClip audioClip;
    }
    public MusicAnswer[] musicAnswers;
    private AudioClip currentAudio;

    [System.Serializable]
    public class OrchestraGroup
    {
        public string groupName;
        public string[] instrumentNames; // nama-nama instrumen anggota
        public GameObject orchestraPrefab;
    }
    [Header("Orchestra Groups")]
    public OrchestraGroup[] orchestraGroups;

    private string currentAnswer = "";
    private string currentAnswerEnglish = "";
    private string currentInstrument = "";
    private bool isEnglish = false;

    // Track orkestra yang sudah di-spawn
    private readonly List<GameObject> spawnedOrchestras = new List<GameObject>();
    private readonly HashSet<string> detectedQRCodes = new HashSet<string>(); // Tambahkan ini

    void Start()
    {
        questionPanel.SetActive(false);
        answerButton.SetActive(false);
        textDebug.SetActive(false);
        resetButton.SetActive(false);

        if (languageButtonText != null)
            languageButtonText.text = "Bahasa Indonesia";
    }

    // Dipanggil oleh ObjectSpawner setelah alat musik muncul
    public void ShowQuestion(string instrumentName)
    {
        // Cek jumlah QR aktif
        var spawner = FindFirstObjectByType<ObjectSpawner>();
        var activeQrs = spawner != null ? spawner.GetActiveInstrumentNames() : new List<string>();

        if (activeQrs.Count != 1)
        {
            // Lebih dari 1 QR, hide question system
            questionPanel.SetActive(false);
            answerButton.SetActive(false);
            textDebug.SetActive(false);
            resetButton.SetActive(true);
            return;
        }

        // Hanya 1 QR, tampilkan question system
        foreach (var ma in musicAnswers)
        {
            if (ma.instrumentName == instrumentName)
            {
                currentInstrument = instrumentName;
                currentAnswer = ma.correctAnswer.ToLower();
                currentAnswerEnglish = ma.correctAnswerEnglish.ToLower();
                currentAudio = ma.audioClip;
                questionPanel.SetActive(true);
                answerButton.SetActive(true);
                textDebug.SetActive(true);
                resetButton.SetActive(true);
                UpdateQuestionText();
                break;
            }
        }
    }

    private void UpdateQuestionText()
    {
        if (isEnglish)
            questionText.text = $"How to play {currentInstrument}?";
        else
            questionText.text = $"Bagaimana cara memainkan {currentInstrument}?";
    }

    // Dipanggil oleh VoiceManager
    public void CheckAnswer(string playerAnswer)
    {
        playerAnswer = playerAnswer.ToLower();

        bool isCorrect = isEnglish
            ? playerAnswer.Contains(currentAnswerEnglish)
            : playerAnswer.Contains(currentAnswer);

        if (isCorrect)
        {
            // Benar
            questionText.text = isEnglish ? "Correct!" : "Benar!";

            // Cari alat musik yang aktif di scene
            var spawner = FindFirstObjectByType<ObjectSpawner>();
            if (spawner != null)
            {
                GameObject instrument = spawner.GetSpawnedInstrument(currentInstrument);
                if (instrument != null)
                {
                    // 1. Ganti warna alat musik
                    var rend = instrument.GetComponentInChildren<Renderer>();
                    if (rend != null && instrumentColors.Length > 0)
                    {
                        Color color = instrumentColors[Random.Range(0, instrumentColors.Length)];
                        rend.material.color = color;
                    }

                    // 2. Spawn not musik mengelilingi alat musik
                    int noteCount = 8;
                    float radius = 0.25f;
                    for (int i = 0; i < noteCount; i++)
                    {
                        float angle = i * Mathf.PI * 2f / noteCount;
                        Vector3 offset = new Vector3(Mathf.Cos(angle), 0.5f, Mathf.Sin(angle)) * radius;
                        Vector3 spawnPos = instrument.transform.position + offset;
                        Instantiate(notePrefab, spawnPos, Quaternion.identity);
                    }
                }
            }

            // 🔊 Mainkan audio alat musik
            if (currentAudio != null)
            {
                AudioSource.PlayClipAtPoint(currentAudio, Camera.main.transform.position);
            }
        }
        else
        {
            // Salah
            Instantiate(wrongVFX, Vector3.zero, Quaternion.identity);
            questionText.text = isEnglish ? "Wrong, try again!" : "Salah, coba lagi!";
        }
    }

    // Dipanggil oleh ObjectSpawner setiap ada QR baru yang terbaca
    public void RegisterQRCode(string qrName)
    {
        if (!string.IsNullOrEmpty(qrName))
            detectedQRCodes.Add(qrName);

        CheckOrchestra();
    }

    public void CheckOrchestra()
    {
        var spawner = FindFirstObjectByType<ObjectSpawner>();
        if (spawner == null) return;

        foreach (var group in orchestraGroups)
        {
            // Jika semua instrumen dalam group sudah ada di detectedQRCodes
            if (group.instrumentNames.All(name => detectedQRCodes.Contains(name)))
            {
                // Hide semua instrumen di scene
                foreach (var name in group.instrumentNames)
                {
                    var go = spawner.GetSpawnedInstrument(name);
                    if (go != null) go.SetActive(false);
                }

                // Spawn orkestra jika belum ada
                if (!spawnedOrchestras.Any(o => o != null && o.name.StartsWith(group.orchestraPrefab.name)))
                {
                    // Posisi rata-rata instrumen
                    Vector3 avgPos = Vector3.zero;
                    int count = 0;
                    foreach (var name in group.instrumentNames)
                    {
                        var go = spawner.GetSpawnedInstrument(name);
                        if (go != null)
                        {
                            avgPos += go.transform.position;
                            count++;
                        }
                    }
                    if (count > 0)
                        avgPos /= count;
                    var ork = Instantiate(group.orchestraPrefab, avgPos, Quaternion.identity);
                    ork.name = group.orchestraPrefab.name + "_Instance";
                    spawnedOrchestras.Add(ork);

                    // Tampilkan panel orkestra
                    ShowOrchestraPanel(group.groupName);
                }
                else
                {
                    // Jika orkestra sudah ada, tetap tampilkan panel orkestra
                    ShowOrchestraPanel(group.groupName);
                }
            }
            else
            {
                // Jika group tidak lengkap, pastikan orkestra dihapus
                var ork = spawnedOrchestras.FirstOrDefault(o => o != null && o.name.StartsWith(group.orchestraPrefab.name));
                if (ork != null)
                {
                    Destroy(ork);
                    spawnedOrchestras.Remove(ork);
                }
                // Tampilkan kembali instrumen yang sempat di-hide
                foreach (var name in group.instrumentNames)
                {
                    var go = spawner.GetSpawnedInstrument(name);
                    if (go != null) go.SetActive(true);
                }
            }
        }
    }

    public void ResetScene(float delay = 1f)
    {
        // 1. Sembunyikan panel soal dan tombol
        questionPanel.SetActive(false);
        answerButton.SetActive(false);
        textDebug.SetActive(false);
        textDebug.GetComponent<TextMeshProUGUI>().text = "";

        // 2. Hapus semua objek AR yang di-spawn
        var spawner = FindFirstObjectByType<ObjectSpawner>();
        if (spawner != null)
        {
            spawner.ClearAllSpawnedObjects();
        }

        // 3. Hapus semua orkestra yang di-spawn
        foreach (var ork in spawnedOrchestras)
        {
            if (ork != null) Destroy(ork);
        }
        spawnedOrchestras.Clear();

        detectedQRCodes.Clear(); // Reset list QR code saat reset

        // 4. Mulai coroutine delay sebelum bisa spawn lagi
        StartCoroutine(EnableSpawningAfterDelay(delay));
    }

    private IEnumerator EnableSpawningAfterDelay(float delay)
    {
        var spawner = FindFirstObjectByType<ObjectSpawner>();
        if (spawner != null)
        {
            spawner.canSpawn = false;
            yield return new WaitForSeconds(delay);
            spawner.canSpawn = true;
        }
    }

    // Fungsi untuk tombol ganti bahasa
    public void ToggleLanguage()
    {
        isEnglish = !isEnglish;
        if (languageButtonText != null)
            languageButtonText.text = isEnglish ? "English" : "Bahasa Indonesia";
        UpdateQuestionText();
    }

    public void ShowOrchestraPanel(string orchestraName)
    {
        questionPanel.SetActive(true);
        answerButton.SetActive(false);
        textDebug.SetActive(false);
        resetButton.SetActive(true);
        questionText.text = isEnglish
            ? $"This is the {orchestraName} orchestra!"
            : $"Ini adalah orkestra {orchestraName}!";
    }
}
