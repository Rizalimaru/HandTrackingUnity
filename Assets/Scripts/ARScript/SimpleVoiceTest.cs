using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class SimpleVoiceTest : MonoBehaviour
{
    private AndroidJavaObject activity;
    private AndroidJavaObject speechRecognizer;
    public TextMeshProUGUI teksKata;

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }

            // Panggil SpeechRecognizer Intent
            using (AndroidJavaClass speechRecognizerClass = new AndroidJavaClass("android.speech.SpeechRecognizer"))
            {
                speechRecognizer = speechRecognizerClass.CallStatic<AndroidJavaObject>("createSpeechRecognizer", activity);
            }

            StartListening();
        }
        else
        {
            Debug.LogWarning("Speech recognition hanya bisa di Android device");
        }
    }

    public void StartListening()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
            {
                string ACTION_RECOGNIZE_SPEECH = intentClass.GetStatic<string>("ACTION_RECOGNIZE_SPEECH");
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", ACTION_RECOGNIZE_SPEECH);

                intent.Call<AndroidJavaObject>("putExtra", "android.speech.extra.LANGUAGE_MODEL", "free_form");
                intent.Call<AndroidJavaObject>("putExtra", "android.speech.extra.LANGUAGE", "en-US"); // bisa diganti "id-ID"

                activity.Call("startActivityForResult", intent, 10);
            }
        }
    }
}
