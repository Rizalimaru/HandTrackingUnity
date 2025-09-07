using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.Android;
using System.Collections;
using TMPro;

public class ProvinceFinder : MonoBehaviour
{
    public TextMeshProUGUI locationText;
    public TouchManager touchManager; // Referensi ke TouchManager

    [System.Serializable]
    public class GeoJSON { public string type; public List<Feature> features; }
    [System.Serializable]
    public class Feature { public string type; public Properties properties; public Geometry geometry; }
    [System.Serializable]
    public class Properties { public string NAME_1; }
    [System.Serializable]
    public class Geometry { public string type; public JToken coordinates; }

    private GeoJSON data;

    IEnumerator Start()
    {
        if (touchManager == null)
        {
            locationText.text = "Error: TouchManager belum di-assign.";
            Debug.LogError("TouchManager reference is not set in ProvinceFinder inspector.");
            yield break;
        }

        // 1. Minta izin lokasi
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                locationText.text = "Menunggu izin lokasi...";
                yield return null;
            }
        }

        // 2. Cek apakah GPS device aktif
        if (!Input.location.isEnabledByUser)
        {
            locationText.text = "GPS tidak aktif. Masuk ke mode bebas.";
            Debug.LogWarning("GPS not enabled. Entering freeroam mode.");
            // 🔹 PERBAIKAN: Panggil fungsi yang benar untuk masuk mode bebas
            touchManager.ResetCameraAndSelection();
            touchManager.SetMapInteraction(true); // Pastikan interaksi peta aktif
            yield break;
        }

        Input.location.Start();

        // 3. Tunggu GPS aktif
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            locationText.text = "Mengaktifkan GPS...";
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            locationText.text = "Gagal mendapatkan lokasi. Masuk ke mode bebas.";
            Debug.LogError("Location service failed. Entering freeroam mode.");
            // 🔹 PERBAIKAN: Panggil fungsi yang benar
            touchManager.ResetCameraAndSelection();
            touchManager.SetMapInteraction(true);
            Input.location.Stop();
            yield break;
        }

        // 4. Ambil file GeoJSON
        string path = Path.Combine(Application.streamingAssetsPath, "IDN_1.json");
        string json = "";

        if (path.Contains("://") || path.Contains(":///"))
        {
            using (var www = UnityEngine.Networking.UnityWebRequest.Get(path))
            {
                yield return www.SendWebRequest();
                if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    locationText.text = "Gagal memuat file provinsi.";
                    yield break;
                }
                json = www.downloadHandler.text;
            }
        }
        else
        {
            json = File.ReadAllText(path);
        }

        data = JsonConvert.DeserializeObject<GeoJSON>(json);

        // 5. Dapatkan lokasi SEKALI saja
        if (Input.location.status == LocationServiceStatus.Running)
        {
            float lat = Input.location.lastData.latitude;
            float lon = Input.location.lastData.longitude;
            Debug.Log($"GPS Data: Lat={lat}, Lon={lon}");

            string provinceNameJson = GetProvinceFromCoords(lon, lat);

            if (provinceNameJson != null)
            {
                locationText.text = "Provinsi ditemukan: " + provinceNameJson;
                // Panggil TouchManager untuk highlight provinsi
                touchManager.HighlightProvinceByName(provinceNameJson);
            }
            else
            {
                locationText.text = "Provinsi tidak terdeteksi. Masuk ke mode bebas.";
                // 🔹 PERBAIKAN: Panggil fungsi yang benar
                touchManager.ResetCameraAndSelection();
                touchManager.SetMapInteraction(true);
            }
        }
        else
        {
            locationText.text = "Gagal mendapatkan lokasi final. Masuk mode bebas.";
            // 🔹 PERBAIKAN: Panggil fungsi yang benar
            touchManager.ResetCameraAndSelection();
            touchManager.SetMapInteraction(true);
        }

        Input.location.Stop();
    }

    string GetProvinceFromCoords(float lon, float lat)
    {
        foreach (var feature in data.features)
        {
            if (feature.geometry.type == "Polygon")
            {
                var polygon = feature.geometry.coordinates[0];
                if (IsPointInPolygon(lon, lat, polygon))
                    return feature.properties.NAME_1;
            }
            else if (feature.geometry.type == "MultiPolygon")
            {
                foreach (var poly in feature.geometry.coordinates)
                {
                    var polygon = poly[0];
                    if (IsPointInPolygon(lon, lat, polygon))
                        return feature.properties.NAME_1;
                }
            }
        }
        return null;
    }

    bool IsPointInPolygon(float lon, float lat, JToken polygonToken)
    {
        var polygon = polygonToken.ToObject<List<List<float>>>();
        bool inside = false;
        int j = polygon.Count - 1;

        for (int i = 0; i < polygon.Count; j = i++)
        {
            float xi = polygon[i][0], yi = polygon[i][1];
            float xj = polygon[j][0], yj = polygon[j][1];

            bool intersect = ((yi > lat) != (yj > lat)) &&
                             (lon < (xj - xi) * (lat - yi) / (yj - yi) + xi);
            if (intersect)
                inside = !inside;
        }
        return inside;
    }
}