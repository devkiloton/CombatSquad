using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager Instance;
    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayer,
        UpdateStat
    }
    public List<PlayerInfo> AllPlayers = new List<PlayerInfo>();
    private int index;

    private List<LeaderboardPlayer> LboardPlayers = new List<LeaderboardPlayer>();

    public enum GameState
    {
        Waiting,
        Playing,
        Ending
    }

    public int KillsToWin = 3;
    public Transform EndScreenCam;
    public GameState state = GameState.Waiting;
    public float WaitAfterEnding = 5f;

    public void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);

            state = GameState.Playing;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && state != GameState.Ending)
        {
            if (UIController.Instance.leaderboard.activeInHierarchy)
            {
                UIController.Instance.leaderboard.SetActive(false);
            }
            else
            {
                ShowLeaderboard();
            }
        }   
    }

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            switch (theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;

                case EventCodes.ListPlayer:
                    ListPlayersReceive(data);
                    break;

                case EventCodes.UpdateStat:
                    UpdateStatsReceive(data);
                    break;
            }
        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void NewPlayerSend(string username)
    {
        object[] package = new object[4];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer, package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
            );
    }

    public void NewPlayerReceive(object[] dataReceived)
    {
        PlayerInfo player = new PlayerInfo((string)dataReceived[0], (int)dataReceived[1],
                                            (int)dataReceived[2], (int)dataReceived[3]);
        AllPlayers.Add(player);

        ListPlayersSend();
    }

    public void ListPlayersSend()
    {
        object[] package = new object[AllPlayers.Count + 1];

        package[0] = state;

        for(int i = 0; i < AllPlayers.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = AllPlayers[i].Name;
            piece[1] = AllPlayers[i].Actor;
            piece[2] = AllPlayers[i].Kills;
            piece[3] = AllPlayers[i].Deaths;

            package[i + 1] = piece;
        }

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListPlayer, package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );

    }

    public void ListPlayersReceive(object[] dataReceived)
    {
        AllPlayers.Clear();

        state = (GameState)dataReceived[0];

        for(int i = 1; i < dataReceived.Length; i++)
        {
            object[] piece = (object[])dataReceived[i];

            PlayerInfo player = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
                );
            AllPlayers.Add(player);

            if(PhotonNetwork.LocalPlayer.ActorNumber == player.Actor)
            {
                index = i - 1;
            }
        }
        StateCheck();
    }
    public void UpdateStatesSend(int actorSending, int statToUpdate, int amountToChange)
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdateStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void UpdateStatsReceive(object[] dataReceived)
    {
        int actor = (int)dataReceived[0];
        int statType = (int)dataReceived[1];
        int amount = (int)dataReceived[2];

        for(int i = 0; i < AllPlayers.Count; i++)
        {
            if(AllPlayers[i].Actor == actor)
            {
                switch (statType)
                {
                    case 0: //kills
                        AllPlayers[i].Kills += amount;
                        Debug.Log("player" + AllPlayers[i].Name + " : kills" + AllPlayers[i].Kills);
                        break;
                    case 1: //deaths
                        AllPlayers[i].Deaths += amount;
                        Debug.Log("player" + AllPlayers[i].Name + " : Deaths" + AllPlayers[i].Deaths);
                        break;
                }

                if(i == index)
                {
                    UpdateStatsDisplay();
                }

                if(UIController.Instance.leaderboard.activeInHierarchy)
                {
                    ShowLeaderboard();
                }

                break;
            }
        }
        ScoreCheck();
    }

    public void UpdateStatsDisplay()
    {
        if(AllPlayers.Count > index)
        {
            UIController.Instance.KillsText.text = "Kills: " + AllPlayers[index].Kills;
            UIController.Instance.DeathsText.text = "Deaths: " + AllPlayers[index].Deaths;
        }
        else
        {
            UIController.Instance.KillsText.text = "Kills: 0";
            UIController.Instance.DeathsText.text = "Deaths: 0";
        }
    }

    private void ShowLeaderboard()
    {
        UIController.Instance.leaderboard.SetActive(true);

        foreach(LeaderboardPlayer lp in LboardPlayers)
        {
            Destroy(lp.gameObject);
        }

        LboardPlayers.Clear();

        UIController.Instance.leaderboardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = sortPlayers(AllPlayers);

        foreach(PlayerInfo player in sorted)
        {
            LeaderboardPlayer newPlayerDisplay = Instantiate(UIController.Instance.leaderboardPlayerDisplay,
                                                             UIController.Instance.leaderboardPlayerDisplay.transform.parent);
            newPlayerDisplay.SetDetails(player.Name, player.Kills, player.Deaths);

            newPlayerDisplay.gameObject.SetActive(true);

            LboardPlayers.Add(newPlayerDisplay);
        }
    }

    private List<PlayerInfo> sortPlayers(List<PlayerInfo> players)
    {
        List < PlayerInfo > sorted = new List<PlayerInfo>();
        while(sorted.Count < players.Count)
        {
            int highest = -1;
            PlayerInfo selectedPlayer = players[0];

            foreach(PlayerInfo player in players)
            {
                if (!sorted.Contains(player))
                {
                    if(player.Kills > highest)
                    {
                        selectedPlayer = player;
                        highest = player.Kills;
                    }
                }
            }
            sorted.Add(selectedPlayer);
        }
        return sorted;
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        SceneManager.LoadScene(0);
    }

    private void ScoreCheck()
    {
        bool winnerFound = false;

        foreach(PlayerInfo player in AllPlayers)
        {
            if(player.Kills >= KillsToWin && KillsToWin > 0)
            {
                winnerFound = true;
                break;
            }
        }

        if (winnerFound)
        {
            if(PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                state = GameState.Ending;
                ListPlayersSend();
            }
        }
    }

    private void StateCheck()
    {
        if(state == GameState.Ending)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        state = GameState.Ending;

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }

        UIController.Instance.EndScreen.SetActive(true);
        ShowLeaderboard();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera.main.transform.position = EndScreenCam.position;
        Camera.main.transform.rotation = EndScreenCam.rotation;

        StartCoroutine(EndCo());
    }

    private IEnumerator EndCo()
    {
        yield return new WaitForSeconds(WaitAfterEnding);

        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }
}

[System.Serializable]
public class PlayerInfo
{
    public string Name;
    public int Actor;
    public int Kills;
    public int Deaths;

    public PlayerInfo(string _name, int _actor, int _kills, int _deaths)
    {
        Name = _name;
        Actor = _actor;
        Kills = _kills;
        Deaths = _deaths;
    }
}