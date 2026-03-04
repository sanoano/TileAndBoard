using TMPro;
using UnityEngine;

public class tileColour : MonoBehaviour
{// For use with materials that use _shaderTileSelection. Script assigned to the tile itself to receive signals.
    //[SerializeField] private int state = 0; //0 is no vfx, 1 is attack (red), 2 is defend (blue), 3 is move (yellow)
    private Renderer mainRenderer;
    private float defaultEmmission = .5f;
    private float previewEmmission = .25f;

    [Header("Damage Numbers")]
    private Camera mainCamera;
    [SerializeField] GameObject grossDamageGO, netDamageGO;
    [SerializeField] TextMeshProUGUI grossDamageTMP, netDamageTMP;

    void Start()
    {// Sets to default colour...kinda unnessesary but just a safeguard if the shader/mat defaults get edited somehow.
        mainCamera = Camera.main;
        TryGetComponent<Renderer>(out mainRenderer);

        if (mainRenderer != null)
            mainRenderer.material.SetFloat("_Mode", 0);
    }

    void Update()
    {
        grossDamageGO.transform.LookAt(mainCamera.transform);
        netDamageGO.transform.LookAt(mainCamera.transform);

        /*if (grossDamageTMP.text == "0" && netDamageTMP.text == "0") commented out so we can see the numbers in action......
        {
            grossDamageGO.SetActive(false);
            netDamageGO.SetActive(false);
        }
        else
        {
            grossDamageGO.SetActive(true);
            netDamageGO.SetActive(true);
        }*/
    }

    public void TileRecieveSignal(int newState, bool preview)
    {// Public function to change the state. Technically requires a 0 signal to be sent when you don't want a tile lit up...oh well.
     // Preview just makes tiles emmit a lower glow for when card actions aren't confirmed.

        mainRenderer.material.SetFloat("_Mode", newState);

        //We don't need to check for if the tile is in state 0 to turn of emmission as the shader handles that automatically c:
        if (preview)
            mainRenderer.material.SetFloat("_Emmission_Intensity", previewEmmission);
        else if (!preview)
            mainRenderer.material.SetFloat("_Emmission_Intensity", defaultEmmission);
    }

    public void TileRecieveDamage(int Damage, int Defence)
    {//Updates the little numbers above each tile. Similar issue to the TileRecieveSignal is that it needs to be reset back to 0,0 and the element will hide itself
        if (Damage > Defence && Defence != 0)
        {// Dmg is larger than Def (red)
            grossDamageGO.SetActive(true);
            netDamageGO.SetActive(true);

            grossDamageTMP.text = Damage.ToString();
            netDamageTMP.text = (Damage - Defence).ToString();
        }
        else if (Defence >= Damage && Defence != 0)
        {// Def is larger than Dmg (blue)
            grossDamageGO.SetActive(true);
            netDamageGO.SetActive(true);

            grossDamageTMP.text = Damage.ToString();
            netDamageTMP.text = "0";
        }
        else if (Damage > 0 && Defence == 0)
        {// Dmg with no Def at all (red)
            netDamageGO.SetActive(true);

            grossDamageTMP.text = "";
            netDamageTMP.text = Damage.ToString();
        }
    }
}
