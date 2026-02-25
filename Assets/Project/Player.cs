using KinematicCharacterController;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;

    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.CameraTarget);
    }

    void Update()
    {
        playerCamera.UpdateRotation(_playerInput);
        playerCharacter.UpdateInput(_playerInput, playerCamera.transform.rotation);
        playerCharacter.UpdateBody();
    }

    private void LateUpdate()
    {
        playerCamera.UpdatePosition(playerCharacter.CameraTarget);
    }
}
