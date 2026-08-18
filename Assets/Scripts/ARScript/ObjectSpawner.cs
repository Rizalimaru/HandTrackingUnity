using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

public class ObjectSpawner : MonoBehaviour
{
    [Header("AR")]
    public ARTrackedImageManager trackedImageManager;
    public bool canSpawn = true;

    [System.Serializable]
    public class ImagePrefab
    {
        public string imageName;   // Nama marker di Reference Image Library
        public GameObject prefab;  // Prefab alat musik
    }

    [Header("Mappings")]
    public ImagePrefab[] imagePrefabs;

    private readonly Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();

    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        
        ClearAllSpawnedObjects();
    }

    // Mendapatkan nama instrumen yang aktif di scene
    public List<string> GetActiveInstrumentNames()
    {
        List<string> names = new List<string>();
        foreach (var kvp in spawnedPrefabs)
        {
            // Jangan masukkan objek yang masih menunggu reveal (belum muncul di layar)
            if (kvp.Value != null && kvp.Value.activeInHierarchy && !pendingReveal.Contains(kvp.Key))
                names.Add(kvp.Key);
        }
        return names;
    }

    // Toleransi waktu sebelum menyembunyikan objek (dalam detik)
    private readonly float hideGracePeriod = 2f;
    private readonly Dictionary<string, Coroutine> hideTimers = new Dictionary<string, Coroutine>();

    // Daftar objek yang masih menunggu reveal (jangan di-SetActive(true) oleh UpdatePrefabPose!)
    private readonly HashSet<string> pendingReveal = new HashSet<string>();

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        if (!canSpawn) return;
        foreach (var trackedImage in eventArgs.added)
        {
            SpawnPrefab(trackedImage);
            UpdatePrefabPose(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            string key = trackedImage.referenceImage.name;
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                // Batalkan timer sembunyi jika tracking kembali aktif
                if (hideTimers.TryGetValue(key, out var timer))
                {
                    if (timer != null) StopCoroutine(timer);
                    hideTimers.Remove(key);
                }

                // Jika prefab belum ada (misal setelah reset), spawn ulang
                if (!spawnedPrefabs.ContainsKey(key))
                    SpawnPrefab(trackedImage);

                UpdatePrefabPose(trackedImage);
            }
            else
            {
                // Jangan langsung sembunyikan! Beri toleransi dulu.
                if (!hideTimers.ContainsKey(key))
                {
                    hideTimers[key] = StartCoroutine(HideAfterGracePeriod(key));
                }
            }
        }

        foreach (var kvp in eventArgs.removed)
        {
            var trackedImage = kvp.Value;
            string key = trackedImage.referenceImage.name;

            if (spawnedPrefabs.TryGetValue(key, out var go))
            {
                Destroy(go);
                spawnedPrefabs.Remove(key);
            }
            // Bersihkan timer juga
            if (hideTimers.TryGetValue(key, out var t))
            {
                if (t != null) StopCoroutine(t);
                hideTimers.Remove(key);
            }
        }

        // Cek sistem question dan orkestra setiap ada perubahan QR
        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            // Cek question system
            var activeNames = GetActiveInstrumentNames();
            if (activeNames.Count == 1)
                gm.ShowQuestion(activeNames[0]);
            else if (activeNames.Count == 0)
                gm.HideQuestionPanel();

            // Cek orkestra
            gm.CheckOrchestra();
        }
    }

    private void SpawnPrefab(ARTrackedImage trackedImage)
    {
        string key = trackedImage.referenceImage.name;

        if (spawnedPrefabs.ContainsKey(key))
        {
            if (!pendingReveal.Contains(key))
                spawnedPrefabs[key].SetActive(true);
            return;
        }

        // Jangan spawn 2x jika sudah dalam antrian
        if (pendingReveal.Contains(key)) return;

        foreach (var ip in imagePrefabs)
        {
            if (ip.imageName == key && ip.prefab != null)
            {
                // JANGAN instantiate sekarang! Tandai dulu, tunggu tracking stabil.
                pendingReveal.Add(key);
                StartCoroutine(DelayedSpawn(key, ip.prefab, trackedImage));
                break;
            }
        }
    }

    private IEnumerator DelayedSpawn(string key, GameObject prefab, ARTrackedImage trackedImage)
    {
        // Tunggu sampai posisi tracking stabil (tidak spawn di depan muka)
        yield return new WaitForSeconds(0.8f);

        // Cek apakah tracked image masih valid
        if (trackedImage == null || trackedImage.trackingState == TrackingState.None)
        {
            pendingReveal.Remove(key);
            yield break;
        }

        // BARU sekarang buat objeknya, langsung di posisi marker yang sudah stabil
        var go = Instantiate(prefab, trackedImage.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        spawnedPrefabs[key] = go;
        pendingReveal.Remove(key);

        // Tampilkan UI setelah 0.7 detik lagi (total ~1.5 detik dari scan)
        StartCoroutine(ShowQuestionAfterDelay(key, 0.7f));

        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
            gm.RegisterQRCode(key);
    }

    private IEnumerator ShowQuestionAfterDelay(string key, float delay)
    {
        yield return new WaitForSeconds(delay);

        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.ShowQuestion(key);
        }
    }

    private IEnumerator HideAfterGracePeriod(string key)
    {
        yield return new WaitForSeconds(hideGracePeriod);

        if (spawnedPrefabs.TryGetValue(key, out var go))
        {
            go.SetActive(false);
        }
        hideTimers.Remove(key);
    }

    private void UpdatePrefabPose(ARTrackedImage trackedImage)
    {
        string key = trackedImage.referenceImage.name;
        if (spawnedPrefabs.TryGetValue(key, out var go))
        {
            // Jangan update posisi jika masih menunggu reveal
            if (pendingReveal.Contains(key)) return;

            go.SetActive(true);
            go.transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);
        }
    }

    public void ClearAllSpawnedObjects()
    {
        foreach (var go in spawnedPrefabs.Values)
        {
            if (go != null)
                Destroy(go);
        }
        spawnedPrefabs.Clear();
    }

    public GameObject GetSpawnedInstrument(string instrumentName)
    {
        if (spawnedPrefabs.TryGetValue(instrumentName, out var go))
            return go;
        return null;
    }
}
