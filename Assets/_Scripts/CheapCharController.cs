using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using Unity.VisualScripting;

public class CheapCharController : MonoBehaviour
{
    [SerializeField] Animator characterAnimator;

    CharacterActions ca;

    private void Start() {
        InputManager.OnDeviceAttached += OnDeviceAttached;

        ca = CharacterActions.GetDefaultPlayerActions();
    }

    // Update is called once per frame
    void Update()
    {
        if(ca.Move.HasChanged)
        {
            MoveCharacter(ca.Move.Value);
            Debug.LogFormat("Move state changed: {0} ", ca.Move.Value);
        }

        if(ca.MoveLeft.HasChanged)
        {
            Debug.LogFormat("Move left(!) state changed: {0}", ca.Move.Value);
        }
    }

    private void OnDeviceAttached(InputDevice device)
    {
        Debug.LogFormat("Current device : {0}", device.ToString());
    }

    private void MoveCharacter(Vector2 axis) {
        characterAnimator.SetFloat("LeftOrRight", axis.x/2f);
        characterAnimator.SetFloat("ForwardOrBackward", axis.y/2f);
    }
}
