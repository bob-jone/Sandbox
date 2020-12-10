using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    [Tooltip("Character this object represents.")]
    [SerializeField]
    private CharacterTypes _characterType;

    // Character this object represents
    public CharacterTypes CharacterType { get { return _characterType; } }

    // SpawningCanvas to report actions to
    private SpawningCanvas _spawningCanvas;

    // Initializes this script for use. Should only be completed once
    public void FirstInitialize(SpawningCanvas sc)
    {
        _spawningCanvas = sc;
    }

    // Called when this button is clicked
    public void OnClick_Button()
    {
        _spawningCanvas.SelectCharacter(this);
    }
}
