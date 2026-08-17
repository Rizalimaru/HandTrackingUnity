/// <summary>
/// Static helper to pass the selected game mode from the main menu to the KimiaAR scene.
/// Set GameModeSelector.SelectedMode before calling SceneManager.LoadScene("KimiaAR").
/// </summary>
public static class GameModeSelector
{
    public enum KimiaMode
    {
        Racikan, // Mode Racikan Ajaib (quiz — Kimia1 equivalent)
        Kamus    // Mode Kamus Elemen  (dictionary — Kimia2 equivalent)
    }

    /// <summary>
    /// The mode that was chosen from the main menu.
    /// Read this in KimiaGameModeManager.Start() to activate the correct mode.
    /// </summary>
    public static KimiaMode SelectedMode = KimiaMode.Racikan;
}
