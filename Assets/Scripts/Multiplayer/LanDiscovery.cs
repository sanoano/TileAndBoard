using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

[Serializable]
public sealed class LanSessionInfo
{
    public string id;
    public string name;
    public string hostName;
    public int playerCount;
    public int maxPlayers;
    public string gameVersion;
    public int port;
    public int turnTimeSeconds;
    public int startingPlayerHealth;

    [NonSerialized] public string address;
    [NonSerialized] internal float lastSeen;
    [NonSerialized] internal string payload;
}

public sealed class LanDiscovery : MonoBehaviour
{
    private const int DiscoveryPort = 47777;
    private const float BroadcastInterval = 1f;
    private const float SessionLifetime = 3f;
    private const string MessagePrefix = "tileandboard-lan:";

    private readonly Dictionary<string, LanSessionInfo> sessions = new();
    private UdpClient socket;
    private LanSessionInfo hostedSession;
    private float nextBroadcast;

    public event Action SessionsChanged;

    public IReadOnlyCollection<LanSessionInfo> Sessions => sessions.Values;
    public bool IsSearching => socket != null && hostedSession == null;

    public void StartHosting(LanSessionInfo session)
    {
        Stop();
        hostedSession = session;
        OpenSocket();
        BroadcastSession();
    }

    public void StartSearching()
    {
        Stop();
        OpenSocket();
    }

    public void Stop()
    {
        hostedSession = null;
        socket?.Close();
        socket = null;
        sessions.Clear();
    }

    private void Update()
    {
        ReceiveSessions();

        if (hostedSession != null && Time.unscaledTime >= nextBroadcast)
        {
            BroadcastSession();
        }

        if (IsSearching)
        {
            RemoveStaleSessions();
        }
    }

    private void OpenSocket()
    {
        try
        {
            socket = new UdpClient(AddressFamily.InterNetwork);
            socket.ExclusiveAddressUse = false;
            socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Client.Bind(new IPEndPoint(IPAddress.Any, DiscoveryPort));
            socket.EnableBroadcast = true;
        }
        catch (SocketException exception)
        {
            Debug.LogWarning($"Could not start LAN discovery: {exception.Message}");
            Stop();
        }
    }

    private void BroadcastSession()
    {
        if (socket == null || hostedSession == null) return;

        if (Unity.Netcode.NetworkManager.Singleton != null)
        {
            hostedSession.playerCount = Unity.Netcode.NetworkManager.Singleton.ConnectedClientsIds.Count;
        }

        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(MessagePrefix + JsonUtility.ToJson(hostedSession));
            socket.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, DiscoveryPort));
        }
        catch (SocketException exception)
        {
            Debug.LogWarning($"Could not advertise LAN game: {exception.Message}");
        }

        nextBroadcast = Time.unscaledTime + BroadcastInterval;
    }

    private void ReceiveSessions()
    {
        if (!IsSearching) return;

        try
        {
            while (socket.Available > 0)
            {
                IPEndPoint sender = new(IPAddress.Any, 0);
                string message = Encoding.UTF8.GetString(socket.Receive(ref sender));
                if (!message.StartsWith(MessagePrefix, StringComparison.Ordinal)) continue;

                if (message.Length > 2048) continue;

                string payload = message[MessagePrefix.Length..];
                LanSessionInfo session;
                try
                {
                    session = JsonUtility.FromJson<LanSessionInfo>(payload);
                }
                catch (ArgumentException)
                {
                    continue;
                }
                if (!IsValid(session) || session.gameVersion != Application.version) continue;

                session.address = sender.Address.ToString();
                session.lastSeen = Time.unscaledTime;
                session.payload = payload;

                bool changed = !sessions.TryGetValue(session.id, out LanSessionInfo previous) ||
                               previous.payload != payload ||
                               previous.address != session.address;
                sessions[session.id] = session;

                if (changed)
                {
                    SessionsChanged?.Invoke();
                }
            }
        }
        catch (SocketException exception)
        {
            Debug.LogWarning($"Could not receive LAN games: {exception.Message}");
        }
    }

    private void RemoveStaleSessions()
    {
        List<string> staleIds = null;

        foreach (var pair in sessions)
        {
            if (Time.unscaledTime - pair.Value.lastSeen <= SessionLifetime) continue;
            staleIds ??= new List<string>();
            staleIds.Add(pair.Key);
        }

        if (staleIds == null) return;

        foreach (string id in staleIds)
        {
            sessions.Remove(id);
        }

        SessionsChanged?.Invoke();
    }

    private static bool IsValid(LanSessionInfo session)
    {
        return session != null &&
               !string.IsNullOrWhiteSpace(session.id) && session.id.Length <= 64 &&
               !string.IsNullOrWhiteSpace(session.name) && session.name.Length <= 64 &&
               !string.IsNullOrWhiteSpace(session.hostName) && session.hostName.Length <= 64 &&
               session.port is > 0 and <= ushort.MaxValue &&
               session.playerCount >= 0 &&
               session.maxPlayers is > 0 and <= 16 &&
               session.playerCount < session.maxPlayers &&
               session.turnTimeSeconds > 0 &&
               session.startingPlayerHealth > 0;
    }

    private void OnDestroy()
    {
        Stop();
    }
}
