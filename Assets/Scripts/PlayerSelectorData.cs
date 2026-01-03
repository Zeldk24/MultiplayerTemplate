using System;
using Unity.Netcode;
using UnityEngine;

public struct PlayerSelectorData : INetworkSerializable, IEquatable<PlayerSelectorData>
{
    public ulong ClientId;
    public int CharIndex;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharIndex);
    }

    public PlayerSelectorData(ulong clientId, int charIndex)
    {
        ClientId = clientId;
        CharIndex = charIndex;
    }
    public bool Equals(PlayerSelectorData other)
    {
        return ClientId == other.ClientId && CharIndex == other.CharIndex;
    }

    
}
