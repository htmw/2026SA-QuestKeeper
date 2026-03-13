using UnityEngine;
using UnityEngine.InputSystem;

// Ensures if the controller is put on any prefab, it will always have the base character class.
[RequireComponent(typeof(CharacterBase))]
public class PlayerController : MonoBehaviour
{
    private CharacterBase fighter;
    private PlayerControls controls;

    private void Awake()
    {
        fighter = GetComponent<CharacterBase>();
        controls = new PlayerControls();

        // -- EVENT BINDINGS --
        controls.Player.Jump.performed += ctx => fighter.Jump();
        controls.Player.Attack.performed += ctx => fighter.Attack();

        controls.Player.Block.started += ctx => fighter.Block(true);
        controls.Player.Block.canceled += ctx => fighter.Block(false);
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        // -- CONTINOUS POLLING -- 

        // Read the joystick/WASD as a vector2 (X is left/right, Y is up/down)
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        
        // Since this is a 2D fighting game, we only care about the X value
        fighter.Move(moveInput.x);
    }
}
