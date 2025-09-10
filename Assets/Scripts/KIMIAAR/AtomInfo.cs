using UnityEngine;

public class AtomInfo : MonoBehaviour
{
    public AtomData data; // drag ScriptableObject ke sini lewat Inspector

    void Update()
    {
        // Debug testing di editor → tekan SPACE untuk langsung munculin info atom ini
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowInfo();
        }
    }

    public void ShowInfo()
    {
        if (UIManager.Instance != null && data != null)
        {
            UIManager.Instance.ShowAtomInfo(data);
            Debug.Log($"[DEBUG] Menampilkan info atom: {data.atomName}");
        }
    }
}
