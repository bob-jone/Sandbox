using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class SpawningCanvas : MonoBehaviour
{
    // Current player instance
    private PlayerInstance _playerInstance;

    // CanvasGroup on this object.
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        // Initialize character selections.
        CharacterSelection[] characterSelections = gameObject.GetComponentsInChildren<CharacterSelection>();
        foreach (CharacterSelection cs in characterSelections)
            cs.FirstInitialize(this);

        // Listen for when player instance spawns.
        PlayerInstance.OnPlayerInstance += PlayerInstance_OnPlayerInstance;

        // Hide the canvas
        _canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }

    // Received when this player instance is set as the local player.
    private void PlayerInstance_OnPlayerInstance(PlayerInstance obj)
    {
        _playerInstance = obj;
    }

    // Show the canvas
    public void Show()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;
    }

    // Hide the canvas
    public void Hide()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }

    // Selects a character as set on CharacterSelection.
    public void SelectCharacter(CharacterSelection selection)
    {
        _playerInstance.PlayerSpawner.TrySpawn(selection.CharacterType);
    }
}
