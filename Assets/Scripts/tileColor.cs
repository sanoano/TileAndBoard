using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class tileColour : MonoBehaviour
{// For use with materials that use _shaderTileSelection. Script assigned to the tile itself to receive signals.
    //[SerializeField] private int state = 0; //0 is no vfx, 1 is attack (red), 2 is defend (blue), 3 is move (yellow)
    private Renderer mainRenderer;
    private float defaultEmmission = 1.0f;
    private float previewEmmission = 0f;

    [Header("Damage Numbers")]
    private Camera mainCamera;
    [SerializeField] GameObject grossDamageGO, netDamageGO;
    [SerializeField] TextMeshProUGUI grossDamageTMP, netDamageTMP;

    void Start()
    {// Sets to default colour...kinda unnessesary but just a safeguard if the shader/mat defaults get edited somehow.
        mainCamera = Camera.main;
        TryGetComponent<Renderer>(out mainRenderer);

        if (mainRenderer != null)
            TileRecieveSignal(0, false);


        grossDamageGO.SetActive(false);
        netDamageGO.SetActive(false);
        
        // if (gameObject.layer == LayerMask.NameToLayer("Player2Tile"))
        // {
        //     grossDamageGO.gameObject.transform.localPosition = new Vector3(grossDamageGO.gameObject.transform.localPosition.x,
        //         -grossDamageGO.gameObject.transform.localPosition.y,
        //         grossDamageGO.gameObject.transform.localPosition.z);
        //     
        //     netDamageGO.gameObject.transform.localPosition = new Vector3(netDamageGO.gameObject.transform.localPosition.x,
        //         -netDamageGO.gameObject.transform.localPosition.y,
        //         netDamageGO.gameObject.transform.localPosition.z);
        //     
        // }
        
        grossDamageGO.gameObject.transform.localScale = new Vector3(-grossDamageGO.gameObject.transform.localScale.x,
            grossDamageGO.gameObject.transform.localScale.y,
            grossDamageGO.gameObject.transform.localScale.z);
        
        netDamageGO.gameObject.transform.localScale = new Vector3(-netDamageGO.gameObject.transform.localScale.x,
            netDamageGO.gameObject.transform.localScale.y,
            netDamageGO.gameObject.transform.localScale.z);
    }

    void Update()
    {
        grossDamageGO.transform.LookAt(mainCamera.transform);
        netDamageGO.transform.LookAt(mainCamera.transform);

        
    }

    public void TileRecieveSignal(int newState, bool preview)
    {// Public function to change the state. Technically requires a 0 signal to be sent when you don't want a tile lit up...oh well.
     // Preview just makes tiles emmit a lower glow for when card actions aren't confirmed.

        mainRenderer.material.SetFloat("_Mode", newState);

        if (newState == 0)
            mainRenderer.enabled = false;
        else
            mainRenderer.enabled = true;

        //We don't need to check for if the tile is in state 0 to turn of emmission as the shader handles that automatically c:
        if (preview)
            mainRenderer.material.SetFloat("_Emmission_Intensity", previewEmmission);
        else if (!preview)
            mainRenderer.material.SetFloat("_Emmission_Intensity", defaultEmmission);
    }

    public void TileRecieveDamage(int Damage, int Defence)
    { //Updates the little numbers above each tile. Similar issue to the TileRecieveSignal is that it needs to be reset back to 0,0 and the element will hide itself

        int netAmount = Damage - Defence;
        if (netAmount < 0) netAmount = 0;
        
        if (Damage > Defence && Defence != 0 && Damage != 0)
        {// Dmg is larger than Def (red)
            grossDamageGO.SetActive(true);
            netDamageGO.SetActive(true);

            grossDamageTMP.text = Damage.ToString();
            netDamageTMP.text = netAmount.ToString();
        }
        else if (Defence >= Damage && Defence != 0 && Damage !=0)
        {// Def is larger than Dmg (blue)
            grossDamageGO.SetActive(true);
            netDamageGO.SetActive(true);

            grossDamageTMP.text = Damage.ToString();
            netDamageTMP.text = "0";
        }
        else if (Damage > 0 && Defence == 0)
        {// Dmg with no Def at all (red)
            grossDamageGO.SetActive(false);
            netDamageGO.SetActive(true);

            grossDamageTMP.text = "";
            netDamageTMP.text = Damage.ToString();
        }
        else if (Damage == 0 && Defence > 0)
        {// Def with no Dmg
            grossDamageGO.SetActive(false);
            netDamageGO.SetActive(true);

            grossDamageTMP.text = "";
            netDamageTMP.text = "0";
        }
        else if(Damage == 0 && Defence == 0)
        {//Nothing
            grossDamageGO.SetActive(false);
            netDamageGO.SetActive(false);

            grossDamageTMP.text = "";
            netDamageTMP.text = "";
        }
    }
}
