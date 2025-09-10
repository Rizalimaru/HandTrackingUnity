using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

// --- Struktur data untuk mencocokkan JSON ---
[System.Serializable]
public class Instrument
{
    public string nama_alat;
    public string deskripsi;
    public string gambar;
}

[System.Serializable]
public class ProvinceData
{
    public string id_provinsi;
    public string nama_provinsi;
    public List<Instrument> daftar_alat;
}

[System.Serializable]
public class ProvinceCollection
{
    public List<ProvinceData> provinsi;
}
// ---------------------------------------------

public class InfoManager : MonoBehaviour
{
    // Referensi ke skrip lain
    public TouchManager touchManager;

    [Header("Panel Daftar Alat Musik")]
    public GameObject panelDaftarAlatMusik;
    public Transform contentArea; // Hubungkan ke objek 'Content' di dalam Scroll View
    public GameObject itemAlatMusikPrefab; // Hubungkan Prefab yang sudah dibuat

    [Header("Panel Detail Alat Musik")]
    public GameObject panelDetail;
    public TextMeshProUGUI teksJudulDetail;
    public TextMeshProUGUI teksDeskripsiDetail;
    public Image gambarDetail;
    public Button tombolTutupDetail;

    // Variabel internal untuk menyimpan data
    private ProvinceCollection koleksiProvinsi;
    private Dictionary<string, ProvinceData> dataDict = new Dictionary<string, ProvinceData>();

    void Start()
    {
        if (touchManager == null)
        {
            Debug.LogError("Referensi TouchManager belum di-assign di InfoManager!");
        }

        // Memanggil fungsi pemuat data sebagai sebuah Coroutine
        StartCoroutine(LoadProvinceData());
        
        tombolTutupDetail.onClick.AddListener(HideDetailPanel);
        panelDaftarAlatMusik.SetActive(false);
        panelDetail.SetActive(false);
    }

    // Fungsi ini sekarang menjadi Coroutine untuk menangani pemuatan file di Android
    IEnumerator LoadProvinceData()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "AlatMusik.json");
        string jsonString = "";

        // Logika ini wajib untuk Android, karena file ada di dalam .apk
        if (path.Contains("://") || path.Contains(":///"))
        {
            using (UnityWebRequest www = UnityWebRequest.Get(path))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Gagal memuat AlatMusik.json: " + www.error);
                }
                else
                {
                    jsonString = www.downloadHandler.text;
                }
            }
        }
        // Logika ini untuk saat menjalankan di Editor Unity
        else
        {
            if (File.Exists(path))
            {
                jsonString = File.ReadAllText(path);
            }
            else
            {
                Debug.LogError("File AlatMusik.json tidak ditemukan! Pastikan ada di folder StreamingAssets.");
            }
        }

        // Proses data JSON hanya jika berhasil dimuat
        if (!string.IsNullOrEmpty(jsonString))
        {
            koleksiProvinsi = JsonUtility.FromJson<ProvinceCollection>(jsonString);
            dataDict.Clear();
            foreach (var data in koleksiProvinsi.provinsi)
            {
                dataDict[data.id_provinsi] = data;
            }

            // Kode debugging untuk mencetak semua ID yang berhasil dimuat ke Logcat
            string allLoadedKeys = "";
            foreach (string key in dataDict.Keys)
            {
                allLoadedKeys += "'" + key + "', ";
            }
            Debug.Log("SEMUA ID PROVINSI YANG DIMUAT DARI JSON: " + allLoadedKeys);
        }
    }
    
    // Fungsi ini dipanggil TouchManager untuk menampilkan daftar alat musik
    public void ShowInstrumentListForProvince(string provinceID)
    {
        HideDetailPanel();

        // Kode debugging untuk melihat ID provinsi yang dicari
        Debug.Log("Mencari data untuk provinsi: '---" + provinceID + "---'");
        
        // Hapus item-item lama dari daftar sebelum menampilkan yang baru
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        if (dataDict.TryGetValue(provinceID, out ProvinceData data))
        {
            // Buat item untuk setiap alat musik di provinsi tersebut
            foreach (var instrument in data.daftar_alat)
            {
                GameObject newItem = Instantiate(itemAlatMusikPrefab, contentArea);

                Transform imageTransform = newItem.transform.Find("Image");
                if (imageTransform == null)
                {
                    Debug.LogError("KESALAHAN PREFAB: Tidak bisa menemukan anak objek 'Image'. Periksa ejaan!");
                    continue; 
                }

                Button itemButton = imageTransform.GetComponent<Button>();
                if (itemButton == null)
                {
                    Debug.LogError("KESALAHAN PREFAB: Objek 'Image' tidak memiliki komponen <Button>.");
                    continue; 
                }

                imageTransform.GetComponent<Image>().sprite = LoadSpriteFromResources(instrument.gambar);
                newItem.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = instrument.nama_alat;

                // Tambahan: Set NamaProvinsi
                var namaProvinsiObj = newItem.transform.Find("NamaProvinsi");
                if (namaProvinsiObj != null)
                {
                    var namaProvinsiText = namaProvinsiObj.GetComponent<TextMeshProUGUI>();
                    if (namaProvinsiText != null)
                        namaProvinsiText.text = data.nama_provinsi;
                }
                else
                {
                    Debug.LogWarning("Prefab tidak memiliki child 'NamaProvinsi'.");
                }

                itemButton.onClick.AddListener(() =>
                {
                    ShowDetailPanel(instrument);
                });
            }
            
            panelDaftarAlatMusik.SetActive(true);
            
            // Matikan interaksi peta
            if (touchManager != null)
            {
                touchManager.SetMapInteraction(false);
            }
        }
        else
        {
            Debug.LogWarning("DATA TIDAK DITEMUKAN: Data untuk provinsi '" + provinceID + "' tidak ditemukan di JSON.");
        }
    }

    // Fungsi publik untuk menutup panel dari tombol UI
    public void HideInstrumentListPanel()
    {
        panelDaftarAlatMusik.SetActive(false);
        panelDetail.SetActive(false);

        // Aktifkan kembali interaksi peta
        if (touchManager != null)
        {
            touchManager.SetMapInteraction(true);
        }
    }

    // Menampilkan panel detail saat item alat musik diklik
    void ShowDetailPanel(Instrument instrumentData)
    {
        teksJudulDetail.text = instrumentData.nama_alat;
        teksDeskripsiDetail.text = instrumentData.deskripsi;
        gambarDetail.sprite = LoadSpriteFromResources(instrumentData.gambar);

        panelDetail.SetActive(true);
    }
    
    // Memuat gambar dari folder Resources
    Sprite LoadSpriteFromResources(string imageName)
    {
        string imageNameWithoutExtension = Path.GetFileNameWithoutExtension(imageName);
        Sprite loadedSprite = Resources.Load<Sprite>("GambarAlatMusik/" + imageNameWithoutExtension);
        if (loadedSprite == null)
        {
            Debug.LogError("GAMBAR TIDAK DITEMUKAN di 'Resources/GambarAlatMusik/': " + imageName);
        }
        return loadedSprite;
    }

    // Menyembunyikan panel detail saat tombol tutupnya diklik
    void HideDetailPanel()
    {
        panelDetail.SetActive(false);
    }
}