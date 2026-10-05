using UnityEngine;

/// <summary>
/// Sauvegarde des etoiles par niveau (PlayerPrefs "Stars_<nom de la scene>", meilleur resultat conserve).
/// </summary>
public static class StarRating
{
    public static string KeyFor(string sceneName) => "Stars_" + sceneName;

    public static int Get(string sceneName)
    {
        return PlayerPrefs.GetInt(KeyFor(sceneName), 0);
    }

    /// <summary>Enregistre si c'est mieux que le meilleur resultat. Renvoie le meilleur resultat.</summary>
    public static int SaveBest(string sceneName, int stars)
    {
        int best = Mathf.Max(Get(sceneName), stars);
        PlayerPrefs.SetInt(KeyFor(sceneName), best);
        PlayerPrefs.Save();
        return best;
    }
}
