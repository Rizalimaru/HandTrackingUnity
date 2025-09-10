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
    }

    // Mendapatkan nama instrumen yang aktif di scene
    public List<string> GetActiveInstrumentNames()
    {
        List<string> names = new List<string>();
        foreach (var kvp in spawnedPrefabs)
        {
            if (kvp.Value != null && kvp.Value.activeInHierarchy)
                names.Add(kvp.Key);
        }
        return names;
    }

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
                // Jika prefab belum ada (misal setelah reset), spawn ulang
                if (!spawnedPrefabs.ContainsKey(key))
                    SpawnPrefab(trackedImage);

                UpdatePrefabPose(trackedImage);
            }
            else
            {
                if (spawnedPrefabs.TryGetValue(key, out var go))
                    go.SetActive(false);
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
            spawnedPrefabs[key].SetActive(true);
            return;
        }

        foreach (var ip in imagePrefabs)
        {
            if (ip.imageName == key && ip.prefab != null)
            {
                var go = Instantiate(ip.prefab, trackedImage.transform);
                spawnedPrefabs[key] = go;
                StartCoroutine(ShowQuestionAfterDelay(key, 0.2f));

                // Daftarkan QR ke GameManager
                var gm = FindFirstObjectByType<GameManager>();
                if (gm != null)
                    gm.RegisterQRCode(key);

                break;
            }
        }
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

    private void UpdatePrefabPose(ARTrackedImage trackedImage)
    {
        string key = trackedImage.referenceImage.name;
        if (spawnedPrefabs.TryGetValue(key, out var go))
        {
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

    public void ResetScanning()
{
    // Hapus semua objek yang telah di-spawn
    ClearAllSpawnedObjects();

    // Nonaktifkan dan aktifkan kembali ARTrackedImageManager untuk memaksa refresh
    if (trackedImageManager != null)
    {
        trackedImageManager.enabled = false;
        trackedImageManager.enabled = true;
    }

    // Izinkan proses scanning dimulai ulang
    canSpawn = true;

    Debug.Log("Scanning telah di-reset.");
}

    public GameObject GetSpawnedInstrument(string instrumentName)
    {
        if (spawnedPrefabs.TryGetValue(instrumentName, out var go))
            return go;
        return null;
    }
}
