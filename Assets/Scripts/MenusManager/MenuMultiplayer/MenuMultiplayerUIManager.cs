using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MenuMultiplayerUIManager : MonoBehaviour
{
    [SerializeField] private Button startHostBtn;
    [SerializeField] private Button startClientBtn;

    [SerializeField] private TMP_InputField textField;

    private void Awake()
    {
        startHostBtn.onClick.AddListener(() => GameManager.Instance.CreateRelay());
        startClientBtn.onClick.AddListener(() => GameManager.Instance.JoinRelay());
    }
}
