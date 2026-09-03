using Unity.Netcode;

public static class Player
{
    
    public enum PlayerId : byte
    {
        Player1,
        Player2,
    }
    
    public static PlayerId AssignPlayerID()
    {
        if (NetworkManager.Singleton)
        {
            bool isHost = NetworkManager.Singleton.IsHost ||
                          NetworkManager.Singleton.LocalClient.IsSessionOwner;

            return isHost ? PlayerId.Player1 : PlayerId.Player2;
        }

        return PlayerId.Player1;
    }
    
    
}
