using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher launcher;
    public GameObject LoadingScreen;
    public GameObject MenuButtons;
    public TMP_Text LoadingText;

    public GameObject CreateRoomScreen;
    public TMP_InputField RoomNameInput;

    public GameObject RoomScreen;
    public TMP_Text RoomNameText;

    private void Awake()
    {
        launcher = this;
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
    }
}
