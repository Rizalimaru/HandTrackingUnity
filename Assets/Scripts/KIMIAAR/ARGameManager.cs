using UnityEngine;

public class ARGameManager : MonoBehaviour
{
    public static ARGameManager Instance;
    public Transform currentWadah;

    void Awake()
    {
        Instance = this;
    }

    public void SetWadah(Transform wadah)
    {
        currentWadah = wadah;
    }
}
