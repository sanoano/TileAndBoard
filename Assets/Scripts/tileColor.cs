using UnityEngine;

public class tileColor : MonoBehaviour
{// For use with materials that use _shaderTileSelection. Script assigned to the tile itself to receive signals.
    [SerializeField] private int state = 0; //0 is no vfx, 1 is attack, 2 is defend, 3 is move
    private Material mat;
    void Start()
    {// Sets to default colour
        mat = GetComponent<Material>();
        mat.SetFloat("_Mode", 0);
    }
    void Update()
    {// Checks for any changes in state
        mat.SetFloat("_Mode", state);
    }

    public void TileRecieveSignal(int newState)
    {// Public function to change the state
        state = newState;
        mat.SetFloat("_Mode", state);
    }
}
