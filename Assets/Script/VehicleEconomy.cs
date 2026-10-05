using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Economie fictive : pieces, achat des vehicules (PlayerPrefs "Coins" et "VehicleUnlocked_i").
/// </summary>
public static class Economy
{
    public const string CoinsKey = "Coins";

    /// <summary>Declenche a chaque changement de pieces ou de vehicule debloque.</summary>
    public static event Action Changed;

    public static int Coins => PlayerPrefs.GetInt(CoinsKey, 0);

    public static void AddCoins(int amount)
    {
        if (amount <= 0) return;
        PlayerPrefs.SetInt(CoinsKey, Coins + amount);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public static bool IsUnlocked(int vehicleIndex)
    {
        if (VehicleCatalog.Get(vehicleIndex).price <= 0) return true;
        return PlayerPrefs.GetInt("VehicleUnlocked_" + vehicleIndex, 0) == 1;
    }

    /// <summary>Achete le vehicule si possible. Renvoie vrai s'il est debloque apres l'appel.</summary>
    public static bool TryBuy(int vehicleIndex)
    {
        if (IsUnlocked(vehicleIndex)) return true;
        int price = VehicleCatalog.Get(vehicleIndex).price;
        if (Coins < price) return false;

        PlayerPrefs.SetInt(CoinsKey, Coins - price);
        PlayerPrefs.SetInt("VehicleUnlocked_" + vehicleIndex, 1);
        PlayerPrefs.Save();
        Changed?.Invoke();
        return true;
    }
}

/// <summary>
/// Prix et statistiques de chaque vehicule (index = ordre du menu).
/// Les statistiques sont des multiplicateurs appliques au lancement de la partie
/// sur les valeurs deja presentes sur le vehicule : l'inspecteur n'est jamais modifie.
/// </summary>
public static class VehicleCatalog
{
    public struct Info
    {
        public int price;
        public float health;   // multiplicateur de vie max
        public float speed;    // multiplicateur de vitesse de deplacement
        public float fireRate; // multiplicateur de cadence de tir (2 = deux fois plus de tirs)
        public float grenade;  // multiplicateur du delai de recharge des grenades (0.5 = deux fois plus vite)

        public Info(int price, float health, float speed, float fireRate, float grenade)
        { this.price = price; this.health = health; this.speed = speed; this.fireRate = fireRate; this.grenade = grenade; }
    }

    // 0 Muscle, 1 HotRod, 2 CarWhite, 3 Buggy, 4 Ute
    private static readonly Info[] Vehicles =
    {
        new Info(0,    1.0f, 1.0f,  1.0f,  1.0f),
        new Info(1500, 1.5f, 1.1f,  1.3f,  0.8f),
        new Info(500,  1.2f, 1.0f,  1.15f, 1.0f),
        new Info(0,    0.8f, 1.35f, 1.1f,  1.0f),
        new Info(3000, 2.2f, 0.9f,  1.6f,  0.6f),
    };

    // Valeurs de base d'un vehicule (celles de la Muscle) servant a afficher des chiffres lisibles dans le menu
    public const int BaseHealth = 1000;
    public const float BaseSpeed = 10f;
    public const float BaseShotsPerSecond = 2f;

    // Plus grand multiplicateur de chaque stat : sert a remplir les barres
    public const float MaxHealth = 2.2f, MaxSpeed = 1.35f, MaxFireRate = 1.6f;

    public static int Count => Vehicles.Length;

    public static Info Get(int index)
    {
        if (index < 0 || index >= Vehicles.Length) return Vehicles[0];
        return Vehicles[index];
    }
}

/// <summary>
/// Applique les statistiques du vehicule choisi, une fois la scene chargee et avant les Start
/// (VehicleHealth remplit sa vie dans Start). Aucune valeur d'inspecteur n'est modifiee.
/// </summary>
public static class VehicleStatsApplier
{
    public static void Apply()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        int index = PlayerPrefs.GetInt("SelectedVehicleIndex", 0);
        var stats = VehicleCatalog.Get(index);

        var health = player.GetComponent<VehicleHealth>();
        if (health != null) Scale(health, "maxHealth", stats.health, true);

        var controls = player.GetComponent<PlayerControls>();
        if (controls != null) Scale(controls, "speed", stats.speed, false);

        var attack = player.GetComponent<VehicleAttack>();
        if (attack != null)
        {
            Scale(attack, "fireRate", 1f / stats.fireRate, false);
            Scale(attack, "autoFireRate", 1f / stats.fireRate, false);
            Scale(attack, "grenadeCooldown", stats.grenade, false);
        }
    }

    private static void Scale(object target, string field, float factor, bool integer)
    {
        var f = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (f == null) return;
        if (f.FieldType == typeof(int)) f.SetValue(target, Mathf.RoundToInt((int)f.GetValue(target) * factor));
        else if (f.FieldType == typeof(float)) f.SetValue(target, (float)f.GetValue(target) * factor);
    }
}
