using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Active les vehicules supplementaires (index du menu >= firstIndex) sans toucher a GameVehicleActivator.
/// Branche aussi, au lancement, les boutons tactiles Tir et Grenade sur le vehicule actif.
/// </summary>
public class ExtraVehicleActivator : MonoBehaviour
{
    [SerializeField] private GameObject[] extraVehicles;
    [Tooltip("Index du menu du premier vehicule de la liste (les 3 vehicules d'origine occupent 0, 1 et 2).")]
    [SerializeField] private int firstIndex = 3;
    [SerializeField] private string fireButtonName = "Button Fire";
    [SerializeField] private string grenadeButtonName = "Button Grenade";

    private void Awake()
    {
        int selected = PlayerPrefs.GetInt("SelectedVehicleIndex", 0);

        for (int i = 0; i < extraVehicles.Length; i++)
        {
            var rig = extraVehicles[i];
            if (rig == null) continue;

            bool active = firstIndex + i == selected;
            rig.SetActive(active);
            if (active) HookButtons(rig);
        }
    }

    private void HookButtons(GameObject rig)
    {
        var attack = rig.GetComponentInChildren<VehicleAttack>(true);
        if (attack == null) return;

        foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (button.name == fireButtonName) button.onClick.AddListener(attack.Fire);
            else if (button.name == grenadeButtonName) button.onClick.AddListener(attack.FireGrenade);
        }
    }
}
