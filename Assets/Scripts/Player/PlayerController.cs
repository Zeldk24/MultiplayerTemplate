using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkObject))]
public class PlayerController : NetworkBehaviour
{
    [Header("PlayerControll")]
    private Rigidbody playerRb;
    [SerializeField] private float playerSpeed;
    private Vector2 currentPlayerMovement;

    [Header("Ground Control")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            Vector2 move = currentPlayerMovement;
            var movement = new Vector3(move.x, 0, move.y);

            var finalVelocity = new Vector3(movement.x * playerSpeed, playerRb.linearVelocity.y, movement.z * playerSpeed);

            playerRb.linearVelocity = finalVelocity;

            isGrounded = Physics.Raycast(playerRoot.position, Vector3.down, 0.3f, groundLayer);

            if (!isGrounded)
            {
                playerRb.AddForce(Vector3.down * 45, ForceMode.Acceleration);
            }

            if (movement != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(playerRb.rotation, rotation, 10f * Time.fixedDeltaTime);
            }
        }
    }
    private void OnMovement(InputValue value)
    {
        currentPlayerMovement = value.Get<Vector2>();
    }
    private void OnJump()
    {
        if (isGrounded)
        {
            playerRb.AddForce(Vector3.up * 15, ForceMode.Impulse);
        }
    }
   
}
