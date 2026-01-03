using UnityEngine;

[CreateAssetMenu(menuName = "VisualCharacterSO/ CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Visual Prefab")]
    public GameObject[] visualPrefab;

    [Header("Character Prefab")]
    [SerializeField] private GameObject characterPrefab;
}
