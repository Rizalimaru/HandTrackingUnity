using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

/// <summary>
/// Manages switching between Racikan (quiz) and Kamus (dictionary) modes
/// within a single unified KimiaAR scene.
/// 
/// Handles:
///  - Activating/deactivating mode-specific GameObjects
///  - Swapping the ARTrackedImageManager's reference image library at runtime
///  - Cleaning up spawned objects when switching modes
/// </summary>
public class KimiaGameModeManager : MonoBehaviour
{
    public static KimiaGameModeManager Instance;

    [Header("Mode Root Objects")]
    [Tooltip("Parent GameObject containing all Racikan-mode objects (quiz UI, spawn points, AtomInitializer, etc.)")]
    public GameObject racikanParent;

    [Tooltip("Parent GameObject containing all Kamus-mode objects (ARInfoController, etc.)")]
    public GameObject kamusParent;

    [Header("AR Components")]
    [Tooltip("The single ARTrackedImageManager in this scene")]
    public ARTrackedImageManager trackedImageManager;

    [Tooltip("The single ObjectSpawner in this scene")]
    public ObjectSpawner objectSpawner;

    [Header("Reference Image Libraries")]
    [Tooltip("Library for Racikan mode (Kimia1 markers — e.g. Tong)")]
    public XRReferenceImageLibrary racikanLibrary;

    [Tooltip("Library for Kamus mode (Kimia2 markers — 50+ element markers)")]
    public XRReferenceImageLibrary kamusLibrary;

    [Header("Prefab Mappings")]
    [Tooltip("imagePrefabs mapping for Racikan mode (Tong → SpawnWadah)")]
    public ObjectSpawner.ImagePrefab[] racikanPrefabs;

    [Tooltip("imagePrefabs mapping for Kamus mode (50+ element prefabs)")]
    public ObjectSpawner.ImagePrefab[] kamusPrefabs;

    /// <summary>
    /// The currently active mode.
    /// </summary>
    public GameModeSelector.KimiaMode CurrentMode { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Read the mode selected from the main menu
        ApplyMode(GameModeSelector.SelectedMode);
    }

    /// <summary>
    /// Switch to a new mode. Cleans up current tracked objects,
    /// swaps the reference image library, and activates the correct UI.
    /// </summary>
    public void ApplyMode(GameModeSelector.KimiaMode mode)
    {
        CurrentMode = mode;
        StartCoroutine(SwitchModeRoutine(mode));
    }

    private IEnumerator SwitchModeRoutine(GameModeSelector.KimiaMode mode)
    {
        Debug.Log($"[KimiaGameModeManager] Switching to mode: {mode}");

        // 1) Clean up any currently spawned AR objects
        if (objectSpawner != null)
        {
            objectSpawner.ClearAllSpawnedObjects();
        }

        // 2) Disable the tracked image manager before swapping library
        if (trackedImageManager != null)
        {
            trackedImageManager.enabled = false;
        }

        // Wait a frame for subsystems to settle
        yield return null;

        // 3) Swap library and prefab mappings
        if (mode == GameModeSelector.KimiaMode.Racikan)
        {
            if (trackedImageManager != null && racikanLibrary != null)
            {
                trackedImageManager.referenceLibrary = racikanLibrary;
            }
            if (objectSpawner != null && racikanPrefabs != null)
            {
                objectSpawner.imagePrefabs = racikanPrefabs;
            }
        }
        else // Kamus
        {
            if (trackedImageManager != null && kamusLibrary != null)
            {
                trackedImageManager.referenceLibrary = kamusLibrary;
            }
            if (objectSpawner != null && kamusPrefabs != null)
            {
                objectSpawner.imagePrefabs = kamusPrefabs;
            }
        }

        // 4) Activate/deactivate mode-specific GameObjects
        if (racikanParent != null)
            racikanParent.SetActive(mode == GameModeSelector.KimiaMode.Racikan);
        if (kamusParent != null)
            kamusParent.SetActive(mode == GameModeSelector.KimiaMode.Kamus);

        // 5) Wait a frame, then re-enable tracked image manager
        yield return null;
        if (trackedImageManager != null)
        {
            trackedImageManager.enabled = true;
        }

        // 6) Ensure ObjectSpawner can spawn
        if (objectSpawner != null)
        {
            objectSpawner.canSpawn = true;
        }

        Debug.Log($"[KimiaGameModeManager] Mode switched to: {mode}");
    }

    /// <summary>
    /// Call this from UI to go back to main menu.
    /// Cleans up before transitioning.
    /// </summary>
    public void BackToMainMenu()
    {
        if (objectSpawner != null)
            objectSpawner.ClearAllSpawnedObjects();

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainmenuKimia");
    }
}
