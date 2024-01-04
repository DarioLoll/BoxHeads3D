using System;
using Unity.Collections;
using Unity.Netcode;

namespace Models
{
    public struct PlayerId : IEquatable<PlayerId>, INetworkSerializable
    {
        private FixedString64Bytes _lobbyPlayerId;

        public FixedString64Bytes LobbyPlayerId
        {
            get => _lobbyPlayerId;
            set => _lobbyPlayerId = value;
        }

        public ulong ClientId
        {
            get => _clientId;
            set => _clientId = value;
        }

        private ulong _clientId;
        
        public PlayerId(FixedString64Bytes lobbyPlayerId, ulong clientId)
        {
            _lobbyPlayerId = lobbyPlayerId;
            _clientId = clientId;
        }

        public bool Equals(PlayerId other)
        {
            return _lobbyPlayerId.Equals(other._lobbyPlayerId) && _clientId == other._clientId;
        }


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _lobbyPlayerId);
            serializer.SerializeValue(ref _clientId);
        }
    }
}