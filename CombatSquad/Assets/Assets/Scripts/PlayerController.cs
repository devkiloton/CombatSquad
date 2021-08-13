using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private float mouseSensivity;
    [SerializeField]
    private float moveSpeed = 5f;
    [SerializeField]
    private float runSpeed = 8f;
    [SerializeField]
    private float jumpForce = 5f;
    [SerializeField]
    private float gravityMod = 2.5f;
    [SerializeField]
    private GameObject bulletImpact;
    //[SerializeField]
    //private float timeBetweenShots = .1f;
    [SerializeField]
    private float maxHeat = 10f;
    //[SerializeField]
    //private float heatPerShot;
    [SerializeField]
    private float coolRate = 4f;
    [SerializeField]
    private float overheatCoolRate = 5f;
    [SerializeField]
    private float muzzleDisplayTime;
    private float heatCounter;
    private bool overHeated;
    private float shotCounter;
    private float activeMoveSpeed;
    private Transform viewPoint;
    private CharacterController chaCon;
    private Camera cam;
    private Transform groundCheckPoint;
    private bool isGrounded;
    private LayerMask groundLayers;
    private float verticalRotStore;
    private Vector2 mouseInput;
    private Vector3 moveDir;
    private Vector3 movement;
    public Gun[] AllGuns;
    private int selectedGun;
    private float muzzleCounter;
    public GameObject PlayerHitImpact;

    public int MaxHealth = 100;
    private int CurrentHealth;

    public Animator Anim;
    public GameObject PlayerModel;
    public Transform ModelGunPoint;
    public Transform GunHolder;

    private void Start()
    {
        groundCheckPoint = gameObject.GetComponentInChildren<Transform>().Find("GroundCheckPoint");
        viewPoint = gameObject.GetComponentInChildren<Transform>().Find("ViewPoint");
        chaCon = GetComponent<CharacterController>();
        groundLayers = LayerMask.GetMask("Ground");
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
        //SwitchGun();
        photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        CurrentHealth = MaxHealth;

        if (photonView.IsMine)
        {
            PlayerModel.SetActive(false);

            UIController.Instance.WeaponTempSlider.maxValue = maxHeat;
            UIController.Instance.SliderHealth.maxValue = MaxHealth;
        }
        else
        {
            GunHolder.parent = ModelGunPoint;
            GunHolder.localPosition = Vector3.zero;
            GunHolder.localRotation = Quaternion.identity;
        }
        //Transform position = SpawnManager.Instance.SpawnPosition();
        //transform.position = position.position;
        //transform.rotation = position.rotation;
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensivity;

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);

            verticalRotStore -= mouseInput.y;
            verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);

            viewPoint.rotation = Quaternion.Euler(verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);

            moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

            if (Input.GetKey(KeyCode.LeftShift))
            {
                activeMoveSpeed = runSpeed;
            }
            else
            {
                activeMoveSpeed = moveSpeed;
            }

            float yVel = movement.y;

            movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeMoveSpeed;

            movement.y = yVel;

            if (chaCon.isGrounded)
            {
                movement.y = 0f;
            }

            isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayers);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                movement.y = jumpForce;
            }
            if (AllGuns[selectedGun].gunMuzzleFlashes.activeInHierarchy)
            {
                muzzleCounter -= Time.deltaTime;
                if (muzzleCounter <= 0)
                {
                    AllGuns[selectedGun].gunMuzzleFlashes.SetActive(false);
                }
            }

            movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;

            chaCon.Move(movement * Time.deltaTime);

            if (!overHeated)
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    Shoot();
                }

                if (Input.GetMouseButton(0) && AllGuns[selectedGun].isAutomatic)
                {
                    shotCounter -= Time.deltaTime;
                    if (shotCounter <= 0f)
                    {
                        Shoot();
                    }
                }
                heatCounter -= coolRate * Time.deltaTime;
            }
            else
            {
                heatCounter -= overheatCoolRate * Time.deltaTime;
                if (heatCounter <= 0f)
                {
                    overHeated = false;
                    UIController.Instance.overheatedMessage.gameObject.SetActive(false);
                }
            }

            if (heatCounter < 0f)
            {
                heatCounter = 0f;
            }

            UIController.Instance.WeaponTempSlider.value = heatCounter;

            UIController.Instance.SliderHealth.value = CurrentHealth;


            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                selectedGun++;
                if (selectedGun >= AllGuns.Length)
                {
                    selectedGun = AllGuns.Length - 1;
                }
                //SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                selectedGun--;
                if (selectedGun < 0f)
                {
                    selectedGun = 0;
                }
                //SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);
            }

            for (int i = 0; i < AllGuns.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    selectedGun = i;
                    SwitchGun();
                }
            }

            Anim.SetBool("grounded", isGrounded);
            Anim.SetFloat("speed", moveDir.magnitude);

            if (Input.GetKey(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }
    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        ray.origin = cam.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                PhotonNetwork.Instantiate(PlayerHitImpact.name, hit.point, Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, AllGuns[selectedGun].ShotDamage, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                GameObject bulletImpactObj = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal));
                Destroy(bulletImpactObj, 10f);
            }
        }

        shotCounter = AllGuns[selectedGun].timeBetweenShots;

        heatCounter += AllGuns[selectedGun].heatPerShot;

        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;
            UIController.Instance.overheatedMessage.gameObject.SetActive(true);
        }
        AllGuns[selectedGun].gunMuzzleFlashes.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }

    [PunRPC]
    public void DealDamage(string damager, int damageAmount, int actor)
    {
        TakeDamage(damager, damageAmount, actor);
    }

    public void TakeDamage(string damager, int damageAmount, int actor)
    {
        if (photonView.IsMine)
        {
            CurrentHealth -= damageAmount;


            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                //CurrentHealth = MaxHealth;
                UIController.Instance.SliderHealth.value = CurrentHealth;

                PlayerSpawner.Instance.Die(damager);

                MatchManager.Instance.UpdateStatesSend(actor, 0, 1);
            }
        }
    }

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            cam.transform.position = viewPoint.position;
            cam.transform.rotation = viewPoint.rotation;
        }
    }
    private void SwitchGun()
    {
        foreach (Gun gun in AllGuns)
        {
            gun.gameObject.SetActive(false);
        }
        AllGuns[selectedGun].gameObject.SetActive(true);
        AllGuns[selectedGun].gunMuzzleFlashes.SetActive(false);
    }

    [PunRPC]
    public void SetGun(int gunToSwitch)
    {
        if(gunToSwitch < AllGuns.Length)
        {
            selectedGun = gunToSwitch;
            SwitchGun();
        }
    }
}
