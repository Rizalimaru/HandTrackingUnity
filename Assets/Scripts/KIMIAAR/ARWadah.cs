using UnityEngine;

public class ARWadah : MonoBehaviour
{
    void Start()
    {
        if (ARGameManager.Instance != null)
            ARGameManager.Instance.SetWadah(this.transform);
    }
}
