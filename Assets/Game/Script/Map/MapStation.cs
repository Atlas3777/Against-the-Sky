using UnityEngine;
using cowsins;
using UnityEngine.EventSystems;

public class MapStation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private GameObject highlight;
    [SerializeField] private GameObject selectHighlight;
    [SerializeField] private int fuelAmount;
    [SerializeField] private string description;
    
    private int id;

    public int FuelAmount { get { return fuelAmount; } }
    public int ID { get { return id; } }
    public string Description { get { return description; } }

    public void Init(int id)
    {
        this.id = id;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        G.MapManager.PointerClick(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        G.MapManager.PointerEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        G.MapManager.PointerExit();
    }

    public void Highlight()
    {
        highlight.SetActive(true);
    }

    public void Unhighlight()
    {
        highlight.SetActive(false);
    }

    public void Select()
    {
        selectHighlight.SetActive(true);
    }

    public void Unselect()
    {
        selectHighlight.SetActive(false);
    }
}
