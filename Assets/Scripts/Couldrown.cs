using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Untuk TextMeshPro

public class Cauldron : MonoBehaviour
{
    private List<string> elementSymbols = new List<string>();
    private List<GameObject> elementObjects = new List<GameObject>();

    [Header("Database & Spawn")]
    public CompoundDatabase database;
    public Transform spawnPoint; // untuk efek di cauldron
    public Transform[] spawnPoints; // titik spawn elemen
    public ElementPrefabMapping[] elementPrefabs; // simbol -> prefab

    [Header("Effect")]
    public GameObject explosionVFX;
    public GameObject wrongVFX;

    [Header("Quiz Settings")]
    public int currentQuestionIndex = 0;
    private CompoundInfo currentQuestion; // target senyawa
    private float questionStartTime;
    public float hintDelay = 10f; // waktu sebelum hint muncul

    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI hintText;

    void Start()
    {
        PickNextQuestion();
    }

    void Update()
    {
        // Cek apakah sudah waktunya menampilkan hint
        if (currentQuestion != null && Time.time - questionStartTime >= hintDelay)
        {
            ShowHint();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Element"))
        {
            ElementData data = other.GetComponent<ElementData>();
            if (data != null)
            {
                elementSymbols.Add(data.symbol);
                elementObjects.Add(other.gameObject);

                Debug.Log($"Elemen masuk ke cauldron: {data.symbol}");
                Debug.Log($"Isi cauldron saat ini: {string.Join(", ", elementSymbols)}");

                TryReact();
            }
            else
            {
                Debug.LogWarning("Object bertag 'Element' tidak memiliki komponen ElementData!");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Element"))
        {
            ElementData data = other.GetComponent<ElementData>();
            if (data != null)
            {
                elementSymbols.Remove(data.symbol);
                elementObjects.Remove(other.gameObject);
                Debug.Log($"Elemen keluar dari cauldron: {data.symbol}");
                Debug.Log($"Isi cauldron saat ini: {string.Join(", ", elementSymbols)}");
            }
        }
    }

    void TryReact()
    {
        // Cek apakah campuran cocok dengan target soal
        CompoundInfo compound = database.GetCompoundFromElements(elementSymbols);
        if (compound != null)
        {
            if (compound == currentQuestion)
            {
                Debug.Log("Jawaban Benar!");
                StartCoroutine(HandleCorrectAnswer(compound));
            }
            else
            {
                Debug.Log("Jawaban Salah!");
                StartCoroutine(HandleWrongAnswer());
            }
        }
    }

    IEnumerator HandleCorrectAnswer(CompoundInfo compound)
    {
        foreach (GameObject obj in elementObjects)
        {
            StartCoroutine(delayDestroy(obj));
        }

        elementSymbols.Clear();
        elementObjects.Clear();

        yield return new WaitForSeconds(1f);

        if (explosionVFX != null)
            Instantiate(explosionVFX, spawnPoint.position, Quaternion.identity);

        Instantiate(compound.prefabResult, spawnPoint.position, Quaternion.identity);

        yield return new WaitForSeconds(2f);
        PickNextQuestion();
    }

    IEnumerator HandleWrongAnswer()
    {
        if (wrongVFX != null)
            Instantiate(wrongVFX, spawnPoint.position, Quaternion.identity);

        foreach (GameObject obj in elementObjects)
        {
            StartCoroutine(delayDestroy(obj));
        }

        elementSymbols.Clear();
        elementObjects.Clear();

        yield return null;
    }

    IEnumerator delayDestroy(GameObject obj)
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(obj);
    }

    void PickNextQuestion()
    {
        if (database == null || database.compounds == null || database.compounds.Count == 0)
        {
            Debug.LogWarning("Database compound belum di-set atau kosong!");
            currentQuestion = null;
            if (questionText != null) questionText.text = "Tidak ada soal";
            return;
        }

        currentQuestionIndex = Random.Range(0, database.compounds.Count);
        currentQuestion = database.compounds[currentQuestionIndex];

        if (questionText != null)
            questionText.text = $"Buatlah: {currentQuestion.nameCompound} ({currentQuestion.formula})";

        if (hintText != null)
            hintText.text = "";

        questionStartTime = Time.time;

        // Spawn elemen untuk soal ini
        SpawnRequiredElements(currentQuestion.requiredElements);
    }

    void ShowHint()
    {
        if (currentQuestion == null) return;

        if (hintText != null && string.IsNullOrEmpty(hintText.text))
        {
            hintText.text = $"Hint: Campurkan {string.Join(" + ", currentQuestion.requiredElements)}";
            Debug.Log("Hint diberikan ke pemain");
        }
    }

    void SpawnRequiredElements(List<string> elements)
    {
        // Hapus semua sisa elemen lama di scene
        foreach (var obj in GameObject.FindGameObjectsWithTag("Element"))
        {
            Destroy(obj);
        }

        // Spawn di titik yang sudah ditentukan
        for (int i = 0; i < elements.Count && i < spawnPoints.Length; i++)
        {
            GameObject prefab = GetElementPrefab(elements[i]);
            if (prefab != null)
            {
                Instantiate(prefab, spawnPoints[i].position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning($"Prefab untuk elemen {elements[i]} tidak ditemukan!");
            }
        }
    }

    GameObject GetElementPrefab(string symbol)
    {
        foreach (var mapping in elementPrefabs)
        {
            if (mapping.symbol == symbol)
                return mapping.prefab;
        }
        return null;
    }
}

[System.Serializable]
public class ElementPrefabMapping
{
    public string symbol;
    public GameObject prefab;
}
