using System.Reflection;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Controles clavier pour PC. Se cree tout seul au lancement et n'agit que dans les scenes de jeu
/// (quand un GameManager existe). Ne modifie aucun script existant :
/// le clavier pilote le joystick virtuel que lit deja PlayerControls.
///
///   Deplacement : fleches ou WASD (ZQSD sur un clavier AZERTY)
///                 haut/bas = deplacement lateral, gauche/droite = braquage (comme le joystick)
///   Tir         : Ctrl gauche ou J (pour les vehicules sans tir automatique)
///   Grenade     : Espace ou G
///   Capacite    : E ou Maj gauche
///   Pause       : Echap ou P
/// Le tir est automatique. Les menus se jouent a la souris.
/// </summary>
public class PCControls : MonoBehaviour
{
    private VehicleAttack attack;
    private const int KeyboardPointerId = 99;
    private bool joystickHeld;
    private static FieldInfo pausePanelField;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        var go = new GameObject("PCControls");
        DontDestroyOnLoad(go);
        go.AddComponent<PCControls>();
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || GameManager.Instance == null) return;

        if (kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame)
            TogglePause();

        if (Time.timeScale <= 0f) return;

        if (kb.spaceKey.wasPressedThisFrame || kb.gKey.wasPressedThisFrame)
            ThrowGrenade();

        if (kb.eKey.wasPressedThisFrame || kb.leftShiftKey.wasPressedThisFrame)
            VehicleAbility.Use();

        // Tir manuel (necessaire pour les vehicules sans tir automatique). Fire() gere deja la cadence.
        if (kb.leftCtrlKey.isPressed || kb.jKey.isPressed)
            Shoot();
    }

    // On appuie sur le joystick virtuel comme le ferait un doigt (appel de son OnPointerDown public).
    // Quand toutes les touches sont relachees, on "leve le doigt" : le joystick revient au centre.
    private void LateUpdate()
    {
        var kb = Keyboard.current;
        if (kb == null || GameManager.Instance == null) return;

        Vector2 dir = Vector2.zero;
        if (Time.timeScale > 0f)
        {
            if (kb.upArrowKey.isPressed || kb.wKey.isPressed) dir.y += 1f;
            if (kb.downArrowKey.isPressed || kb.sKey.isPressed) dir.y -= 1f;
            if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) dir.x += 1f;
            if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) dir.x -= 1f;
        }

        Terresquall.VirtualJoystick joystick = Terresquall.VirtualJoystick.GetInstance(0);
        if (joystick == null) { joystickHeld = false; return; }

        if (dir != Vector2.zero)
        {
            dir = Vector2.ClampMagnitude(dir, 1f);
            Vector2 target = (Vector2)joystick.transform.position + dir * joystick.GetRadius();
            // pointerId > -1 : le joystick l'interprete comme un doigt et ne suit pas la souris.
            joystick.OnPointerDown(new PointerEventData(EventSystem.current) { position = target, pointerId = KeyboardPointerId });
            joystickHeld = true;
        }
        else if (joystickHeld)
        {
            joystick.OnPointerUp(new PointerEventData(null));
            joystickHeld = false;
        }
    }

    private void Shoot()
    {
        if (attack == null || !attack.gameObject.activeInHierarchy)
            attack = FindFirstObjectByType<VehicleAttack>();
        if (attack != null) attack.Fire();
    }

    private void ThrowGrenade()
    {
        if (attack == null || !attack.gameObject.activeInHierarchy)
            attack = FindFirstObjectByType<VehicleAttack>();
        if (attack != null) attack.FireGrenade();
    }

    private void TogglePause()
    {
        if (ControlsHelpPanel.IsOpen) return; // Echap ferme l'aide (voir ControlsHelpPanel), pas la pause

        var gm = GameManager.Instance;
        if (IsPausePanelOpen(gm))
            gm.OnResumeButtonClick();
        else if (Time.timeScale > 0f)
            gm.OnPauseButtonClick();
    }

    // Lit (sans le modifier) le panel de pause du GameManager pour savoir si le jeu est en pause.
    private static bool IsPausePanelOpen(GameManager gm)
    {
        if (pausePanelField == null)
            pausePanelField = typeof(GameManager).GetField("pauseMenuPanel", BindingFlags.Instance | BindingFlags.NonPublic);
        var panel = pausePanelField != null ? pausePanelField.GetValue(gm) as GameObject : null;
        return panel != null && panel.activeInHierarchy;
    }
}
