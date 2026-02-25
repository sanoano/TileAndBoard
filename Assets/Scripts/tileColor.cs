using UnityEngine;

public class tileColour : MonoBehaviour
{// For use with materials that use _shaderTileSelection. Script assigned to the tile itself to receive signals.
    //[SerializeField] private int state = 0; //0 is no vfx, 1 is attack (red), 2 is defend (blue), 3 is move (yellow)
    private Renderer mainRenderer;

    void Start()
    {// Sets to default colour...kinda unnessesary but just a safeguard if the shader/mat defaults get edited somehow.
        TryGetComponent<Renderer>(out mainRenderer);

        if (mainRenderer != null)
            mainRenderer.material.SetFloat("_Mode", 0);
    }

    public void TileRecieveSignal(int newState)
    {// Public function to change the state. Technically requires a 0 signal to be sent when you don't want a tile lit up...oh well
        mainRenderer.material.SetFloat("_Mode", newState);
    }
}
