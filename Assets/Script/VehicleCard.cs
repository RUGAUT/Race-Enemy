using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Carte de vehicule du menu : affiche les stats en barres colorees + chiffres, verrouille/deverrouille
/// (achat en pieces) et transmet le choix a MenuVehicleSelection quand le vehicule est disponible.
/// </summary>
public class VehicleCard : MonoBehaviour
{
    [SerializeField] private int vehicleIndex;
    [SerializeField] private MenuVehicleSelection selection;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private TextMeshProUGUI priceLabel;

    [Header("Stats : vie, vitesse, tir (dans cet ordre)")]
    [SerializeField] private RectTransform[] statFills;
    [SerializeField] private TextMeshProUGUI[] statValues;
    [SerializeField] private float barWidth = 190f;

    private static readonly Color Locked = new Color(0.35f, 0.35f, 0.35f, 1f);
    private Coroutine warning;

    private void OnEnable()
    {
        Economy.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        Economy.Changed -= Refresh;
    }

    /// <summary>A relier au clic du bouton de la carte.</summary>
    public void Click()
    {
        if (Economy.IsUnlocked(vehicleIndex) || Economy.TryBuy(vehicleIndex))
        {
            selection.ChooseVehicle(vehicleIndex);
            return;
        }

        if (warning != null) StopCoroutine(warning);
        warning = StartCoroutine(NotEnoughCoins());
    }

    private void Refresh()
    {
        var info = VehicleCatalog.Get(vehicleIndex);
        bool unlocked = Economy.IsUnlocked(vehicleIndex);

        if (lockOverlay != null) lockOverlay.SetActive(!unlocked);
        if (icon != null) icon.color = unlocked ? Color.white : Locked;

        if (priceLabel != null)
        {
            priceLabel.color = Color.white;
            priceLabel.text = string.Format(Loc.T("price"), info.price);
        }

        SetStat(0, info.health / VehicleCatalog.MaxHealth, Mathf.RoundToInt(VehicleCatalog.BaseHealth * info.health).ToString());
        SetStat(1, info.speed / VehicleCatalog.MaxSpeed, Mathf.RoundToInt(VehicleCatalog.BaseSpeed * info.speed).ToString());
        SetStat(2, info.fireRate / VehicleCatalog.MaxFireRate,
            (VehicleCatalog.BaseShotsPerSecond * info.fireRate).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "/s");
    }

    private void SetStat(int i, float ratio, string text)
    {
        if (statFills != null && i < statFills.Length && statFills[i] != null)
            statFills[i].sizeDelta = new Vector2(Mathf.Max(14f, barWidth * Mathf.Clamp01(ratio)), statFills[i].sizeDelta.y);
        if (statValues != null && i < statValues.Length && statValues[i] != null)
            statValues[i].text = text;
    }

    private IEnumerator NotEnoughCoins()
    {
        if (priceLabel == null) yield break;
        priceLabel.color = new Color32(0xFF, 0x5A, 0x4A, 255);
        priceLabel.text = Loc.T("need_coins");
        yield return new WaitForSecondsRealtime(1.2f);
        Refresh();
    }
}
