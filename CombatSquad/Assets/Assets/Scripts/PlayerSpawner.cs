using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject PlayerPrefab;
    private GameObject player;
    public GameObject DeathEffect;
    public float RespawnTime = 5f;

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.Instance.SpawnPosition();

        player = PhotonNetwork.Instantiate(PlayerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    public void Die(string damager)
    {
        

        UIController.Instance.DeathText.text = "You were killed by " + damager;

        //PhotonNetwork.Destroy(player);

        //SpawnPlayer();

        MatchManager.Instance.UpdateStatesSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

        if(player != null)
        {
            StartCoroutine(DieCo());
        }
    }

    public IEnumerator DieCo()
    {
        PhotonNetwork.Instantiate(DeathEffect.name, player.transform.position, Quaternion.identity);

        PhotonNetwork.Destroy(player);
        UIController.Instance.DeathScreen.SetActive(true);

        yield return new WaitForSeconds(RespawnTime);

        UIController.Instance.DeathScreen.SetActive(false);

        SpawnPlayer();
    }
}
