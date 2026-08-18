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
        if (atomInitializer == null) return;

        int selectedIndex = compoundDropdown.value;

        // Jika opsi pertama dipilih, matikan Test Mode (kembali acak)
        if (selectedIndex == 0)
        {
            atomInitializer.isTestMode = false;
            atomInitializer.forcedCompound = "";
            Debug.Log("[TestTool] Kembali ke mode Acak (Random)");
        }
        else
        {
            // Jika memilih senyawa, nyalakan Test Mode
            atomInitializer.isTestMode = true;
            // Index dikurangi 1 karena opsi pertama adalah "-- ACAK --"
            atomInitializer.forcedCompound = atomInitializer.PossibleCompounds[selectedIndex - 1];
            Debug.Log($"[TestTool] Mengunci senyawa ke: {atomInitializer.forcedCompound}");
        }

        // Paksa reset soal agar senyawa baru langsung muncul di meja
        atomInitializer.ResetGame();
    }
}
