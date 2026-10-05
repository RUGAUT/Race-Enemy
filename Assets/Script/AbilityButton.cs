using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bouton de capacite a l'ecran : icone selon la capacite du vehicule, jauge de recharge circulaire,
/// icone attenuee tant que la capacite n'est pas prete.
/// </summary>
public class AbilityButton : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image cooldownFill;
    [Tooltip("Dans l'ordre : SuperShot, Turbo, Heal, Shield, Shockwave")]
    [SerializeField] private Sprite[] icons;

    private void OnEnable()
    {
        ApplyIcon(AbilityCatalog.Get(PlayerPrefs.GetInt("SelectedVehicleIndex", 0)).type);
    }

    private void ApplyIcon(AbilityType type)
    {
        if (icon != null && icons != null && (int)type < icons.Length && icons[(int)type] != null)
            icon.sprite = icons[(int)type];
    }

    /// <summary>A relier au clic du bouton.</summary>
    public void Use()
    {
        VehicleAbility.Use();
    }

    private void Update()
    {
        var ability = VehicleAbility.Instance;
        if (ability == null) return;

        ApplyIcon(ability.Type);

        float progress = ability.CooldownProgress;
        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = progress;
            cooldownFill.enabled = progress > 0f;
        }
        if (icon != null) icon.color = ability.Ready ? Color.white : new Color(1f, 1f, 1f, 0.55f);
    }
}
