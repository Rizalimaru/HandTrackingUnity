using UnityEngine;

public class AppRelaunch : MonoBehaviour
{
    public void RelaunchApp()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string packageName = Application.identifier; // ex: com.rizaltech.kimia

        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject launchIntent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);

            if (launchIntent != null)
            {
                // tambahkan flags agar restart benar-benar seperti "fresh start"
                int FLAG_ACTIVITY_CLEAR_TOP = 0x04000000;
                int FLAG_ACTIVITY_NEW_TASK  = 0x10000000;
                launchIntent.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_CLEAR_TOP | FLAG_ACTIVITY_NEW_TASK);

                currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    currentActivity.Call("startActivity", launchIntent);
                    currentActivity.Call("finish"); // tutup activity lama
                }));
            }
            else
            {
                Debug.LogError("Launch intent tidak ditemukan untuk package: " + packageName);
            }
        }
#endif
    }
}
