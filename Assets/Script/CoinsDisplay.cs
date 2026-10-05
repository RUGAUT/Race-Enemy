using TMPro;
using UnityEngine;

/// <summary>Affiche le nombre de pieces du joueur (mis a jour a chaque achat ou gain).</summary>
public class CoinsDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    private void OnEnable()
    {
        Economy.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        Economy.Changed -= Refresh;
    }

    private void Refresh()
    {
        if (label != null) label.text = Economy.Coins.ToString();
    }
}
