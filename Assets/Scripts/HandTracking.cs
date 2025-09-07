using UnityEngine;

public class HandTracking : MonoBehaviour
{
    public UDPReceiver udpReceiver;
    public GameObject[] leftHandPoints;
    public GameObject[] rightHandPoints;
    public GameObject leftLine;
    public GameObject rightLine;

    public float yMin = -1.5f;
    public float yMax = 1.5f;

    private Vector3[] leftLandmarks = new Vector3[21];
    private Vector3[] rightLandmarks = new Vector3[21];

    public Vector3[] GetLeftLandmarks() => leftLandmarks;
    public Vector3[] GetRightLandmarks() => rightLandmarks;
    public string GetHandPose(Vector3[] landmarks) => GetHandPoseInternal(landmarks);
    private bool leftHandDetected = false;
    private bool rightHandDetected = false;

    void Update()
    {
        // Reset detection flag setiap frame
        leftHandDetected = false;
        rightHandDetected = false;

        string data = udpReceiver.data;
        if (string.IsNullOrEmpty(data)) return;

        data = data.Trim('[', ']');
        string[] items = data.Split(',');

        if (items.Length < 2) return;

        int index = 0;
        while (index < items.Length)
        {
            string handType = items[index].Trim().Replace("'", "");
            index++;

            if (index + 62 >= items.Length) break;

            GameObject[] targetPoints = handType == "Left" ? leftHandPoints : rightHandPoints;
            Vector3[] targetLandmarks = handType == "Left" ? leftLandmarks : rightLandmarks;

            if (handType == "Left") leftHandDetected = true;
            if (handType == "Right") rightHandDetected = true;

            for (int i = 0; i < 21; i++)
            {
                float x = 7 - float.Parse(items[index++]) / 100f;
                float y = float.Parse(items[index++]) / 100f;
                float z = float.Parse(items[index++]) / 100f;

                Vector3 pos = new Vector3(x, y, z);
                targetLandmarks[i] = pos;

                if (targetPoints != null && targetPoints.Length > i && targetPoints[i] != null)
                {
                    targetPoints[i].transform.localPosition = pos;
                    targetPoints[i].SetActive(true); // Pastikan titik diaktifkan saat data ada
                }
            }

            string pose = GetHandPose(targetLandmarks);
            Debug.Log($"{handType} Hand Pose: {pose}");
        }

        // Sembunyikan tangan jika tidak terdeteksi
        if (!leftHandDetected) SetHandVisibility(leftHandPoints, false);
        if (!rightHandDetected) SetHandVisibility(rightHandPoints, false);
        if (!leftHandDetected)
        {
            leftLine.SetActive(false);
        }
        else
        {
            leftLine.SetActive(true);
        }
        
        if (!rightHandDetected)
        {
            rightLine.SetActive(false);
        }
        else
        {
            rightLine.SetActive(true);
        }

        

    }

    void SetHandVisibility(GameObject[] handPoints, bool visible)
    {
        if (handPoints == null) return;
        foreach (GameObject point in handPoints)
        {
            if (point != null)
                point.SetActive(visible);
        }
    }

    bool IsFingerBent(Vector3 tip, Vector3 pip, Vector3 mcp)
    {
        return tip.y < pip.y && pip.y < mcp.y;
    }

    private string GetHandPoseInternal(Vector3[] landmarks)
    {
        if (landmarks.Length < 21) return "Unknown";

        bool thumbBent = IsFingerBent(landmarks[4], landmarks[3], landmarks[2]);
        bool indexBent = IsFingerBent(landmarks[8], landmarks[7], landmarks[6]);
        bool middleBent = IsFingerBent(landmarks[12], landmarks[11], landmarks[10]);
        bool ringBent = IsFingerBent(landmarks[16], landmarks[15], landmarks[14]);
        bool pinkyBent = IsFingerBent(landmarks[20], landmarks[19], landmarks[18]);

        int bentCount = 0;
        if (thumbBent) bentCount++;
        if (indexBent) bentCount++;
        if (middleBent) bentCount++;
        if (ringBent) bentCount++;
        if (pinkyBent) bentCount++;

        if (bentCount >= 4) return "Fist";
        if (bentCount == 0) return "Open";
        if (!indexBent && middleBent && ringBent && pinkyBent) return "Point";

        return "Unknown";
    }

}
