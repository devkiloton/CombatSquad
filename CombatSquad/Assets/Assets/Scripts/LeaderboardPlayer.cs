using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardPlayer : MonoBehaviour
{
    public TMP_Text PlayerNameText;
    public TMP_Text killsText;
    public TMP_Text deathsText;

    public void SetDetails(string name, int kills, int deaths)
    {
        PlayerNameText.text = name;
        killsText.text = kills.ToString();
        deathsText.text = deaths.ToString();
    }
}
