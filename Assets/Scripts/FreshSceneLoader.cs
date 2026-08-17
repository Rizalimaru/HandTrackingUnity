using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class FreshSceneLoader : MonoBehaviour
{
    /// <summary>
    /// Load the unified KimiaAR scene in Racikan (quiz) mode.
    /// Call this from the "Mulai" / "Mode Racikan" button.
    /// </summary>
    public void LoadKimiaRacikan()
    {
        GameModeSelector.SelectedMode = GameModeSelector.KimiaMode.Racikan;
        StartCoroutine(LoadCleanRoutine("KimiaAR"));
    }

    /// <summary>
    /// Load the unified KimiaAR scene in Kamus (dictionary) mode.
    /// Call this from the "Kamus Mode" button.
    /// </summary>
    public void LoadKimiaKamus()
    {
        GameModeSelector.SelectedMode = GameModeSelector.KimiaMode.Kamus;
        StartCoroutine(LoadCleanRoutine("KimiaAR"));
    }

    /// <summary>
    /// Legacy method — kept for backward compatibility with Kimia1/Kimia2 backup scenes.
    /// Panggil ini dari UI Button / kode: LoadFresh("Kimia1");
    /// </summary>
    public void LoadFresh(string sceneName)
    {
        StartCoroutine(LoadCleanRoutine(sceneName));
    }

    private IEnumerator LoadCleanRoutine(string sceneName)
    {
        Debug.Log("[FreshSceneLoader] Preparing clean load for: " + sceneName);

        // 1) Clear any spawned AR objects (without destroying the AR Session itself)
        foreach (var sp in Object.FindObjectsByType<ObjectSpawner>(FindObjectsSortMode.None))
        {
            Debug.Log("[FreshSceneLoader] Clearing ObjectSpawner: " + sp.gameObject.name);
            sp.ClearAllSpawnedObjects();
        }

        // 2) Release unused assets & run GC
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();
        yield return null;

        // 3) Load scene (Single mode replaces the current scene)
        Debug.Log("[FreshSceneLoader] Loading scene: " + sceneName);
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!op.isDone) yield return null;

        Debug.Log("[FreshSceneLoader] Clean load finished: " + sceneName);
    }
}
