using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] Transform spawnPoint;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            //[] Para cada client conectado, spawna o player com o próprio ID já atribuido no objeto.
            foreach (var client in GameManager.Instance.playerSelectorDatas)
            {
                SpawnPlayer(client.ClientId, client.CharIndex);
            }
        }
    }
    private void SpawnPlayer(ulong clientId, int charIndex)
    {
        GameObject prefabGameplay = GameManager.Instance.characterList[charIndex].playerGamePrefab;

        GameObject playerObj = Instantiate(prefabGameplay, spawnPoint);

        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}