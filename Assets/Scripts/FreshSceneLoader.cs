using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class FreshSceneLoader : MonoBehaviour
{
    // Panggil ini dari UI Button / kode: LoadFresh("Kimia1");
    public void LoadFresh(string sceneName)
    {
        StartCoroutine(LoadFreshRoutine(sceneName));
    }

    private IEnumerator LoadFreshRoutine(string sceneName)
    {
        Debug.Log("[FreshSceneLoader] Preparing fresh load for: " + sceneName);

        // 1) Destroy possible persistent AR objects (ARSession, ARSessionOrigin, ARTrackedImageManager)
        foreach (var s in FindObjectsOfType<ARSession>(true))
        {
            Debug.Log("[FreshSceneLoader] Destroying ARSession: " + s.gameObject.name);
            Destroy(s.gameObject);
        }

        foreach (var o in FindObjectsOfType<ARSessionOrigin>(true))
        {
            Debug.Log("[FreshSceneLoader] Destroying ARSessionOrigin: " + o.gameObject.name);
            Destroy(o.gameObject);
        }

        foreach (var m in FindObjectsOfType<ARTrackedImageManager>(true))
        {
            Debug.Log("[FreshSceneLoader] Destroying ARTrackedImageManager: " + m.gameObject.name);
            Destroy(m.gameObject);
        }

        // 2) destroy any custom persistent spawners or managers (jika kamu pernah pakai DontDestroyOnLoad)
        foreach (var sp in FindObjectsOfType<ObjectSpawner>(true))
        {
            Debug.Log("[FreshSceneLoader] Clearing & destroying ObjectSpawner: " + sp.gameObject.name);
            sp.ClearAllSpawnedObjects();
            Destroy(sp.gameObject);
        }

        // 3) release unused assets & run GC (memberi kesempatan Unity untuk cleanup)
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();
        yield return null;

        // 4) Load scene sebagai Single (ini akan unload scene lain jika masih ada)
        Debug.Log("[FreshSceneLoader] Loading scene: " + sceneName);
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!op.isDone) yield return null;

        // 5) Wait a frame and try to reset the new ARSession if it exists
        yield return null;
        var newSession = FindObjectOfType<ARSession>();
        if (newSession != null)
        {
            Debug.Log("[FreshSceneLoader] Resetting ARSession in new scene");
            // disable/enable + Reset untuk memastikan subsystems restart
            newSession.enabled = false;
            yield return null;
            newSession.enabled = true;
            newSession.Reset();
        }

        Debug.Log("[FreshSceneLoader] Fresh load finished: " + sceneName);
    }
}
