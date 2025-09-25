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

    public void BackToMainMenu()
    {
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


