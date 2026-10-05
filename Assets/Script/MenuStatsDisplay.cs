using TMPro;
using UnityEngine;

/// <summary>
/// Affiche dans le menu les statistiques sauvegardees (MainMenu_Pro).
/// Lit PlayerPrefs "BestZombieScore" : le record de zombies elimines en une partie.
/// </summary>
public class MenuStatsDisplay : MonoBehaviour
{
    public const string BestZombieScoreKey = "BestZombieScore";

    [SerializeField] private TextMeshProUGUI zombieRecordText;

    private void OnEnable()
    {
        if (zombieRecordText != null)
            zombieRecordText.text = PlayerPrefs.GetInt(BestZombieScoreKey, 0).ToString();
    }
}
