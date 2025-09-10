using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

public class ObjectSpawn : MonoBehaviour
{
    [Header("AR")]
    public ARTrackedImageManager trackedImageManager;

    [System.Serializable]
    public class ImagePrefab
    {
        public string imageName;   // Nama marker di Reference Image Library
        public GameObject prefab;  // Prefab yang mau di-spawn
    }

    [Header("Mappings")]
    public ImagePrefab[] imagePrefabs;

    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();

    [System.Obsolete]
    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    [System.Obsolete]
    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    [System.Obsolete]
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Marker baru terdeteksi
        foreach (var trackedImage in eventArgs.added)
        {
            SpawnPrefab(trackedImage);
        }

        // Marker yang diupdate
        foreach (var trackedImage in eventArgs.updated)
        {
            UpdatePrefabPose(trackedImage);
        }

        // Marker yang hilang
        foreach (var trackedImage in eventArgs.removed)
        {
            string key = trackedImage.referenceImage.name;
            if (spawnedPrefabs.TryGetValue(key, out var go))
            {
                Destroy(go);
                spawnedPrefabs.Remove(key);
            }
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
                // 🚀 Spawn bebas (tidak parent ke marker, biar bisa mutar/animasi sendiri)
                var go = Instantiate(ip.prefab, trackedImage.transform.position, trackedImage.transform.rotation);
                spawnedPrefabs[key] = go;
                break;
            }
        }
    }

    private void UpdatePrefabPose(ARTrackedImage trackedImage)
    {
        string key = trackedImage.referenceImage.name;
        if (spawnedPrefabs.TryGetValue(key, out var go))
        {
            // Kalau mau prefab ikut marker: aktifkan baris ini
            // go.transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);

            // Kalau mau prefab bebas (cuma sekali spawn, lalu lepas)
            go.SetActive(true);
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
}
