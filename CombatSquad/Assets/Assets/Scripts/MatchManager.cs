using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    public MatchManager Instance;

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
}
