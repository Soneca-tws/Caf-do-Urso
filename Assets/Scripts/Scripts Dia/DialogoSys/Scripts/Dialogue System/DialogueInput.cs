using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueInput : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;

    private void Update()
    {
        // Verifica se o mouse ou o teclado estão conectados e se foram pressionados neste frame
        bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (mouseClicked || spacePressed)
        {
            runner.AdvanceDialogue();
        }
    }
}