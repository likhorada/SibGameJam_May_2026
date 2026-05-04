using UnityEngine;

public sealed class CraftGameOverTrigger : MonoBehaviour
{
    [SerializeField] private string roomId = "room_02";
    [SerializeField] private ElementDefinition gameOverElement;
    [SerializeField] private GameOverController gameOverController;

    private void OnEnable()
    {
        CraftingPanelUI.ElementCrafted += HandleElementCrafted;
    }

    private void OnDisable()
    {
        CraftingPanelUI.ElementCrafted -= HandleElementCrafted;
    }

    private void HandleElementCrafted(string tableId, string craftedRoomId, ElementDefinition result)
    {
        if (result == null)
            return;

        if (!string.IsNullOrEmpty(roomId) && craftedRoomId != roomId)
            return;

        if (!IsGameOverElement(result))
            return;

        if (gameOverController == null)
            gameOverController = FindAnyObjectByType<GameOverController>();

        if (gameOverController == null)
        {
            Debug.LogError("CraftGameOverTrigger: GameOverController not found");
            return;
        }

        gameOverController.TriggerGameOver();
    }

    private bool IsGameOverElement(ElementDefinition result)
    {
        if (gameOverElement != null)
            return result == gameOverElement;

        return result.Id == "explosion";
    }
}
