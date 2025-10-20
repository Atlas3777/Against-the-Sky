using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapContextMenu : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI fuelAmountText;
    [SerializeField]
    private TextMeshProUGUI description;
    [SerializeField]
    private Button selectStationButton;

    private MapManager mapManager;

    public void Init(MapManager mapManager)
    {
        this.mapManager = mapManager;
    }

    public void ShowContextMenu(MapStation station,Vector3 position)
    {
        fuelAmountText.text = station.FuelAmount.ToString();
        description.text = station.Description;
        selectStationButton.onClick.AddListener(()=>SelectStation(station));

        transform.position = position;
        gameObject.SetActive(true);
    }

    public void HideContextMenu()
    {
        fuelAmountText.text = "";
        description.text = "";
        selectStationButton.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
    }

    private void SelectStation(MapStation station)
    {
        print("Selected station id: "+station.ID);
        mapManager.SelectStation(station);
    }
}
