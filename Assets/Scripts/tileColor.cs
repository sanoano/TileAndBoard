using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class tileColour : MonoBehaviour
{// For use with materials that use _shaderTileSelection. Script assigned to the tile itself to receive signals.
    //[SerializeField] private int state = 0; //0 is no vfx, 1 is attack (red), 2 is defend (blue), 3 is move (yellow)
    private Renderer mainRenderer, previewRenderer;
    //private float defaultEmmission = 1.0f;
    //private float previewEmmission = 0f;

    [SerializeField] private GameObject previewSpace;

    [Header("Damage Numbers")]
    private Camera mainCamera;
    private Canvas mainCanvas;
    [SerializeField] private GameObject grossDamage, netDamage;
    private TextMeshProUGUI grossDamageTMP, netDamageTMP;

    void Start()
    {// Sets to default colour...kinda unnessesary but just a safeguard if the shader/mat defaults get edited somehow.
        mainCamera = Camera.main;
        TryGetComponent<Renderer>(out mainRenderer);
        previewRenderer = previewSpace.GetComponent<Renderer>();
        mainCanvas = gameObject.GetComponentInChildren<Canvas>();


        if (mainRenderer != null)
            TileRecieveSignal(0, false);

        grossDamageTMP = grossDamage.GetComponent<TextMeshProUGUI>();
        netDamageTMP = netDamage.GetComponent<TextMeshProUGUI>();

        grossDamage.SetActive(false);
        netDamage.SetActive(false);
        
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
        
        grossDamage.gameObject.transform.localScale = new Vector3(-grossDamage.gameObject.transform.localScale.x,
            grossDamage.gameObject.transform.localScale.y,
            grossDamage.gameObject.transform.localScale.z);
        
        netDamage.gameObject.transform.localScale = new Vector3(-netDamage.gameObject.transform.localScale.x,
            netDamage.gameObject.transform.localScale.y,
            netDamage.gameObject.transform.localScale.z);
    }

    void Update()
    {
        grossDamage.transform.LookAt(mainCamera.transform);
        netDamage.transform.LookAt(mainCamera.transform);

        
    }

    public void TileRecieveSignal(int newState, bool preview)
    {// Public function to change the state. Technically requires a 0 signal to be sent when you don't want a tile lit up...oh well.
     // Preview makes a translucent shape appear instead of the tile.

        if (newState == 0)
        {
            previewRenderer.enabled = false;
            mainRenderer.enabled = false;
        }
        else
        {
            if (preview)
            {
                previewRenderer.enabled = true;
                previewRenderer.material.SetFloat("_Mode", newState);
            }
            else
            {
                mainRenderer.enabled = true;
                mainRenderer.material.SetFloat("_Mode", newState);
            }
            
        }
    }

    public void TileRecieveDamage(int Damage, int Defence)
    { //Updates the little numbers above each tile. Similar issue to the TileRecieveSignal is that it needs to be reset back to 0,0 and the element will hide itself

        int netAmount = Damage - Defence;
        if (netAmount < 0) netAmount = 0;
        
        if (Damage > Defence && Defence != 0 && Damage != 0)
        {// Dmg is larger than Def (red)
            grossDamage.SetActive(true);
            netDamage.SetActive(true);

            grossDamageTMP.text = Damage.ToString();
            netDamageTMP.text = netAmount.ToString();
        }
        else if (Defence >= Damage && Defence != 0 && Damage !=0)
        {// Def is larger than Dmg (blue)
            grossDamage.SetActive(true);
            netDamage.SetActive(true);

            grossDamageTMP.text = Damage.ToString();
            netDamageTMP.text = "0";
        }
        else if (Damage > 0 && Defence == 0)
        {// Dmg with no Def at all (red)
            grossDamage.SetActive(false);
            netDamage.SetActive(true);

            grossDamageTMP.text = "";
            netDamageTMP.text = Damage.ToString();
        }
        else if (Damage == 0 && Defence > 0)
        {// Def with no Dmg
            grossDamage.SetActive(false);
            netDamage.SetActive(true);

            grossDamageTMP.text = "";
            netDamageTMP.text = "";
        }
        else if(Damage == 0 && Defence == 0)
        {//Nothing
            grossDamage.SetActive(false);
            netDamage.SetActive(false);

            grossDamageTMP.text = "";
            netDamageTMP.text = "";
        }
    }

    public void TileRecievePopup(int amount, int type)
    {//0 is HP, 1 is Manna, 2 is LP
        UIPopupNumbers.Create(mainCanvas.transform.position, mainCanvas.transform, amount, type);
    }
}
