using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class SceneTransition : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        StartCoroutine(Reload(sceneName));
    }

    private System.Collections.IEnumerator Reload(string sceneName)
    {
        // Unload semua asset lama
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();

        // Load scene baru dan tutup semua scene lama
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// Load KimiaAR in Racikan (quiz) mode.
    /// </summary>
    public void LoadKimiaRacikan()
    {
        GameModeSelector.SelectedMode = GameModeSelector.KimiaMode.Racikan;
        LoadScene("KimiaAR");
    }

    /// <summary>
    /// Load KimiaAR in Kamus (dictionary) mode.
    /// </summary>
    public void LoadKimiaKamus()
    {
        GameModeSelector.SelectedMode = GameModeSelector.KimiaMode.Kamus;
        LoadScene("KimiaAR");
    }

    public void BackToMainMenu()
    {
        // Clean up any spawned AR objects before going back
        foreach (var sp in Object.FindObjectsByType<ObjectSpawner>(FindObjectsSortMode.None))
        {
            sp.ClearAllSpawnedObjects();
        }
        SceneManager.LoadScene("MainmenuKimia");
    }


    public void QuitApp()
    {
        Application.Quit();
        // Untuk editor, agar keluar play mode saat testing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}
