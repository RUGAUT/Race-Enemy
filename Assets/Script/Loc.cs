using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Petit systeme de traduction FR / EN.
/// Loc.T("cle") renvoie le texte dans la langue courante (sauvegardee dans PlayerPrefs "Language").
/// Pour ajouter une langue : ajouter un code dans Languages et une colonne dans Table.
/// </summary>
public static class Loc
{
    public const string French = "fr";
    public const string English = "en";

    private const string PrefKey = "Language";
    private static string current;

    public static event Action Changed;

    public static string Language
    {
        get
        {
            if (current == null)
                current = PlayerPrefs.GetString(PrefKey, Application.systemLanguage == SystemLanguage.French ? French : English);
            return current;
        }
    }

    public static void SetLanguage(string code)
    {
        if (code == Language) return;
        current = code;
        PlayerPrefs.SetString(PrefKey, code);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public static string T(string key)
    {
        if (Table.TryGetValue(key, out var entry))
            return Language == French ? entry.fr : entry.en;
        return key;
    }

    // cle -> (francais, anglais)
    private static readonly Dictionary<string, (string fr, string en)> Table = new Dictionary<string, (string, string)>
    {
        // --- Menu principal ---
        { "play", ("JOUER", "PLAY") },
        { "options", ("OPTIONS", "OPTIONS") },
        { "credits", ("CRÉDITS", "CREDITS") },
        { "vehicles", ("VÉHICULES", "VEHICLES") },
        { "back", ("RETOUR", "BACK") },
        { "alpha", ("ALPHA", "ALPHA") },
        { "select_level", ("CHOISIR LE NIVEAU", "SELECT LEVEL") },
        { "select_vehicle", ("CHOISIR LE VÉHICULE", "SELECT VEHICLE") },
        { "level_1", ("NIVEAU 1", "LEVEL 1") },
        { "level_2", ("NIVEAU 2", "LEVEL 2") },
        { "level_3", ("NIVEAU 3", "LEVEL 3") },
        { "soon", ("BIENTÔT", "COMING SOON") },
        { "zombie_record", ("ZOMBIES ÉLIMINÉS (RECORD)", "ZOMBIES KILLED (RECORD)") },
        { "missions", ("MISSIONS", "MISSIONS") },
        { "volume", ("VOLUME", "VOLUME") },
        { "language", ("LANGUE", "LANGUAGE") },
        { "credits_text", ("RACE ENEMY\n\nMerci d'avoir joué !", "RACE ENEMY\n\nThanks for playing!") },
        { "loading", ("Chargement...", "Loading...") },

        // --- En jeu ---
        { "victory", ("VICTOIRE !", "VICTORY!") },
        { "defeat", ("ÉCHEC", "DEFEAT") },
        { "retry", ("REJOUER", "RETRY") },
        { "menu", ("MENU", "MENU") },
        { "resume", ("REPRENDRE", "RESUME") },
        { "pause", ("PAUSE", "PAUSE") },
        { "record_label", ("RECORD :", "RECORD:") },
        { "zombies_killed", ("Zombies Tués", "Zombies Killed") },
        { "distance", ("Distance", "Distance") },
        { "distance_score", ("Distance Score", "Distance Score") },
        { "zombie_score", ("Zombie Score", "Zombie Score") },

        { "boss_incoming", ("BOSS EN APPROCHE !", "BOSS INCOMING!") },
        { "combo", ("COMBO", "COMBO") },
        { "star_win", ("Victoire", "Victory") },
        { "star_health", ("Vie {0} %+", "Health {0}%+") },
        { "star_zombies", ("{0}+ zombies", "{0}+ zombies") },
        { "ability_supershot", ("SUPER TIR !", "SUPER SHOT!") },
        { "ability_turbo", ("TURBO !", "TURBO!") },
        { "ability_heal", ("SOIN !", "HEAL!") },
        { "ability_shield", ("BOUCLIER !", "SHIELD!") },
        { "ability_shockwave", ("ONDE DE CHOC !", "SHOCKWAVE!") },
        { "ability_ready", ("CAPACITÉ PRÊTE", "ABILITY READY") },
        { "stat_hp", ("PV", "HP") },
        { "stat_speed", ("VIT", "SPD") },
        { "stat_fire", ("TIR", "FIRE") },
        { "price", ("{0}", "{0}") },
        { "need_coins", ("PAS ASSEZ DE PIÈCES", "NOT ENOUGH COINS") },
        { "coins", ("PIÈCES", "COINS") },
        { "coins_earned", ("+{0}", "+{0}") },

        // --- Aide des controles ---
        { "controls", ("CONTRÔLES", "CONTROLS") },
        { "close", ("FERMER", "CLOSE") },
        { "pc_move", ("Flèches / ZQSD : se déplacer", "Arrow keys / WASD: move") },
        { "pc_fire", ("Ctrl / J : tirer", "Ctrl / J: shoot") },
        { "pc_grenade", ("Espace / G : grenade", "Space / G: grenade") },
        { "pc_pause", ("Échap / P : pause", "Esc / P: pause") },
        { "touch_move", ("Joystick (gauche) : se déplacer", "Left joystick: move") },
        { "touch_fire", ("Bouton de tir : tirer", "Fire button: shoot") },
        { "touch_grenade", ("Bouton grenade : lancer", "Grenade button: throw") },
        { "touch_pause", ("Engrenage (haut droite) : pause", "Gear icon (top right): pause") },
    };
}
