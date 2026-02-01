using Unity.Netcode;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public static class Player
{
    
    public enum PlayerId : byte
    {
        Player1,
        Player2,
    }
    
    public static PlayerId AssignPlayerID()
    {

        PlayerId playerId;
        
        if (NetworkManager.Singleton.LocalClient.ClientId == 1)
        {
            playerId = PlayerId.Player1;
        }
        else
        {
            playerId = PlayerId.Player2;
        }

        Debug.Log(playerId);

        return playerId;
    }
    
    
}