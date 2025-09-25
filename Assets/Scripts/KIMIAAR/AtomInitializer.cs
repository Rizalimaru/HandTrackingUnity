using UnityEngine;
using System.Collections.Generic; // Dibutuhkan untuk Dictionary dan List
using System.Linq; // Dibutuhkan untuk .ToList()
using System.Collections;
using TMPro;

public class AtomInitializer : MonoBehaviour
{
    [Header("Data Soal")]
    private string[] possibleCompounds = { "NaCl", "CO2", "CaO", "O2", "MgO", "KCl", "SO2" };
    public string targetCompound;

    [Header("Pengaturan Prefab dan Spawn")]
    public AtomPrefabMap[] atomLibrary;
    public Transform[] spawnPoints;
    

    private Dictionary<string, GameObject> atomPrefabDict = new Dictionary<string, GameObject>();

    [System.Serializable]
    public class AtomPrefabMap
    {
        public string atomName;
        public GameObject atomPrefab;
    }

    private int nextAtomIndex = 0; // atom target ke berapa yang harus diklik
    public GameObject compoundTextParent;


    private List<AtomInteraction> correctAtomsClicked = new List<AtomInteraction>();

    void Awake()
    {
        foreach (var item in atomLibrary)
        {
            if (item.atomPrefab != null && !atomPrefabDict.ContainsKey(item.atomName))
            {
                atomPrefabDict.Add(item.atomName, item.atomPrefab);
            }
        }
    }

    void Start()
    {
        StartNewRound();
    }

    public void StartNewRound()
    {
        targetCompound = possibleCompounds[Random.Range(0, possibleCompounds.Length)];
        Debug.Log($"Soal ronde ini adalah: {targetCompound}");

        // Tampilkan soal di UIManager
        if (UIManager.Instance != null)
            UIManager.Instance.ShowSoal(targetCompound);

        if (compoundTextParent != null)
        {
            compoundTextParent.SetActive(true);
            TextMeshPro tmp = compoundTextParent.GetComponentInChildren<TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = $"Buat senyawa: {targetCompound}";
            }
        }

        correctAtomsClicked.Clear();
        nextAtomIndex = 0; // reset progress urutan klik

        InitializeAtoms();
    }


    private void ClearSpawnPoints()
    {
        foreach (Transform point in spawnPoints)
        {
            for (int i = point.childCount - 1; i >= 0; i--)
            {
                Destroy(point.GetChild(i).gameObject);
            }
        }
    }
public bool TryClickAtom(AtomInteraction clickedAtom)
{
    string[] compoundAtoms = ParseCompound(targetCompound);

    // 🔒 Cegah akses index di luar panjang array
    if (nextAtomIndex >= compoundAtoms.Length)
    {
        Debug.LogWarning("Semua atom target sudah dipilih, tapi ada klik tambahan.");
        return false;
    }

    // Cek apakah klik sesuai urutan
    if (clickedAtom.atomName == compoundAtoms[nextAtomIndex])
    {
        correctAtomsClicked.Add(clickedAtom);
        nextAtomIndex++;

        // Kalau sudah semua benar
        if (nextAtomIndex >= compoundAtoms.Length)
        {
            StartCoroutine(TungguBro());
            Debug.Log("Semua atom benar diklik sesuai urutan!");
        }
        return true; // valid
    }

    // Kalau urutan salah
    //UIManager.Instance.ShowSalah();
    return false;
}




    // 🔄 Ganti RestartScene dengan ResetGame
    public void ResetGame()
    {
        correctAtomsClicked.Clear();

        UIManager.Instance.HideBenar();

        foreach (var atom in GameObject.FindGameObjectsWithTag("Atom"))
        {
            Destroy(atom);
        }

        // Hapus semua atom yang ada di spawn poin
        ClearSpawnPoints();

        // Mulai ronde baru
        StartNewRound();
    }

    public void OnCorrectAtomClicked(AtomInteraction clickedAtomScript)
    {
        if (!correctAtomsClicked.Contains(clickedAtomScript))
        {
            correctAtomsClicked.Add(clickedAtomScript);
        }

        if (correctAtomsClicked.Count == 2)
        {
            StartCoroutine(TungguBro());
            Debug.Log("Dua atom benar telah digabungkan!");
        }
    }

    private IEnumerator TungguBro()
    {
        string[] reactants = ParseCompound(targetCompound);
        string resultText = $"{reactants[0]} + {reactants[1]} → {targetCompound}";
        yield return new WaitForSeconds(4f);
        UIManager.Instance.SetHasilText(resultText);
        yield return new WaitForSeconds(2f);

        // 🔄 Ganti dari RestartScene() ke ResetGame()
        ResetGame();
    }

void InitializeAtoms()
{
    if (spawnPoints.Length < 3)
    {
        Debug.LogError("Anda harus menyediakan setidaknya 3 spawn point!");
        return;
    }

    ClearSpawnPoints();

    string[] compoundAtoms = ParseCompound(targetCompound);
    List<string> allAtomNames = atomLibrary.Select(a => a.atomName).ToList();

    // 🔒 Kumpulkan semua pasangan atom dari daftar soal (untuk deteksi senyawa valid)
    HashSet<string> validPairs = new HashSet<string>();
    foreach (string comp in possibleCompounds)
    {
        string[] atoms = ParseCompound(comp);
        for (int i = 0; i < atoms.Length; i++)
        {
            for (int j = i + 1; j < atoms.Length; j++)
            {
                validPairs.Add(atoms[i] + atoms[j]);
                validPairs.Add(atoms[j] + atoms[i]);
            }
        }
    }

    // 🎯 Pilih distractor yang aman (tidak bikin senyawa lain)
    string distractorAtomName;
    do
    {
        distractorAtomName = allAtomNames[Random.Range(0, allAtomNames.Count)];
    }
    while (
        (targetCompound != "O2") && ( // Kecuali kasus O2
            distractorAtomName == compoundAtoms[0] ||
            distractorAtomName == compoundAtoms[1] ||
            validPairs.Contains(compoundAtoms[0] + distractorAtomName) ||
            validPairs.Contains(distractorAtomName + compoundAtoms[0]) ||
            validPairs.Contains(compoundAtoms[1] + distractorAtomName) ||
            validPairs.Contains(distractorAtomName + compoundAtoms[1])
        )
    );

    // 📦 Buat daftar atom untuk di-spawn
    List<string> atomsToSpawnNames = new List<string>
    {
        compoundAtoms[0],
        compoundAtoms[1],
        distractorAtomName
    };

    // 🔀 Acak urutan
    atomsToSpawnNames = atomsToSpawnNames.OrderBy(x => Random.value).ToList();

    // 🚀 Spawn di scene
    for (int i = 0; i < spawnPoints.Length; i++)
    {
        string atomName = atomsToSpawnNames[i];
        if (atomPrefabDict.ContainsKey(atomName))
        {
            GameObject prefabToSpawn = atomPrefabDict[atomName];
            GameObject newAtomInstance = Instantiate(prefabToSpawn, spawnPoints[i].position, spawnPoints[i].rotation);
            newAtomInstance.transform.SetParent(spawnPoints[i]);

            AtomInteraction atomScript = newAtomInstance.GetComponent<AtomInteraction>();
            if (atomScript != null)
            {
                atomScript.atomName = atomName;
                atomScript.group = GetGroup(atomName);
                atomScript.targetCompound = this.targetCompound;
                Debug.Log($"Menciptakan atom {atomName} di spawn point {i}.");
            }
        }
        else
        {
            Debug.LogWarning($"Nama atom '{atomName}' tidak ditemukan di library!");
        }
    }
}


    string[] ParseCompound(string compound)
    {
        if (compound == "K2O") return new string[] { "K", "O" };
        if (compound == "CaCl2") return new string[] { "Ca", "Cl" };
        if (compound == "O2") return new string[] { "O", "O" };
        if (compound == "XeF2") return new string[] { "Xe", "F" };
        if (compound == "SO2") return new string[] { "S", "O" };
        if (compound == "CO2") return new string[] { "C", "O" };

        List<string> atoms = new List<string>();
        string currentAtom = "";

        foreach (char c in compound)
        {
            if (char.IsUpper(c))
            {
                if (!string.IsNullOrEmpty(currentAtom))
                {
                    atoms.Add(currentAtom);
                }
                currentAtom = c.ToString();
            }
            else if (char.IsLower(c))
            {
                currentAtom += c;
            }
            else if (char.IsDigit(c))
            {
                continue;
            }
        }

        if (!string.IsNullOrEmpty(currentAtom))
        {
            atoms.Add(currentAtom);
        }

        return atoms.ToArray();
    }

    string GetGroup(string atomName)
    {
        switch (atomName)
        {
            case "Na": return "1A";
            case "Cl": return "7A";
            case "K": return "1A";
            case "O": return "6A";
            case "C": return "4A";
            case "N": return "5A";
            case "S": return "6A";
            case "H": return "1A";
            case "Xe": return "8A";
            case "F": return "7A";
            case "Ca": return "2A";
            case "Mg": return "2A";
            default: return "Unknown";
        }
    }
}
