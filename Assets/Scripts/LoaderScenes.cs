using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LoaderScenes;

public static class LoaderScenes
{
   public enum Scenes
    {
        Menu,
        MultiplayerScene,
        CharacterSelection,
        Gameplay
    }

    public static void Load(Scenes scenes)
    {
        SceneManager.LoadScene(scenes.ToString());
    }

    public static void LoadInNetwork(Scenes scenes)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(scenes.ToString(), LoadSceneMode.Single);
    }
}
