using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [SerializeField] private InputActionAsset inputActionsAssets;

    private InputAction movementInput;
    
    private void Awake()
    {
        if (instance != null && instance == this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        //[] Encontra mapas de ações
        var playerMap = inputActionsAssets.FindActionMap("Player");
        
        movementInput = playerMap.FindAction("Movement");
    }
    public Vector2 MovementInput()
    {
        return movementInput.ReadValue<Vector2>();
    }
}
