using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ajoute un composant LocalizedText sur les textes de la scene ouverte dont le texte francais est connu.
/// Les textes ecrits par les scripts (score, distance, chargement) sont traduits dans le code, pas ici.
/// Ne modifie aucun texte existant : le texte francais reste celui de la scene.
/// </summary>
public static class SceneLocalizer
{
    // texte francais present dans la scene -> cle de Loc.cs
    private static readonly Dictionary<string, string> Map = new Dictionary<string, string>
    {
        { "JOUER", "play" }, { "OPTIONS", "options" }, { "CRÉDITS", "credits" }, { "VÉHICULES", "vehicles" },
        { "RETOUR", "back" }, { "ALPHA", "alpha" }, { "CHOISIR LE NIVEAU", "select_level" },
        { "CHOISIR LE VÉHICULE", "select_vehicle" }, { "NIVEAU 1", "level_1" }, { "NIVEAU 2", "level_2" },
        { "NIVEAU 3", "level_3" }, { "BIENTÔT", "soon" }, { "ZOMBIES ÉLIMINÉS (RECORD)", "zombie_record" },
        { "MISSIONS", "missions" }, { "VOLUME", "volume" }, { "LANGUE", "language" },
        { "RACE ENEMY\n\nMerci d'avoir joué !", "credits_text" },
        { "VICTOIRE !", "victory" }, { "ÉCHEC", "defeat" }, { "REJOUER", "retry" }, { "MENU", "menu" },
        { "REPRENDRE", "resume" }, { "PAUSE", "pause" }, { "RECORD :", "record_label" },
        { "FERMER", "close" }, { "CONTRÔLES", "controls" },
        { "Flèches / ZQSD : se déplacer", "pc_move" }, { "Ctrl / J : tirer", "pc_fire" },
        { "Espace / G : grenade", "pc_grenade" }, { "Échap / P : pause", "pc_pause" },
        { "Joystick (gauche) : se déplacer", "touch_move" }, { "Bouton de tir : tirer", "touch_fire" },
        { "Bouton grenade : lancer", "touch_grenade" }, { "Engrenage (haut droite) : pause", "touch_pause" },
    };

    [MenuItem("Race Enemy/Localize Scene Texts")]
    public static void Apply()
    {
        var scene = SceneManager.GetActiveScene();
        int added = 0;
        var unmapped = new StringBuilder();

        foreach (var t in Resources.FindObjectsOfTypeAll<TMP_Text>())
        {
            if (!t.gameObject.scene.IsValid() || t.gameObject.scene != scene) continue;
            if (t.GetComponent<LocalizedText>() != null) continue;

            if (Map.TryGetValue(t.text.Trim(), out string key))
            {
                var lt = t.gameObject.AddComponent<LocalizedText>();
                var so = new SerializedObject(lt);
                so.FindProperty("key").stringValue = key;
                so.ApplyModifiedPropertiesWithoutUndo();
                added++;
            }
            else if (HasLetters(t.text))
            {
                unmapped.AppendLine("  - " + t.transform.parent?.name + "/" + t.name + " = \"" + t.text.Replace("\n", "\\n") + "\"");
            }
        }

        if (added > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log("[Localize] " + scene.name + " : " + added + " textes traduisibles ajoutes.\nTextes non traduits (probablement ecrits par un script ou deja neutres) :\n" + unmapped);
    }

    private static bool HasLetters(string s)
    {
        foreach (char c in s) if (char.IsLetter(c)) return true;
        return false;
    }
}
