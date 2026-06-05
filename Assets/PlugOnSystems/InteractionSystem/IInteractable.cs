public interface IInteractable
{
    void Interact();
    string GetInteractPrompt();
    void OnFocus();  
    void OnLoseFocus();
}