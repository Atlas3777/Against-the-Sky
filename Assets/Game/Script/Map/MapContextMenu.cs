using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapContextMenu : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI fuelAmountText;
    [SerializeField]
    private TextMeshProUGUI description;

    private MapManager mapManager;

    public void Init(MapManager mapManager)
    {
        this.mapManager = mapManager;
    }

    public void ShowContextMenu(MapStation station, Vector3 position)
    {
        fuelAmountText.text = station.FuelAmount.ToString();
        description.text = station.Description;

        transform.position = position;
        gameObject.SetActive(true);
    }

    public void HideContextMenu()
    {
        fuelAmountText.text = "";
        description.text = "";
        gameObject.SetActive(false);
    }
}
