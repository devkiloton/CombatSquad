using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

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

    public GameObject OptionsScreen;



    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptions();
        }

        if(OptionsScreen.activeInHierarchy && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ShowHideOptions()
    {
        if (!OptionsScreen.activeInHierarchy)
        {
            OptionsScreen.SetActive(true);
        }
        else
        {
            OptionsScreen.SetActive(false);
        }
    }

    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
