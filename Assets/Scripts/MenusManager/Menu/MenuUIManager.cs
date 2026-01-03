using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    [SerializeField] private Button playBtn;
    private void Awake()
    {
        playBtn.onClick.AddListener(() => LoaderScenes.Load(LoaderScenes.Scenes.MultiplayerScene)); 
    }
}
