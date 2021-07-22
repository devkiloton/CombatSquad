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
    }

    private void CloseMenus()
    {
        LoadingScreen.SetActive(false);
        MenuButtons.SetActive(false);
        CreateRoomScreen.SetActive(false);
        RoomScreen.SetActive(false);
        ErrorScreen.SetActive(false);
        RoomBrowserScreen.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();

        LoadingText.text = "Joining Lobby";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        MenuButtons.SetActive(true);

        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();
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
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
