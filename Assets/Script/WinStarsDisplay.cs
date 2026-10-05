using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Etoiles de victoire basees sur de vrais criteres. A placer sur le panel de victoire :
/// les etoiles sont calculees quand le panel s'affiche.
///   Etoile 1 : terminer le niveau (donc toujours obtenue sur un ecran de victoire)
///   Etoile 2 : finir avec au moins "healthPercentForStar2" % de vie
///   Etoile 3 : eliminer au moins "zombiesForStar3" zombies (le boss n'est pas compte)
/// </summary>
public class WinStarsDisplay : MonoBehaviour
{
    [SerializeField] private Image[] stars;
    [SerializeField] private TextMeshProUGUI[] labels;
    [SerializeField] private Sprite starOn;
    [SerializeField] private Sprite starOff;

    [Header("Criteres")]
    [SerializeField, Range(1, 100)] private int healthPercentForStar2 = 50;
    [SerializeField] private int zombiesForStar3 = 30;

    /// <summary>Etoiles obtenues sur la derniere victoire (lue par RunRewards).</summary>
    public static int LastStars;

    private static readonly Color Earned = new Color32(0xFF, 0xC8, 0x1E, 255);
    private static readonly Color Missed = new Color(1f, 1f, 1f, 0.45f);

    private void OnEnable()
    {
        bool[] got = Evaluate();
        int count = 0;
        foreach (bool g in got) if (g) count++;

        for (int i = 0; i < stars.Length && i < got.Length; i++)
        {
            if (stars[i] != null) stars[i].sprite = got[i] ? starOn : starOff;
            if (i < labels.Length && labels[i] != null)
                labels[i].color = got[i] ? Earned : Missed;
        }

        SetLabels();
        LastStars = count;
        StarRating.SaveBest(SceneManager.GetActiveScene().name, count);
    }

    private bool[] Evaluate()
    {
        var vehicle = FindFirstObjectByType<VehicleHealth>();
        int healthPercent = vehicle != null && vehicle.MaxHealth > 0
            ? vehicle.CurrentHealth * 100 / vehicle.MaxHealth
            : 0;

        return new[]
        {
            true,
            healthPercent >= healthPercentForStar2,
            GameFeedback.ZombieKills >= zombiesForStar3,
        };
    }

    private void SetLabels()
    {
        if (labels == null || labels.Length < 3) return;
        if (labels[0] != null) labels[0].text = Loc.T("star_win");
        if (labels[1] != null) labels[1].text = string.Format(Loc.T("star_health"), healthPercentForStar2);
        if (labels[2] != null) labels[2].text = string.Format(Loc.T("star_zombies"), zombiesForStar3);
    }
}
