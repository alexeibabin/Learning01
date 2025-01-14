using UnityEngine;

public class CheapCharController : MonoBehaviour
{
    [SerializeField] Animator characterAnimator;

void Update()
{
    // Move the player
    MoveCharacter(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
}

    private void MoveCharacter(Vector2 axis) {
        characterAnimator.SetFloat("LeftOrRight", axis.x/2f);
        characterAnimator.SetFloat("ForwardOrBackward", axis.y/2f);
    }
}
