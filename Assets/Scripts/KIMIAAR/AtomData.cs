using UnityEngine;

[CreateAssetMenu(fileName = "AtomData", menuName = "Chemistry/Atom Data")]
public class AtomData : ScriptableObject
{
    [Header("Identitas Atom")]
    public string atomName;       // contoh: Na
    public string fullName;       // contoh: Natrium
    public string group;          // contoh: Golongan 1A
    public string description;    // penjelasan singkat

    [Header("Visual")]
    public Sprite icon;           // gambar atom untuk UI
    public Color displayColor = Color.white; // warna khusus kalau mau
}
