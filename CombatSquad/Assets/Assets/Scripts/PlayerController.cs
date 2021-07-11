using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
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

    private void Start()
    {
        groundCheckPoint = gameObject.GetComponentInChildren<Transform>().Find("GroundCheckPoint");
        viewPoint = gameObject.GetComponentInChildren<Transform>().Find("ViewPoint");
        groundLayers = LayerMask.GetMask("Ground");
        chaCon = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
    }

    private void Update()
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
        //print(isGrounded);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            movement.y = jumpForce;
        }

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;

        chaCon.Move(movement * Time.deltaTime);

        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }

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
    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(.5f,.5f,0f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("we hit" + hit.collider.gameObject.name);
        }
    }
    private void LateUpdate()
    {
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }
}
