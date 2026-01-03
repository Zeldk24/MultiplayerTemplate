using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    private Lobby hostLobby;
    private float heartBeatTimer;

    public string joinCode;
    private string Key_Start_Game = "RelayJoinCode";

    [Serializable]
    public struct CharacterList
    {
        string playerName;
        public GameObject lobbyPlayerPrefab;
        public GameObject playerGamePrefab;
    }

    public List<CharacterList> characterList;
    public NetworkList<PlayerSelectorData> playerSelectorDatas = new NetworkList<PlayerSelectorData>();
   
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

           
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private async void Start()
    {
        //[] O "await" garante que o serviço esteja pronto antes de usa-lo, devido a latência que a rede pode ter.
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => Debug.Log(AuthenticationService.Instance.PlayerId);
      
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay()
    {
        try
        {
            /*[] O parâmetro aqui do "CreateAllocationAsync" se chama maxConnections (conexões máximas). Como o Host é quem cria a alocação e
               já está "dentro" dela, ele não conta como uma conexão externa que virá através do servidor da Unity.
            */
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );

            string lobbyName = "MyLobby";
            int maxPlayers = 4;

           
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { Key_Start_Game, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                }
            };

            /*[] O parâmetro aqui no "CreateLobbyAsync" se chama maxPlayers. O Lobby da Unity trata o Host como um dos membros da lista de jogadores. 
              Assim que você cria o Lobby, você automaticamente ocupa a primeira vaga (slot 0).
           */
            hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

          
            NetworkManager.Singleton.StartHost();
            LoaderScenes.LoadInNetwork(LoaderScenes.Scenes.CharacterSelection);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinRelay()
    {
        try
        {
            hostLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            string relayCode = hostLobby.Data[Key_Start_Game].Value;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();

            Debug.Log("Você entrou no Lobby: " + hostLobby.Name);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
        catch (LobbyServiceException e) 
        {
            Debug.Log(e);
        }
    }

    public async Task RemoveLobbyPlayer()
    {
        try
        {
            if (hostLobby != null)
            {
                string playerId = AuthenticationService.Instance.PlayerId;

                if (hostLobby.HostId == playerId)
                {
                    await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);

                }
                else
                {
                    await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, playerId);
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        finally
        {
            hostLobby = null;
            NetworkManager.Singleton.Shutdown();
        }
    }
    private void Update()
    {
        HandleHeartBeatTimer();
    }
    private async void HandleHeartBeatTimer()
    {
        /*[] Esse método serve para soltar um ping no lobby a cada 15 segundos, isso evita que o lobby feche por inatividade
             mesmo se tiver jogadores dentro do lobby. Além disso ele verifica se o host está conectado na sala, para que assim o lobby possa
             fechar sozinho quando o host sair.
        */
        if (hostLobby != null && hostLobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer < 0)
            {
                heartBeatTimer = 15;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }

        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectionTestServerRPC(ulong clientId, int direction)
    {
        for(int i = 0; i < playerSelectorDatas.Count; i++)
        {
            if (playerSelectorDatas[i].ClientId == clientId)
            {
                PlayerSelectorData data = playerSelectorDatas[i];

                int nextIndex = data.CharIndex + direction;

                if (nextIndex >= characterList.Count) { nextIndex = 0; }
                if (nextIndex < 0) { nextIndex = characterList.Count - 1; }

                playerSelectorDatas[i] = new PlayerSelectorData(clientId, nextIndex);
                return;
            }
        }
    }
    
}
