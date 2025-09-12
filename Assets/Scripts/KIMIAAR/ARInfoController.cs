using UnityEngine;
using System.Collections.Generic;

public class ARInfoController : MonoBehaviour
{
    [Tooltip("Drag ObjectSpawner dari scene ke sini")]
    public ObjectSpawner objectSpawner;

    // Untuk melacak info apa yang sedang ditampilkan
    private string currentlyDisplayedKey = null;

    void Update()
    {
        if (objectSpawner == null)
        {
            Debug.LogError("ObjectSpawner belum di-assign di ARInfoController!");
            return;
        }

        // Dapatkan daftar nama objek/gambar yang aktif dari ObjectSpawner
        List<string> activeObjects = objectSpawner.GetActiveInstrumentNames();

        // LOGIKA UTAMA:
        // 1. Jika ada TEPAT SATU objek yang aktif
        if (activeObjects.Count == 1)
        {
            string activeKey = activeObjects[0];

            // Hanya update UI jika objek yang aktif adalah objek baru (bukan yang sama dari frame sebelumnya)
            if (activeKey != currentlyDisplayedKey)
            {
                // Ambil GameObject yang sedang aktif
                GameObject spawnedObject = objectSpawner.GetSpawnedInstrument(activeKey);
                if (spawnedObject != null)
                {
                    // Ambil komponen AtomInfo dari objek tersebut
                    AtomInfo atomInfo = spawnedObject.GetComponent<AtomInfo>();
                    if (atomInfo != null)
                    {
                        // Panggil fungsi untuk menampilkan info
                        atomInfo.ShowInfo();
                        
                        // Tandai bahwa kita sedang menampilkan info untuk objek ini
                        currentlyDisplayedKey = activeKey;
                    }
                }
            }
        }
        // 2. Jika TIDAK ADA objek aktif, atau LEBIH DARI SATU
        else
        {
            // Hanya sembunyikan UI jika sebelumnya ada yang ditampilkan
            if (currentlyDisplayedKey != null)
            {
                // Panggil fungsi untuk menyembunyikan panel info
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.HideAtomInfo();
                }

                // Reset pelacak
                currentlyDisplayedKey = null;
            }
        }
    }
}