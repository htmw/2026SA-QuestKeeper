using UnityEngine;
using UnityEngine.InputSystem;

// Ensures if the controller is put on any prefab, it will always have the base character class.
[RequireComponent(typeof(CharacterBase))]
public class PlayerController : MonoBehaviour
{
    private CharacterBase fighter;
    private PlayerControls controls;

    public bool isInputLocked = false;
    public void LockMovement() { isInputLocked = true; }
    public void UnlockMovement() { isInputLocked = false; }

    private void Awake()
    {
        fighter = GetComponent<CharacterBase>();
        controls = new PlayerControls();

        // -- EVENT BINDINGS --
        controls.Player.Jump.performed += ctx => { if (!isInputLocked) fighter.Jump(); };
        controls.Player.Attack.performed += ctx => { if (!isInputLocked) fighter.Attack(); };

        controls.Player.Block.started += ctx => { if (!isInputLocked) fighter.Block(true); };
        controls.Player.Block.canceled += ctx => fighter.Block(false);

        controls.Player.Kick.performed += ctx => { if (!isInputLocked) fighter.Kick(); };
        controls.Player.Duck.started += ctx => { if (!isInputLocked) fighter.Duck(true); };
        controls.Player.Duck.canceled += ctx => fighter.Duck(false);
        controls.Player.Grab.performed += ctx => { if (!isInputLocked) fighter.Grab(); };
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
        float moveX = isInputLocked ? 0f : controls.Player.Move.ReadValue<Vector2>().x;

        // Temp Debug
        Debug.Log("MoveX: " + moveX);
        fighter.Move(moveX);

    }
}
