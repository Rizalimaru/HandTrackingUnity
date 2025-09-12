using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NUnit.Framework.Constraints;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private GameObject uiSalah;
    [SerializeField] private GameObject uiBenar;


    [SerializeField] private GameObject Hasil;

    [SerializeField] private TextMeshProUGUI textHasil;



    [Header("UI Atom Info")]
    [SerializeField] private GameObject atomInfoPanel;
    [SerializeField] private TextMeshProUGUI atomNameText;
    [SerializeField] private TextMeshProUGUI atomDescText;
    [SerializeField] private Image atomIcon;

    void Awake() { Instance = this; }

    public void ShowSalah()
    {
        if (uiSalah != null) uiSalah.SetActive(true);
        Invoke("HideSalah", 1f);
    }

    void HideSalah() { if (uiSalah != null) uiSalah.SetActive(false); }

    public void ShowBenar()
    {
        if (uiBenar != null) uiBenar.SetActive(true);
    }

    public void SetHasilText(string text)
    {
        Hasil.SetActive(true);
        textHasil.text = text;

    }

    public void HideBenar()
    {
        Hasil.SetActive(false);
    }

    // 👇 Tambahan baru
    public void ShowAtomInfo(AtomData data)
    {
        if (atomInfoPanel != null)
        {
            atomInfoPanel.SetActive(true);

            if (atomNameText != null)
                atomNameText.text = $"{data.atomName} - {data.fullName}";

            if (atomDescText != null)
                atomDescText.text = $"{data.description}\nGolongan: {data.group}";

        }
    }

    public void HideAtomInfo()
    {
        if (atomInfoPanel != null)
        {
            atomInfoPanel.SetActive(false); // Sembunyikan panel
            Debug.Log("[DEBUG] Menyembunyikan panel info atom.");
        }
    }

    // Pindah scene
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }


}

   
