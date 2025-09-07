using UnityEngine;
using System.Collections.Generic;

public class TouchManager : MonoBehaviour
{
    public InfoManager infoManager;
    public GameObject pointer;
    public Camera mainCamera;
    public float zoomFOV = 30f;
    public float zoomSpeed = 5f;
    public Vector3 offset = new Vector3(0, 5, -5);

    public AudioClip touchSound; // Tambahkan ini
    private AudioSource audioSource; // Tambahkan ini

    private ProvinceTouch lastSelectedProvince;
    private Vector3 initialCamPos;
    private Quaternion initialCamRot;
    private float initialFOV;

    private Vector3 targetPos;
    private Quaternion targetRot;
    private float targetFOV;
    
    private bool isMapInteractionEnabled = true;

    void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        initialCamPos = mainCamera.transform.position;
        initialCamRot = mainCamera.transform.rotation;
        initialFOV = mainCamera.fieldOfView;

        targetPos = initialCamPos;
        targetRot = initialCamRot;
        targetFOV = initialFOV;

        // Tambahkan inisialisasi AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (isMapInteractionEnabled)
        {
            if (Input.GetMouseButtonDown(0))
            {
                DetectTouch(Input.mousePosition);
            }
        }
        
        // Pergerakan kamera
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * zoomSpeed);
        mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRot, Time.deltaTime * zoomSpeed);
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }

    void DetectTouch(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            ProvinceTouch province = hit.collider.GetComponent<ProvinceTouch>();
            if (province != null)
            {
                HandleProvinceSelection(province);
                return;
            }
        }
        ResetCameraAndSelection();
    }

    void HandleProvinceSelection(ProvinceTouch province)
    {
        if (lastSelectedProvince != null)
        {
            lastSelectedProvince.OnRelease();
        }

        province.OnTouch();
        lastSelectedProvince = province;
        
        targetPos = province.GetComponent<Collider>().bounds.center + offset;
        targetRot = Quaternion.LookRotation(province.GetComponent<Collider>().bounds.center - targetPos);
        targetFOV = zoomFOV;

        // Mainkan sound saat map ditekan
        if (touchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(touchSound);
        }

        if (infoManager != null)
        {
            // Langsung gunakan nama GameObject yang sudah sama dengan ID di JSON
            infoManager.ShowInstrumentListForProvince(province.gameObject.name);
        }
    }

    // Fungsi ini dipanggil oleh ProvinceFinder saat lokasi GPS ditemukan
    public void HighlightProvinceByName(string provinceName)
    {
        GameObject provinceObject = GameObject.Find(provinceName);
        if (provinceObject != null)
        {
            ProvinceTouch province = provinceObject.GetComponent<ProvinceTouch>();
            if (province != null)
            {
                Debug.Log("Provinsi ditemukan dari GPS: " + provinceName);
                HandleProvinceSelection(province);

            }
        }
        else
        {
             Debug.LogWarning("Objek provinsi '" + provinceName + "' tidak ditemukan di scene untuk disorot.");
        }
    }

    public void ResetCameraAndSelection()
    {
        if (lastSelectedProvince != null)
        {
            lastSelectedProvince.OnRelease();
            lastSelectedProvince = null;
        }
        targetPos = initialCamPos;
        targetRot = initialCamRot;
        targetFOV = initialFOV;
        pointer.SetActive(false);
    }

    public void SetMapInteraction(bool isEnabled)
    {
        isMapInteractionEnabled = isEnabled;
    }
}