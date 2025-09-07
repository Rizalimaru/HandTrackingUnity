using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CompoundInfo
{
    public string nameCompound;           // Nama senyawa, contoh: Air
    public string formula;                // Rumus kimia, contoh: H₂O
    public List<string> requiredElements; // Misal: H, H, O
    public GameObject prefabResult;       // Prefab hasil seperti H2O
}

[CreateAssetMenu(fileName = "CompoundDatabase", menuName = "Chemistry/CompoundDatabase")]
public class CompoundDatabase : ScriptableObject
{
    public List<CompoundInfo> compounds;

    public CompoundInfo GetCompoundFromElements(List<string> input)
    {
        foreach (var compound in compounds)
        {
            if (IsMatch(compound.requiredElements, input))
                return compound;
        }
        return null;
    }

    private bool IsMatch(List<string> required, List<string> input)
    {
        var req = new List<string>(required);
        var inp = new List<string>(input);

        foreach (var symbol in req)
        {
            if (inp.Contains(symbol))
                inp.Remove(symbol);
            else
                return false;
        }
        return inp.Count == 0;
    }
}
