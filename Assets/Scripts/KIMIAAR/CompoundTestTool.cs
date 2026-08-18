using UnityEngine;
using TMPro; // Gunakan TextMeshPro untuk Dropdown
using System.Collections.Generic;

public class CompoundTestTool : MonoBehaviour
{
    [Header("Referensi Sistem")]
    [Tooltip("Drag script AtomInitializer dari Hierarchy ke sini")]
    public AtomInitializer atomInitializer;

    [Header("Referensi UI")]
    [Tooltip("Drag Dropdown TextMeshPro dari Canvas ke sini")]
    public TMP_Dropdown compoundDropdown;

    // --- GLOBAL STATE ---
    // Menyimpan pilihan agar tetap ingat meskipun wadah (AR) belum di-scan atau hilang
    public static bool GlobalIsTestMode = false;
    public static string GlobalForcedCompound = "";

    void Start()
    {
        if (atomInitializer == null)
        {
            atomInitializer = Object.FindFirstObjectByType<AtomInitializer>();
        }

        if (atomInitializer != null && compoundDropdown != null)
        {
            SetupDropdown();
        }
        else
        {
            Debug.LogWarning("[TestTool] AtomInitializer atau Dropdown belum di-assign!");
        }
    }

    /// <summary>
    /// Mengambil daftar senyawa dari AtomInitializer dan memasukkannya ke Dropdown
    /// </summary>
    private void SetupDropdown()
    {
        // 1. Bersihkan dropdown
        compoundDropdown.ClearOptions();

        // 2. Ambil daftar dari script utama
        string[] compounds = atomInitializer.PossibleCompounds;

        // 3. Masukkan opsi "Acak (Random)" sebagai opsi pertama
        List<string> options = new List<string> { "-- ACAK (Normal Mode) --" };
        options.AddRange(compounds);

        // 4. Masukkan ke UI
        compoundDropdown.AddOptions(options);

        // 5. Tambahkan Listener agar setiap ganti opsi langsung dieksekusi (opsional)
        // compoundDropdown.onValueChanged.AddListener(delegate { ApplyTestCompound(); });
    }

    /// <summary>
    /// Fungsi ini dipanggil saat tombol "Terapkan" ditekan
    /// </summary>
    public void ApplyTestCompound()
    {
        // Cari SEMUA AtomInitializer yang ada di scene
        // (Berjaga-jaga jika ada script ganda di Scene dan di dalam Prefab Wadah)
        AtomInitializer[] allInitializers = Object.FindObjectsByType<AtomInitializer>(FindObjectsSortMode.None);

        int selectedIndex = compoundDropdown.value;

        if (selectedIndex == 0)
        {
            GlobalIsTestMode = false;
            GlobalForcedCompound = "";
            Debug.Log("[DevMode] Mode Acak (Core Game) dipilih.");
        }
        else
        {
            GlobalIsTestMode = true;
            // Gunakan teks opsi dropdown secara langsung agar sinkron
            GlobalForcedCompound = compoundDropdown.options[selectedIndex].text;
            Debug.Log($"[DevMode] Mengunci soal untuk testing: {GlobalForcedCompound}");
        }

        // Jika ada wadah/script yang sudah aktif, langsung reset semuanya
        if (allInitializers.Length > 0)
        {
            foreach (var initializer in allInitializers)
            {
                initializer.isTestMode = GlobalIsTestMode;
                initializer.forcedCompound = GlobalForcedCompound;
                initializer.ResetGame();
            }
        }
        else
        {
            Debug.LogWarning("[DevMode] Wadah belum di-scan. Setingan ini akan otomatis diterapkan saat wadah muncul.");
        }
    }
}
