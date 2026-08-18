using UnityEngine;
using System.Collections.Generic; // Dibutuhkan untuk Dictionary dan List
using System.Linq; // Dibutuhkan untuk .ToList()
using System.Collections;
using TMPro;

public class AtomInitializer : MonoBehaviour
{
    [Header("Data Soal")]
    [SerializeField]
    private string[] possibleCompounds = {
    "NaCl", // Natrium Klorida
    "CO2", // Karbondioksida
    "CaO",
    "O2",
    "MgO",
    "KCl",
    "SO2",   
    "LiCl",  // Lithium chloride
    "NaF",   // Sodium fluoride
    "NaBr",  // Sodium bromide
    "KF",    // Potassium fluoride
    "KBr",   // Potassium bromide
    "RbCl",  // Rubidium chloride
    "CsI",   // Cesium iodide
    "CaF2",  // Calcium fluoride
    "MgCl2", // Magnesium chloride
    "BeO",   // Beryllium oxide
    "SrCl2", // Strontium chloride
    "BaO",   // Barium oxide
    "AlF3",  // Aluminium fluoride
    "BF3",   // Boron trifluoride
    "SiO2",  // Silicon dioxide
    "N2O5",  // Dinitrogen pentoxide
    "PCl3",  // Phosphorus trichloride
    "SF6",   // Sulfur hexafluoride
    "XeF2" };   // Xenon difluoride

    // Properti publik agar script UI Testing bisa membaca daftar soal
    public string[] PossibleCompounds => possibleCompounds;

    public string targetCompound;

    [Header("Testing Mode")]
    [Tooltip("Centang untuk memaksa memunculkan compound tertentu dari Dropdown Test")]
    public bool isTestMode = false;
    [Tooltip("Nama senyawa yang akan dipaksa muncul jika Test Mode aktif")]
    public string forcedCompound = "";

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

    // --- Variabel Hint ---
    private float idleTimer = 0f;
    private bool isHinting = false;

    private List<AtomInteraction> correctAtomsClicked = new List<AtomInteraction>();

    void Awake()
    {
        foreach (var item in atomLibrary)
        {
            if (item != null && !string.IsNullOrEmpty(item.atomName) && item.atomPrefab != null)
            {
                string cleanName = item.atomName.Trim(); // Cegah error karena salah ketik spasi ("Cl ")
                if (!atomPrefabDict.ContainsKey(cleanName))
                {
                    atomPrefabDict.Add(cleanName, item.atomPrefab);
                }
            }
        }
    }

    void Start()
    {
        StartNewRound();
    }

    public void StartNewRound()
    {
        StopAllHints();

        // 1. Singkronisasi dengan pilihan UI (sekalipun dipilih saat wadah belum muncul)
        isTestMode = CompoundTestTool.GlobalIsTestMode;
        forcedCompound = CompoundTestTool.GlobalForcedCompound;

        // LOGIKA TEST MODE
        if (isTestMode && !string.IsNullOrEmpty(forcedCompound))
        {
            targetCompound = forcedCompound;
            Debug.Log($"[TEST MODE] Memaksa soal: {targetCompound}");
        }
        else
        {
            targetCompound = possibleCompounds[Random.Range(0, possibleCompounds.Length)];
            Debug.Log($"Soal ronde ini adalah: {targetCompound}");
        }

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

    void Update()
    {
        if (string.IsNullOrEmpty(targetCompound)) return;

        string[] compoundAtoms = ParseCompound(targetCompound);
        if (nextAtomIndex >= compoundAtoms.Length) return;

        idleTimer += Time.deltaTime;

        if (idleTimer >= 5f && !isHinting)
        {
            TriggerHintForNextAtom(compoundAtoms);
        }
    }

    private void TriggerHintForNextAtom(string[] compoundAtoms)
    {
        string targetAtomName = compoundAtoms[nextAtomIndex];
        AtomInteraction[] allAtoms = Object.FindObjectsByType<AtomInteraction>(FindObjectsSortMode.None);
        
        foreach (var atom in allAtoms)
        {
            if (atom.atomName == targetAtomName && !correctAtomsClicked.Contains(atom))
            {
                atom.TriggerHint();
                isHinting = true;
                break; // Cukup 1 atom yang di-hint
            }
        }
    }

    private void StopAllHints()
    {
        idleTimer = 0f;
        isHinting = false;
        AtomInteraction[] allAtoms = Object.FindObjectsByType<AtomInteraction>(FindObjectsSortMode.None);
        foreach (var atom in allAtoms)
        {
            if (atom != null) atom.StopHint();
        }
    }
public bool TryClickAtom(AtomInteraction clickedAtom)
{
    StopAllHints(); // Hentikan hint jika ada interaksi

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
        UIManager.Instance.ShowSalah();
    
        AudioKimia.Instance.PlaySFX(0);
        return false;
}




    // 🔄 Ganti RestartScene dengan ResetGame
    public void ResetGame()
    {
        StopAllHints();
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
        AudioKimia.Instance.PlaySFX(2);
        yield return new WaitForSeconds(2f);
        AudioKimia.Instance.PlaySFX(2);
        yield return new WaitForSeconds(2f);
        
        UIManager.Instance.SetHasilText(resultText);
        AudioKimia.Instance.PlaySFX(3);
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

    // 📦 Buat daftar atom untuk di-spawn berdasarkan komponen aslinya
    List<string> atomsToSpawnNames = new List<string>();
    
    // 1. Masukkan semua atom wajib pembentuk senyawa (maksimal sebanyak slot spawn)
    for (int i = 0; i < compoundAtoms.Length; i++)
    {
        if (i < spawnPoints.Length)
        {
            atomsToSpawnNames.Add(compoundAtoms[i]);
        }
    }

    // 2. Penuhi sisa slot dengan distractor jika ada sisa tempat (misal NaCl butuh 2, ada 3 slot, maka 1 distractor)
    int attempts = 0;
    while (atomsToSpawnNames.Count < spawnPoints.Length && attempts < 50)
    {
        attempts++;
        string distractorAtomName = allAtomNames[Random.Range(0, allAtomNames.Count)];
        
        bool isInvalid = false;
        if (targetCompound != "O2")
        {
            foreach (string atom in compoundAtoms)
            {
                if (distractorAtomName == atom || 
                    validPairs.Contains(atom + distractorAtomName) || 
                    validPairs.Contains(distractorAtomName + atom))
                {
                    isInvalid = true;
                    break;
                }
            }
        }

        if (!isInvalid)
        {
            atomsToSpawnNames.Add(distractorAtomName);
        }
    }

    // 🔀 Acak urutan agar tidak selalu di posisi yang sama
    atomsToSpawnNames = atomsToSpawnNames.OrderBy(x => Random.value).ToList();

    // 🚀 Bersihkan sisa atom lama (memastikan benar-benar bersih sebelum spawn baru)
    ClearSpawnPoints();

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


        if (compound == "CaF2") return new string[] { "Ca", "F" };
        if (compound == "MgCl2")return new string[] { "Mg", "Cl" };
        if (compound == "SrCl2")return new string[] { "Sr", "Cl" };
        if (compound == "AlF3") return new string[] { "Al", "F" };
        if (compound == "BF3")  return new string[] { "B",  "F" };
        if (compound == "SiO2") return new string[] { "Si", "O" };
        if (compound == "N2O5") return new string[] { "N",  "O" };
        if (compound == "PCl3") return new string[] { "P",  "Cl" };
        if (compound == "SF6")  return new string[] { "S",  "F" };

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
