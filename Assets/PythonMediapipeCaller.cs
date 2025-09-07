using UnityEngine;
using System.Diagnostics;

public class PythonMediapipeCaller : MonoBehaviour
{
    private Process pythonProcess;

    void Start()
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "python";
        start.Arguments = "Assets/MediapipeScript.py";
        start.UseShellExecute = false;
        start.CreateNoWindow = true;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;

        pythonProcess = Process.Start(start);
        UnityEngine.Debug.Log("Python script started.");
    }

    void OnDisable()
    {
        KillPythonProcess();
    }

    void OnApplicationQuit()
    {
        KillPythonProcess();
    }

    private void KillPythonProcess()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            try
            {
                pythonProcess.Kill(); // pakai true untuk membunuh semua proses anak jika ada
                UnityEngine.Debug.Log("Python script terminated.");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("Failed to terminate Python process: " + e.Message);
            }
        }
    }
}
