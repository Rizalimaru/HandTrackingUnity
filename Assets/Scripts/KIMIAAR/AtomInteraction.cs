using UnityEngine;
using System.Collections;

public class AtomInteraction : MonoBehaviour
{
    private bool isMoving = false;
    private Rigidbody rb;

    // ID unik untuk setiap atom
    public string uniqueID { get; private set; }

    // Informasi atom
    public string atomName; // Nama atom (misalnya, Na, Cl, O)
    public string group;    // Golongan periodik (misalnya, 1A, 7A)

    public string targetCompound;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // physics mati (bisa digerakkan manual pakai script

    }

    void Update()
    {
        // Percobaaan klik dengan keyboard (untuk testing di editor)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnClicked();
        }


    }


public void OnClicked()
{
    if (!isMoving)
    {
        AtomInitializer initializer = FindFirstObjectByType<AtomInitializer>();
        if (initializer != null)
        {
            if (initializer.TryClickAtom(this))
            {
                // Kalau benar & sesuai urutan → jalanin animasi
                GameObject wadah = GameObject.FindWithTag("Wadah");
                if (wadah != null)
                {
                    Vector3 worldTarget = wadah.transform.position;
                    transform.SetParent(wadah.transform);
                    StartCoroutine(MoveToWadahThenFall(worldTarget));
                    isMoving = true;
                }
            }
        }
    }
}

private IEnumerator MoveToWadahThenFall(Vector3 targetPos)
{
    Debug.Log($"Atom {atomName} bergerak ke wadah di posisi {targetPos}");
    Vector3 startPos = transform.position;
    float duration = 3f;
    float elapsed = 0f;

    // ⬅️ Tambahkan offset di Y target
    targetPos = new Vector3(targetPos.x, targetPos.y + 0.01f, targetPos.z);

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        // Gerakan linear ke target
        Vector3 linearPos = Vector3.Lerp(startPos, targetPos, t);

        // Tambah arc naik (sinus)
        float yArc = Mathf.Sin(t * Mathf.PI) * 0.15f;

        // Terapkan posisi dengan arc
        transform.position = new Vector3(linearPos.x, linearPos.y + yArc, linearPos.z);

        yield return null;
    }

    // Setelah sampai wadah → jatuh alami
    rb.isKinematic = false;

    while (transform.position.y > targetPos.y + 0.05f) // toleransi
    {
        yield return null;
    }

    // Lock lagi persis di posisi target + offset
    rb.isKinematic = true;
    transform.position = new Vector3(transform.position.x, targetPos.y, transform.position.z);
}

}