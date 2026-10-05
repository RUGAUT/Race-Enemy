using TMPro;
using UnityEngine;

/// <summary>
/// Traduit automatiquement un texte TextMeshPro a partir d'une cle (voir Loc.cs)
/// et le met a jour quand la langue change.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;

    private void OnEnable()
    {
        Loc.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        Loc.Changed -= Refresh;
    }

    private void Refresh()
    {
        if (!string.IsNullOrEmpty(key))
            GetComponent<TMP_Text>().text = Loc.T(key);
    }
}
