using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueInput : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;

    private void Update()
    {
        bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (mouseClicked || spacePressed)
        {
            // Se o texto ainda está aparecendo letra por letra, completa primeiro
            if (DialogueView.Instancia != null && DialogueView.Instancia.TentarCompletarDigitacao())
            {
                return;
            }

            runner.AdvanceDialogue();
        }
    }
}