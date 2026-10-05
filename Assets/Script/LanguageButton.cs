using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bouton de choix de langue ("fr" ou "en"). Le bouton de la langue active est mis en avant.
/// </summary>
[RequireComponent(typeof(Button), typeof(Image))]
public class LanguageButton : MonoBehaviour
{
    [SerializeField] private string languageCode = "en";

    private static readonly Color Inactive = new Color(0.55f, 0.55f, 0.55f, 1f);

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => Loc.SetLanguage(languageCode));
    }

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
        GetComponent<Image>().color = Loc.Language == languageCode ? Color.white : Inactive;
    }
}
