using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;
    public GameObject LoadingScreen;
    public GameObject MenuButtons;
    public TMP_Text LoadingText;

    public GameObject CreateRoomScreen;
    public TMP_InputField RoomNameInput;

    public GameObject RoomScreen;
    public TMP_Text RoomNameText;
    public TMP_Text PlayerNameLabel;
    private List<TMP_Text> allPlayersName = new List<TMP_Text>();

    public GameObject ErrorScreen;
    public TMP_Text ErrorText;

    public GameObject RoomBrowserScreen;
    public RoomButton TheRoomButton;
    private List<RoomButton> allRoomButtons = new List<RoomButton>();

    public GameObject NicknameInputScreen;
    public TMP_InputField NicknameInputField;
    public static bool hasSetNickname;

    public string LevelToPlay;
    public GameObject StartButton;

    public GameObject RoomTestButton;

    public string[] allMaps;
    public bool changeMapBetweenRounds = true;

    public void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CloseMenus();
        LoadingScreen.SetActive(true);
        LoadingText.text = "Connecting to network...";

        PhotonNetwork.ConnectUsingSettings();

#if UNITY_EDITOR
        RoomTestButton.SetActive(true);
#endif
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseMenus()
    {
        LoadingScreen.SetActive(false);
        MenuButtons.SetActive(false);
        CreateRoomScreen.SetActive(false);
        RoomScreen.SetActive(false);
        ErrorScreen.SetActive(false);
        RoomBrowserScreen.SetActive(false);
        NicknameInputScreen.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();

        PhotonNetwork.AutomaticallySyncScene = true;

        LoadingText.text = "Joining Lobby";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        MenuButtons.SetActive(true);

        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if (!hasSetNickname)
        {
            CloseMenus();
            NicknameInputScreen.SetActive(true);

            if (PlayerPrefs.HasKey("playerNickname"))
            {
                NicknameInputField.text = PlayerPrefs.GetString("playerNickname");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerNickname");
        }
    }

    public void OpenRoomCreate()
    {
        CloseMenus();
        CreateRoomScreen.SetActive(true);
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(RoomNameInput.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;

            PhotonNetwork.CreateRoom(RoomNameInput.text, options);

            CloseMenus();
            LoadingText.text = "Creating Room...";
            LoadingScreen.SetActive(true);
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();
        RoomScreen.SetActive(true);

        RoomNameText.text = PhotonNetwork.CurrentRoom.Name;

        ListAllPlayers();

        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }

    private void ListAllPlayers()
    {
        foreach(TMP_Text player in allPlayersName)
        {
            Destroy(player.gameObject);
        }
        allPlayersName.Clear();

        Player[] players = PhotonNetwork.PlayerList;
        for(int i = 0; i<players.Length; i++)
        {
            TMP_Text newPlayerLabel = Instantiate(PlayerNameLabel, PlayerNameLabel.transform.parent);
            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);

            allPlayersName.Add(newPlayerLabel);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerLabel = Instantiate(PlayerNameLabel, PlayerNameLabel.transform.parent);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);

        allPlayersName.Add(newPlayerLabel);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        ErrorText.text = "Failed to create room: " + message;
        CloseMenus();
        ErrorScreen.SetActive(true);
    }

    public void CloseErrorScreen()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        LoadingText.text = "Leaving room";
        LoadingScreen.SetActive(true);
    }
    public override void OnLeftRoom()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }
    public void OpenRoomBrowser()
    {
        CloseMenus();
        RoomBrowserScreen.SetActive(true);
    }
    public void CloseRoomBrowser()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(RoomButton rb in allRoomButtons)
        {
            Destroy(rb.gameObject);
        }
        allRoomButtons.Clear();

        TheRoomButton.gameObject.SetActive(false);


        for(int i = 0; i<roomList.Count; i++)
        {
            if(roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newButton = Instantiate(TheRoomButton, TheRoomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);

                allRoomButtons.Add(newButton);
            }
        }
    }
    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);

        CloseMenus();
        LoadingText.text = "Joining Room";
        LoadingScreen.SetActive(true);
    }
    
    public void SetNickname()
    {
        if (!string.IsNullOrEmpty(NicknameInputField.text))
        {
            PhotonNetwork.NickName = NicknameInputField.text;

            PlayerPrefs.SetString("playerNickname", NicknameInputField.text);

            CloseMenus();
            MenuButtons.SetActive(true);
            hasSetNickname = true;
        }
    }

    public void StartGame()
    {
        //PhotonNetwork.LoadLevel(LevelToPlay);
        PhotonNetwork.LoadLevel(allMaps[Random.Range(0, allMaps.Length)]);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }

    public void QuickJoin()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 8;
        PhotonNetwork.CreateRoom("Test", options);
        CloseMenus();
        LoadingText.text = "Creating Room";
        LoadingScreen.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
