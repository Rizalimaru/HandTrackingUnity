package com.myunity.speech;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.speech.RecognizerIntent;
import android.speech.SpeechRecognizer;
import android.speech.RecognitionListener;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.util.ArrayList;

public class SpeechPlugin {
    private static SpeechRecognizer recognizer;

    public static void StartListening(final Activity activity) {
        if (recognizer == null) {
            recognizer = SpeechRecognizer.createSpeechRecognizer(activity);
            recognizer.setRecognitionListener(new RecognitionListener() {
                @Override public void onResults(Bundle results) {
                    ArrayList<String> matches = results.getStringArrayList(SpeechRecognizer.RESULTS_RECOGNITION);
                    if (matches != null && matches.size() > 0) {
                        // Kirim hasil ke Unity
                        UnityPlayer.UnitySendMessage("VoiceManager", "OnSpeechResult", matches.get(0));
                    }
                }

                @Override public void onError(int error) {
                    UnityPlayer.UnitySendMessage("VoiceManager", "OnSpeechResult", "Error code: " + error);
                }

                // yang lain boleh dikosongin
                @Override public void onReadyForSpeech(Bundle params) {}
                @Override public void onBeginningOfSpeech() {}
                @Override public void onRmsChanged(float rmsdB) {}
                @Override public void onBufferReceived(byte[] buffer) {}
                @Override public void onEndOfSpeech() {}
                @Override public void onPartialResults(Bundle partialResults) {}
                @Override public void onEvent(int eventType, Bundle params) {}
            });
        }

        Intent intent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL, RecognizerIntent.LANGUAGE_MODEL_FREE_FORM);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE, "id-ID"); // ganti en-US kalau mau English
        recognizer.startListening(intent);
    }
}
