using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Gestor central de input. Crea y mantiene los InputAction del juego,
/// aplicando los keybindings configurados en DataManager.
/// Debe estar en la escena del menú principal (con DontDestroyOnLoad).
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // ── Acciones expuestas ─────────────────────────────────────────────────
    public InputAction Move  { get; private set; }  // Vector2 composite
    public InputAction Focus { get; private set; }  // Button
    public InputAction Shoot { get; private set; }  // Button
    public InputAction Bomb  { get; private set; }  // Button
    public InputAction Pause { get; private set; }  // Button (Escape, no remapeable)

    // ── Lifecycle ──────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildActions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForDataManagerAndRefresh());
    }

    private IEnumerator WaitForDataManagerAndRefresh()
    {
        while (DataManager.Instance == null)
            yield return null;

        RefreshBindings();
    }

    private void OnDestroy()
    {
        Move?.Dispose();
        Focus?.Dispose();
        Shoot?.Dispose();
        Bomb?.Dispose();
        Pause?.Dispose();
    }

    // ── Construcción de acciones con bindings por defecto ──────────────────
    private void BuildActions()
    {
        // Movimiento: composite 2DVector — índices: 0=composite, 1=Up, 2=Down, 3=Left, 4=Right
        Move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        Move.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/w")
            .With("Down",  "<Keyboard>/s")
            .With("Left",  "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        Focus = new InputAction("Focus", InputActionType.Button, "<Keyboard>/leftShift");
        Shoot = new InputAction("Shoot", InputActionType.Button, "<Keyboard>/space");
        Bomb  = new InputAction("Bomb",  InputActionType.Button, "<Keyboard>/b");
        // Escape no es remapeable; se mantiene fijo.
        Pause = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");

        Move.Enable();
        Focus.Enable();
        Shoot.Enable();
        Bomb.Enable();
        Pause.Enable();
    }

    // ── Aplicación de keybindings desde DataManager ────────────────────────
    /// <summary>
    /// Aplica los keybindings guardados en DataManager.
    /// Llamar al iniciar y cada vez que el jugador guarda ajustes de controles.
    /// </summary>
    public void RefreshBindings()
    {
        if (DataManager.Instance == null) return;

        SettingsData s = DataManager.Instance.SaveData.settings;

        // Move composite: se reemplazan los cuatro bindings de parte
        Move.Disable();
        Move.ApplyBindingOverride(1, ToPath(s.moveUpKey));
        Move.ApplyBindingOverride(2, ToPath(s.moveDownKey));
        Move.ApplyBindingOverride(3, ToPath(s.moveLeftKey));
        Move.ApplyBindingOverride(4, ToPath(s.moveRightKey));
        Move.Enable();

        ApplyButtonOverride(Focus, s.focusKey);
        ApplyButtonOverride(Shoot, s.shootKey);
        ApplyButtonOverride(Bomb,  s.bombKey);
        // Pause (Escape) no se remapea
    }

    private void ApplyButtonOverride(InputAction action, string keyName)
    {
        action.Disable();
        action.ApplyBindingOverride(0, ToPath(keyName));
        action.Enable();
    }

    // ── Conversión nombre → path de InputSystem ────────────────────────────
    /// <summary>
    /// Convierte el nombre de tecla guardado en DataManager al path de
    /// Unity InputSystem. Admite tanto el formato antiguo (mayúsculas: "W",
    /// "LEFTSHIFT") como el nuevo (camelCase: "w", "leftShift").
    /// </summary>
    private string ToPath(string keyName)
    {
        if (string.IsNullOrEmpty(keyName)) return string.Empty;
        return $"<Keyboard>/{NormalizeKeyName(keyName)}";
    }

    private string NormalizeKeyName(string keyName)
    {
        // Normalizar a minúsculas para comparar, luego devolver el camelCase
        // que espera Unity InputSystem para teclas multi-palabra.
        return keyName.ToLower() switch
        {
            "leftshift"   => "leftShift",
            "rightshift"  => "rightShift",
            "leftctrl"    => "leftCtrl",
            "rightctrl"   => "rightCtrl",
            "leftalt"     => "leftAlt",
            "rightalt"    => "rightAlt",
            "capslock"    => "capsLock",
            "numlock"     => "numLock",
            "scrolllock"  => "scrollLock",
            "pageup"      => "pageUp",
            "pagedown"    => "pageDown",
            "arrowup"     => "upArrow",
            "uparrow"     => "upArrow",
            "arrowdown"   => "downArrow",
            "downarrow"   => "downArrow",
            "arrowleft"   => "leftArrow",
            "leftarrow"   => "leftArrow",
            "arrowright"  => "rightArrow",
            "rightarrow"  => "rightArrow",
            "enter"       => "enter",
            "return"      => "enter",
            "numpadenter" => "numpadEnter",
            "backspace"   => "backspace",
            "delete"      => "delete",
            "insert"      => "insert",
            "home"        => "home",
            "end"         => "end",
            "tab"         => "tab",
            "escape"      => "escape",
            "space"       => "space",
            // Tecla de un solo carácter: simplemente minúscula
            var s         => s
        };
    }
}
