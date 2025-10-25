using System.Collections.Generic;
using cowsins;
using cowsins.Inventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MapManager : MonoBehaviour 
{
    [SerializeField] 
    private InteractManager interactManager;
    [SerializeField]
    private GameObject mapObject;
    [SerializeField]
    private MapContextMenu contextMenu;
    [SerializeField] 
    private AudioClip openMapSFX;
    [SerializeField]
    private AudioClip highlightStationSFX;
    [SerializeField]
    private List<MapStation> stations;

    public UnityEvent onMapOpen, onMapClose;

    private PlayerMovement playerMovement;
    private PlayerStats playerStats;

    private bool isMapOpen = false;
    private bool isInventoryOpen = false;

    private MapStation highlightedStation;
    private MapStation selectedStation;

    public static MapManager instance;

    private void Awake()
    {
        // Handle singleton
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }

    public void SetInventoryState(bool state) => isInventoryOpen = state;

    public void SelectStation(MapStation station)
    {
        selectedStation?.Unselect();
        selectedStation = station;
        selectedStation.Select();
    }

    public void PointerEnter(MapStation station)
    {
        ShowContextMenu(station);
        highlightedStation?.Unhighlight();
        highlightedStation = station;
        highlightedStation.Highlight();
        SoundManager.Instance.PlaySound(highlightStationSFX, 0, 0, false, 0);
    }

    public void PointerExit()
    {
        contextMenu.HideContextMenu();
        highlightedStation.Unhighlight();
        highlightedStation = null;
    }

    public void PointerClick(MapStation station)
    {
        SelectStation(station);
    }

    private void ShowContextMenu(MapStation station)
    {
        contextMenu.ShowContextMenu(station, (Vector3)Mouse.current?.position.ReadValue());
    }

    private void Start()
    {
        playerMovement = interactManager.GetComponent<PlayerMovement>();
        playerStats = interactManager.GetComponent<PlayerStats>();

        contextMenu.Init(this);

        InputManager.onTogglePause += CloseMap;
        InputManager.onMapOpenPressed += ToggleMapVisibility;

        InventoryProManager.instance.Events.onOpenInventory.AddListener(() => { SetInventoryState(true); });
        InventoryProManager.instance.Events.onCloseInventory.AddListener(() => { SetInventoryState(false); });

        for (int i = 0;i<stations.Count;i++)
        {
            stations[i].Init(this, i);
        }
    }

    private void ToggleMapVisibility()
    {
        if (PauseMenu.isPaused || playerStats.IsDead) return;
        if (!isInventoryOpen)
        {
            if (!isMapOpen && (!interactManager.inspecting && interactManager.realtimeAttachmentCustomization || !interactManager.realtimeAttachmentCustomization)) OpenMap();
            else CloseMap();
        }
    }

    private void OpenMap()
    {
        isMapOpen = true;

        InputManager.ToggleUIControls(true);
        PauseMenu.Instance.stats.LoseControl();
        playerMovement.StopSpeedlines();
        UIController.instance.UnlockMouse();
        UIController.instance.crosshair.SetVisibility(false);

        mapObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(mapObject);
        SoundManager.Instance.PlaySound(openMapSFX, 0, 0, false, 0);

        onMapOpen?.Invoke();
    }

    private void CloseMap()
    {
        isMapOpen = false;
        mapObject.SetActive(false);
        contextMenu.HideContextMenu();
        PauseMenu.Instance?.stats?.CheckIfCanGrantControl();
        UIController.instance.LockMouse();
        UIController.instance.crosshair.SetVisibility(true);

        InputManager.ToggleGameControls(true);
        InputManager.ToggleUIControls(false);

        onMapClose?.Invoke();
    }
}
