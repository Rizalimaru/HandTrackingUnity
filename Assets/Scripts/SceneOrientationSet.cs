using UnityEngine;

public class SceneOrientationSet : MonoBehaviour
{
    public enum Orientation { Portrait, Landscape }

    [Header("Pilih orientasi untuk scene ini")]
    public Orientation sceneOrientation = Orientation.Portrait;

    void Start()
    {
        if (sceneOrientation == Orientation.Portrait)
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }
        else
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
    }
}
