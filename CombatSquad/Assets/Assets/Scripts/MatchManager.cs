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
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
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
        object[] package = new object[AllPlayers.Count];

        for(int i = 0; i < AllPlayers.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = AllPlayers[i].Name;
            piece[1] = AllPlayers[i].Actor;
            piece[2] = AllPlayers[i].Kills;
            piece[3] = AllPlayers[i].Deaths;

            package[i] = piece;
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

        for(int i = 0; i < dataReceived.Length; i++)
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
                index = i;
            }
        }
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