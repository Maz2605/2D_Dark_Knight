using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput playerInput;

    private Vector2 movementInput;
   
    public void OnMoveInput(InputAction.CallbackContext context)
    {
        Debug.Log("Movevement");
    }
    public void OnJumpInput(InputAction.CallbackContext context)
    {
        Debug.Log("Jump");
    }
    public void OnInputMouse(InputAction.CallbackContext context)
    {
        Debug.Log("Mouse");

    }
    
}
