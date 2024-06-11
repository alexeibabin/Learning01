using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class CharacterActions : PlayerActionSet
{
    public PlayerAction MoveLeft;
    public PlayerAction MoveRight;
    public PlayerAction MoveForward;
    public PlayerAction MoveBackward;

    public PlayerTwoAxisAction Move;

    public CharacterActions()
    {
        MoveLeft = CreatePlayerAction("MoveLeft");
        MoveRight = CreatePlayerAction("MoveRight");
        MoveForward = CreatePlayerAction("MoveForward");
        MoveBackward = CreatePlayerAction("MoveBackward");

        Move = CreateTwoAxisPlayerAction(MoveLeft, MoveRight, MoveBackward, MoveForward);
    }

    public static CharacterActions GetDefaultPlayerActions()
    {
        CharacterActions ca = new CharacterActions();

        ca.MoveLeft.AddDefaultBinding(Key.A, Key.LeftArrow);
        ca.MoveRight.AddDefaultBinding(Key.D, Key.RightArrow);
        ca.MoveForward.AddDefaultBinding(Key.W, Key.UpArrow);
        ca.MoveBackward.AddDefaultBinding(Key.S, Key.DownArrow);

        ca.ListenOptions.IncludeKeys = true;
        ca.ListenOptions.IncludeMouseButtons = true;
        ca.ListenOptions.IncludeUnknownControllers = true;
        ca.ListenOptions.MaxAllowedBindings = 4;

        return ca;
    }
}
