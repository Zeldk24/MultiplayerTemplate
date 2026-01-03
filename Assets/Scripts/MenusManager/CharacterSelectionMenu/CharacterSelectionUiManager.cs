using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionUiManager : NetworkBehaviour
{
    [SerializeField] private Button exitLobby;
    [SerializeField] private Button readyBtn;

    [SerializeField] private Button nextChar;
    [SerializeField] private Button previousChar;

    [SerializeField] private Transform[] spawnPointOnLobby;
    private Dictionary<ulong, GameObject> playerData = new Dictionary<ulong, GameObject>();

    private void Awake()
    {
        exitLobby.onClick.AddListener(async () =>
        {
            await GameManager.Instance.RemoveLobbyPlayer();
            LoaderScenes.Load(LoaderScenes.Scenes.MultiplayerScene);
        });

        readyBtn.onClick.AddListener(() => LoaderScenes.LoadInNetwork(LoaderScenes.Scenes.Gameplay));

        nextChar.onClick.AddListener(() => GameManager.Instance.SelectionTestServerRPC(NetworkManager.Singleton.LocalClientId, 1));
        previousChar.onClick.AddListener(() => GameManager.Instance.SelectionTestServerRPC(NetworkManager.Singleton.LocalClientId, -1));
    }

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.playerSelectorDatas.OnListChanged += OnPlayerChangeVisuals;
        NetworkManager.Singleton.OnClientDisconnectCallback += PlayerLeft;
        RefreshVisuals();

        if (!IsServer) return;

        /*[] Aqui é para garantir que o primeiro player spawnado(Host) também chame o método PlayerJoined, pois o host acaba entrando antes mesmo do evento de CallBack existir,
           e o "ConnectedClientsList" só pega a lista de clientes já conectados. Como o host que vai inicializar esse script, ele acaba não recebendo o CallBack, por isso é uma
           boa prática chamar ele o CallBack de forma manual mesmo.
        */
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            //[] Aqui eu pego o Player que já tem um ClientId e chamo o método PlayerJoined para ele.
            PlayerJoined(client.ClientId);
        }

        //[] Aqui o código só funciona a partir da existência de um Host.
        NetworkManager.Singleton.OnClientConnectedCallback += PlayerJoined;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        foreach (var data in playerData)
        {
            if (data.Value != null)
            {
                NetworkObject netObj = data.Value.GetComponent<NetworkObject>();
                if (netObj != null && netObj.IsSpawned)
                {
                    netObj.Despawn();
                }

            }
        }
        playerData.Clear();

        NetworkManager.Singleton.OnClientConnectedCallback -= PlayerJoined;
        NetworkManager.Singleton.OnClientDisconnectCallback -= PlayerLeft;
    }

    private void PlayerJoined(ulong clientId)
    {
        if (playerData.ContainsKey(clientId)) return;

        var clients = NetworkManager.Singleton.ConnectedClients;

        if (IsServer)
        {
            int charIndex = playerData.Count;

            GameObject visualPrefab = GameManager.Instance.characterList[charIndex].lobbyPlayerPrefab;
            GameObject go = Instantiate(visualPrefab, spawnPointOnLobby[charIndex]);
            go.GetComponent<NetworkObject>().Spawn();


            playerData.Add(clientId, go);
            GameManager.Instance.playerSelectorDatas.Add(new PlayerSelectorData(clientId, charIndex));
        }
        Debug.Log("Entrou: " + clients.Count + "jogador");
    }

    private void PlayerLeft(ulong clientId)
    {
        //[] Se o clientId for igual ao id do cliente local conectado, troque de cena quando chamado.
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            LoaderScenes.Load(LoaderScenes.Scenes.MultiplayerScene);
            return;
        }

        var clients = NetworkManager.Singleton.ConnectedClientsList;

        if (playerData.TryGetValue(clientId, out GameObject go))
        {
            if (IsServer)
            {
                NetworkObject netObj = go.GetComponent<NetworkObject>();
                netObj.Despawn();


                playerData.Remove(clientId);

            }

            for (int i = 0; i < GameManager.Instance.playerSelectorDatas.Count; i++)
            {
                if (GameManager.Instance.playerSelectorDatas[i].ClientId == clientId)
                {
                    GameManager.Instance.playerSelectorDatas.RemoveAt(i);
                    break;
                }
            }
        }
    }

    private void OnPlayerChangeVisuals(NetworkListEvent<PlayerSelectorData> eventChanges)
    {
        RefreshVisuals();
    }
    private void RefreshVisuals()
    {
        if (!IsServer) return;

        foreach (var go in playerData.Values)
        {
            if (go != null)
            {
                NetworkObject netObj = go.GetComponent<NetworkObject>();
                if (netObj != null && netObj.IsSpawned) { netObj.Despawn(); }
            }
           
        }
        playerData.Clear();

        foreach (var data in GameManager.Instance.playerSelectorDatas)
        {
            GameObject visualInstance = GameManager.Instance.characterList[data.CharIndex].lobbyPlayerPrefab;
            GameObject visualPrefab = Instantiate(visualInstance, spawnPointOnLobby[data.ClientId]);
            visualPrefab.GetComponent<NetworkObject>().Spawn();

            playerData.Add(data.ClientId, visualPrefab);
        }
    }
}