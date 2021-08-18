using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public TMP_Text overheatedMessage;
    public Slider WeaponTempSlider;

    public GameObject DeathScreen;
    public TMP_Text DeathText;

    public Slider SliderHealth;

    public TMP_Text KillsText;
    public TMP_Text DeathsText;

    public GameObject leaderboard;
    public LeaderboardPlayer leaderboardPlayerDisplay;

    public GameObject EndScreen;

    public TMP_Text Clock;

    private void Awake()
    {
        Instance = this;
    }

}
