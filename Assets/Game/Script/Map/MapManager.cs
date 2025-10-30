using System.Collections.Generic;
using cowsins;
using cowsins.Inventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MapManager : MonoBehaviour
{
    [SerializeField] private GameObject mapObject;
    [SerializeField] private MapContextMenu contextMenu;
    [SerializeField] private AudioClip openMapSFX;
    [SerializeField] private AudioClip highlightStationSFX;
    [SerializeField] private List<MapStation> stations;
    
    private InteractManager interactManager;
    private bool isMapOpen = false;
    private MapStation highlightedStation;
    private MapStation selectedStation;
    
    public UnityEvent onMapOpen, onMapClose;

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
        interactManager = G.InteractManager;
        
        InputManager.onTogglePause += CloseMap;
        InputManager.onMapOpenPressed += ToggleMapVisibility;

        InventoryProManager.instance.Events.onOpenInventory.AddListener(CloseMap);

        for (int i = 0; i < stations.Count; i++)
        {
            stations[i].Init(i);
        }
    }

    private void ToggleMapVisibility()
    {
        if (PauseMenu.isPaused || G.PlayerStats.IsDead) return;

        if (!isMapOpen && (!interactManager.inspecting && interactManager.realtimeAttachmentCustomization ||
                           !interactManager.realtimeAttachmentCustomization)) OpenMap();
        else CloseMap();
    }

    private void OpenMap()
    {
        isMapOpen = true;

        onMapOpen?.Invoke();

        InputManager.ToggleUIControls(true);
        PauseMenu.Instance.stats.LoseControl();
        UIController.instance.UnlockMouse();
        UIController.instance.crosshair.SetVisibility(false);

        mapObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(mapObject);
        SoundManager.Instance.PlaySound(openMapSFX, 0, 0, false, 0);
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

    private void OnDisable()
    {
        InputManager.onTogglePause -= CloseMap;
        InputManager.onMapOpenPressed -= ToggleMapVisibility;
    }
}