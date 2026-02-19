using UnityEngine;

public class tileColour : MonoBehaviour
{// For use with materials that use _shaderTileSelection. Script assigned to the tile itself to receive signals.
    [SerializeField] private int state = 0; //0 is no vfx, 1 is attack, 2 is defend, 3 is move
    private Renderer mainRenderer;
    void Start()
    {// Sets to default colour
        mainRenderer = GetComponent<Renderer>();

        if (mainRenderer != null)
        {
            Debug.Log("Renderer found!");
            mainRenderer.material.SetFloat("_Mode", 0);
        }
        else
            Debug.Log("No renderer found!");

    }
    void Update()
    {// Checks for any changes in state. Maybe a bit slower than just changing it anytime it's changed in TileRecieveSignal
        mainRenderer.material.SetFloat("_Mode", state);
    }

    public void TileRecieveSignal(int newState)
    {// Public function to change the state. Technically requires a 0 signal to be sent when you don't want a tile lit up...oh well
        state = newState;
    }
}
