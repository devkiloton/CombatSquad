using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public MatchManager Instance;
    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayer,
        ChangeStat
    }
    public List<PlayerInfo> AllPlayers = new List<PlayerInfo>();
    private int index;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
    }

    private void Update()
    {
        
    }

    public void OnEvent(EventData photonEvent)
    {

    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
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