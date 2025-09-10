using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private GameObject uiSalah;
    [SerializeField] private GameObject uiBenar;

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
        Invoke("HideSalah", 2f);
    }

    void HideSalah() { if (uiSalah != null) uiSalah.SetActive(false); }

    public void ShowBenar()
    {
        if (uiBenar != null) uiBenar.SetActive(true);
    }

    public void SetHasilText(string text)
    {
        if (textHasil != null)
        {
            textHasil.gameObject.SetActive(true);
            textHasil.text = text;
        }
    }

    public void HideBenar()
    {
        textHasil.gameObject.SetActive(false);
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

            if (atomIcon != null && data.icon != null)
                atomIcon.sprite = data.icon;
        }
    }

    public void HideAtomInfo()
    {
        if (atomInfoPanel != null)
            atomInfoPanel.SetActive(false);
    }
}
