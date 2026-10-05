using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Slider de volume general pour le menu Options (MainMenu_Pro).
/// Sauvegarde la valeur dans PlayerPrefs ("MasterVolume") et l'applique des le lancement.
/// </summary>
[RequireComponent(typeof(Slider))]
public class MenuVolumeSlider : MonoBehaviour
{
    private const string Key = "MasterVolume";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplySavedVolume()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(Key, 1f);
    }

    private void Start()
    {
        Slider slider = GetComponent<Slider>();
        slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(Key, 1f));
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(Key, value);
    }
}
