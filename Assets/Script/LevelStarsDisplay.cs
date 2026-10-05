using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Affiche dans le menu les etoiles obtenues sur un niveau (voir StarRating).
/// </summary>
public class LevelStarsDisplay : MonoBehaviour
{
    [SerializeField] private string sceneName = "LV1";
    [SerializeField] private Image[] stars;
    [SerializeField] private Sprite starOn;
    [SerializeField] private Sprite starOff;

    private void OnEnable()
    {
        int earned = StarRating.Get(sceneName);
        for (int i = 0; i < stars.Length; i++)
            if (stars[i] != null) stars[i].sprite = i < earned ? starOn : starOff;
    }
}
