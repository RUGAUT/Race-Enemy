using UnityEngine;

public class GameVehicleActivator : MonoBehaviour
{
    [Header("Véhicules dans la scène de jeu")]
    [Tooltip("Place ici TOUS tes véhicules DANS LE MÊME ORDRE que les index de tes boutons du menu")]
    [SerializeField] private GameObject[] inGameVehicles;

    // On utilise Awake (qui se lance avant Start) pour activer le véhicule 
    // avant que les autres scripts (comme la caméra) ne commencent à le chercher.
    private void Awake()
    {
        // On récupère le choix fait dans le menu. 
        // S'il n'y a pas de choix (ex: on lance la scène InGame directement pour tester), ça prendra 0.
        int selectedIndex = PlayerPrefs.GetInt("SelectedVehicleIndex", 0);

        // On parcourt la liste de tous les véhicules
        for (int i = 0; i < inGameVehicles.Length; i++)
        {
            if (inGameVehicles[i] != null)
            {
                // Active uniquement si l'index 'i' correspond au choix sauvegardé, sinon désactive
                inGameVehicles[i].SetActive(i == selectedIndex);
            }
        }
    }
}