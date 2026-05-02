using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central manager for menu panels. Holds a stack so "Back" returns to whatever
/// was open before. Each panel needs a UIMenu component and a unique MenuId.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public enum MenuId
    {
        None,
        MainMenu,   // The Shop / Settings / Credits button container shown before the run starts
        Shop,
        Settings,
        Credits,
        Pause,      // Opened mid-run via the pause button
        GameOver
    }

    [Header("Menus In Scene")]
    [Tooltip("Drag every UIMenu in the scene here. Each must have a unique MenuId.")]
    public List<UIMenu> menus = new List<UIMenu>();

    [Header("Initial State")]
    [Tooltip("Which menu is open when the scene loads. Set to None for no menu.")]
    public MenuId startingMenu = MenuId.MainMenu;

    private Dictionary<MenuId, UIMenu> lookup = new Dictionary<MenuId, UIMenu>();
    private Stack<MenuId> history = new Stack<MenuId>();

    /// <summary>The menu currently on top of the stack, or None if nothing is open.</summary>
    public MenuId Current => history.Count > 0 ? history.Peek() : MenuId.None;

    /// <summary>True if any menu is open. Use this from gameplay code to gate input.</summary>
    public bool IsAnyMenuOpen => history.Count > 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Build the lookup table.
        foreach (var menu in menus)
        {
            if (menu == null) continue;
            if (lookup.ContainsKey(menu.menuId))
            {
                Debug.LogWarning($"UIManager: Duplicate MenuId '{menu.menuId}' on {menu.name}. Skipping.", menu);
                continue;
            }
            lookup[menu.menuId] = menu;
        }

        // Reset every menu to a closed state without animation.
        foreach (var menu in menus)
        {
            if (menu != null) menu.InstantHide();
        }

        // Open the starting menu instantly (no fade-in on the first frame).
        if (startingMenu != MenuId.None)
        {
            UIMenu starting = Get(startingMenu);
            if (starting != null)
            {
                starting.InstantShow();
                history.Push(startingMenu);
            }
            else
            {
                Debug.LogWarning($"UIManager: Starting menu '{startingMenu}' is not in the menus list.");
            }
        }
    }

    /// <summary>Open a menu and push it onto the history stack. Hides the previous one.</summary>
    public void Open(MenuId id)
    {
        if (id == MenuId.None) return;

        UIMenu next = Get(id);
        if (next == null)
        {
            Debug.LogWarning($"UIManager: No menu registered for {id}.");
            return;
        }

        // Hide whatever's currently visible.
        if (history.Count > 0)
        {
            UIMenu top = Get(history.Peek());
            if (top != null && top.gameObject.activeInHierarchy) top.Hide(); else if (top != null) top.InstantHide();
        }

        history.Push(id);
        next.Show();
    }

    /// <summary>
    /// Close the topmost menu and reveal the one underneath. Wire this directly
    /// to back-button onClick events.
    /// </summary>
    public void Back()
    {
        if (history.Count == 0) return;

        UIMenu closing = Get(history.Pop());
        if (closing != null) closing.Hide();

        // Re-show the previous menu, if any.
        if (history.Count > 0)
        {
            UIMenu prev = Get(history.Peek());
            if (prev != null) prev.Show();
        }
    }

    /// <summary>Close every open menu. Useful when starting a run.</summary>
    public void CloseAll()
    {
        while (history.Count > 0)
        {
            UIMenu m = Get(history.Pop());
            if (m != null) m.InstantHide();
        }
    }

    /// <summary>String-based open, for cases where you'd rather wire a Button.onClick
    /// to a single UnityEvent with a string parameter.</summary>
    public void OpenByName(string idName)
    {
        if (System.Enum.TryParse(idName, out MenuId id))
            Open(id);
        else
            Debug.LogWarning($"UIManager: Unknown menu id '{idName}'.");
    }

    private UIMenu Get(MenuId id)
    {
        lookup.TryGetValue(id, out UIMenu menu);
        return menu;
    }
}
