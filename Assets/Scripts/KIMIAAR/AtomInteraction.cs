using UnityEngine;
using System.Collections;

public class AtomInteraction : MonoBehaviour
{
    private bool isMoving = false;
    private Rigidbody rb;

    // --- Variabel Hint ---
    private Coroutine hintCoroutine;
    private Renderer[] atomRenderers;
    private float originalZ;

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
            StopHint(); // Hentikan hint jika atom ini diklik
            
            AtomInitializer initializer = FindFirstObjectByType<AtomInitializer>();
        if (initializer != null)
        {
            if (initializer.TryClickAtom(this))
            {
                // Kalau benar & sesuai urutan → jalanin animasi
                // Gunakan posisi dari initializer karena script tersebut menempel di Wadah
                Vector3 worldTarget = initializer.transform.position;
                transform.SetParent(initializer.transform);
                StartCoroutine(MoveToWadahThenFall(worldTarget));
                isMoving = true;
            }
        }
    }
}

    private IEnumerator MoveToWadahThenFall(Vector3 targetPos)
    {
        AudioKimia.Instance.PlaySFX(1); // suara ambil atom
        Debug.Log($"Atom {atomName} melompat ke wadah di posisi {targetPos}");
        
        Vector3 startPos = transform.position;
        
        // Tambah sedikit random offset agar jika ada banyak atom, jatuhnya tidak bertumpuk di 1 titik persis
        Vector3 randomOffset = new Vector3(
            UnityEngine.Random.Range(-0.03f, 0.03f), 
            0.02f, // Beri sedikit jarak dari dasar wadah agar tidak tembus
            UnityEngine.Random.Range(-0.03f, 0.03f)
        );
        targetPos += randomOffset;

        // Waktu tempuh dipercepat jadi 0.8 detik agar lebih responsif
        float duration = 0.8f;
        float elapsed = 0f;

        // Tinggi parabola tergantung jarak, minimal 0.15 unit
        float distance = Vector3.Distance(startPos, targetPos);
        float jumpHeight = Mathf.Max(0.15f, distance * 0.8f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // 1. Gerakan linear mendatar (X dan Z) + Lerp Y
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            // 2. Tambahkan rumus Parabola fisik pada sumbu Y: 4 * h * t * (1 - t)
            float parabola = 4f * jumpHeight * t * (1f - t);
            currentPos.y += parabola;

            transform.position = currentPos;

            // 3. Efek rotasi berputar saat melayang (seperti dilempar)
            transform.Rotate(Vector3.right * (500f * Time.deltaTime), Space.World);
            transform.Rotate(Vector3.up * (300f * Time.deltaTime), Space.World);

            yield return null;
        }

        // Pastikan posisi mendarat persis di target akhirnya
        transform.position = targetPos;
        
        // Posisikan secara rata agar tidak miring setelah berputar
        transform.rotation = Quaternion.identity;

        // Tetap lock agar tidak bergeser aneh karena physics AR
        rb.isKinematic = true;
    }

    // --- FITUR HINT LOMPAT ---
    public void TriggerHint()
    {
        if (hintCoroutine == null && !isMoving)
        {
            originalZ = transform.position.z;
            
            // Ambil semua renderer (untuk mengaktifkan emission)
            atomRenderers = GetComponentsInChildren<Renderer>();
            foreach (var r in atomRenderers)
            {
                if (r != null && r.material != null)
                {
                    r.material.EnableKeyword("_EMISSION");
                }
            }

            hintCoroutine = StartCoroutine(HintAnimation());
        }
    }

    public void StopHint()
    {
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
            // Kembalikan ke posisi Z semula
            transform.position = new Vector3(transform.position.x, transform.position.y, originalZ);

            // Matikan emission
            if (atomRenderers != null)
            {
                foreach (var r in atomRenderers)
                {
                    if (r != null && r.material != null)
                    {
                        r.material.SetColor("_EmissionColor", Color.black);
                    }
                }
            }
        }
    }

    private IEnumerator HintAnimation()
    {
        float elapsed = 0f;
        Color baseEmission = new Color(0.8f, 0.7f, 0.2f); // Kuning keemasan yang lebih soft

        while (true)
        {
            elapsed += Time.deltaTime * 6f; // Kecepatan lompat
            float pulse = Mathf.Abs(Mathf.Sin(elapsed));
            float jumpOffset = pulse * 0.03f; // Tinggi lompatan halus

            transform.position = new Vector3(transform.position.x, transform.position.y, originalZ + jumpOffset);
            
            // Efek berdenyut menggunakan material emission
            if (atomRenderers != null)
            {
                foreach (var r in atomRenderers)
                {
                    if (r != null && r.material != null)
                    {
                        // Intensitas emission dari 0 sampai ~1.5x
                        r.material.SetColor("_EmissionColor", baseEmission * (pulse * 1.5f));
                    }
                }
            }

            yield return null;
        }
    }

}