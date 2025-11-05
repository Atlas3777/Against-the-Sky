using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public static class UIEnemyTimerUpdater
{
    private static TextMeshProUGUI timeInHUD;
    private static TextMeshProUGUI text;

    public static void RecalculateTimeInHUD()
    {
        text = G.Player.transform.parent.parent.Find("HUD Canvas")
            .Find("HUD").Find("Tasks background").Find("Text")
            .GetComponent<TextMeshProUGUI>();      
        timeInHUD=text.transform.Find("Time").GetComponent<TextMeshProUGUI>();
    }

    public static void SetTime(int seconds) => SetTimeWithPhrase(seconds, "next evacuation:");

    public static void SetTimeWithPhrase(int seconds, string phrase=null)
    {
        if (phrase is null)
        {
            SetTime(seconds);
            return;
        }

        if (!text.gameObject.activeInHierarchy)
            text.gameObject.SetActive(true);
        text.text = phrase; 
        int m = seconds / 60;
        int s = seconds % 60;
        timeInHUD.text = string.Format("{0:d2}:{1:d2}", m, s);
    }

    public static void StopVisual()
    {
        text.gameObject.SetActive(false);
    }
}
