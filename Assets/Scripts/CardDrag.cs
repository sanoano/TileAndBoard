using System;
using System.Collections.Generic;
using Tweens;
using UnityEngine;

public class CardDrag : MonoBehaviour
{

    public static bool isDragged;
    private bool isDraggedLocal;
    private Vector3 mousePosition;
    private Vector3 returnPosition;
    private Camera cam;
    private SpriteRenderer sr;
    private Collider collider;
    private OrbitCamera orbitCamera;
    private Canvas canvas;
    private SpriteRenderer[] canvasChildren;

    private LocalScaleTween growTween;
    private LocalScaleTween shrinkTween;

    public bool isPlaced;

    private Vector3 normalScale;
    private Vector3 bigScale;
    private Vector3 smallScale;

    private GameObject currentHoveredTile;

    [Header("Movement Settings")] 
    public float followSpeed = 15f;

    [Header("Tilt Settings")] 
    public float baseTiltX = 0;
    public float maxTiltZ = 35f;
    public float tiltSensitivity = 15f;
    public float tiltDamping = 10f;
    private Vector3 targetWorldPosition;
    
    private float currentTiltZ;
    
    private Plane dragPlane;
    
    [Header("Anim Settings")] [SerializeField]
    private float animTime;
    
    [Header("SR Children")]
    [SerializeField] private List<SpriteRenderer> srChildren = new List<SpriteRenderer>();

    private void Awake()
    {
        cam = Camera.main;
        orbitCamera = cam.GetComponent<OrbitCamera>();
        canvas = GetComponentInChildren<Canvas>();
        canvasChildren = canvas.GetComponentsInChildren<SpriteRenderer>(true);
        sr = GetComponent<SpriteRenderer>();
        collider = GetComponent<BoxCollider>();
        animTime = 0.25f;
        normalScale = transform.localScale;
        bigScale = new Vector3(transform.localScale.x + 1f, transform.localScale.y + 1f,
            transform.localScale.z + 1f);
        smallScale = new Vector3(transform.localScale.x - 1.5f, transform.localScale.y - 1.5f,
            transform.localScale.z - 1.5f);
        isDragged = false;
        isDraggedLocal = false;

        targetWorldPosition = transform.position;


    }


    private void OnMouseDown()
    {
        if (UIManager.Instance.interactionState != UIManager.InteractionState.None) return;
        if (UIManager.Instance.settingsMenu.activeSelf) return;
        if (isPlaced) return;
        if (CardManager.instance.cardDrawInProgress) return;
        if (orbitCamera.cameraState == OrbitCamera.CameraState.Free) return;
        if (BoardManager.Instance.attackInProgress) return;
        isDragged = true;
        isDraggedLocal = true;
        returnPosition = transform.position;
        dragPlane = new Plane(cam.transform.forward, transform.position);

        shrinkTween = new LocalScaleTween()
        {
            to = smallScale,
            duration = animTime,
            easeType = EaseType.ElasticOut
        };

        gameObject.AddTween(shrinkTween);
        

        // foreach (var unit in BoardManager.Instance.unitsList)
        // {
        //     if (unit.ID == GameManager.instance.playerId)
        //     {
        //         BoardManager.Instance.localBoard.TileTransforms[unit.Position.x, unit.Position.y]
        //             .GetComponent<tileColour>()
        //             .TileRecieveSignal(1, false);
        //     }
        // }
    }

    void Update()
    {
        if (!isPlaced)
        {
            if (isDraggedLocal)
            {
                if (UIManager.Instance.interactionState != UIManager.InteractionState.None) return;
                if (isPlaced) return;
                if (CardManager.instance.cardDrawInProgress) return;

                //Colour tile that card is currently above
                Ray ray2 = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray2, out hit, Mathf.Infinity, BoardManager.Instance.playerSpecificLayer))
                {
                    if (currentHoveredTile == null)
                    {
                        currentHoveredTile = hit.transform.gameObject;
                        currentHoveredTile.GetComponent<tileColour>().TileRecieveSignal(3, true);
                    }
                    else
                    {
                        BoardManager.Instance.ClearTiles();
                        BoardManager.Instance.UpdateTileVisuals();
                        currentHoveredTile = hit.transform.gameObject;
                        currentHoveredTile.GetComponent<tileColour>().TileRecieveSignal(3, true);
                    }
                }
                else
                {
                    if (currentHoveredTile != null)
                    {
                        BoardManager.Instance.ClearTiles();
                        BoardManager.Instance.UpdateTileVisuals();
                        currentHoveredTile = null;
                    }
                }


                //Card tilt when dragging
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if (dragPlane.Raycast(ray, out float distance))
                {
                    targetWorldPosition = ray.GetPoint(distance);
                }

                transform.position =
                    Vector3.Lerp(transform.position, targetWorldPosition, Time.deltaTime * followSpeed);

                Vector3 offset = targetWorldPosition - transform.position;

                float horizontalScreenDistance = Vector3.Dot(offset, cam.transform.right);

                // Map to tilt angle
                float targetTiltZ = -horizontalScreenDistance * tiltSensitivity;
                targetTiltZ = Mathf.Clamp(targetTiltZ, -maxTiltZ, maxTiltZ);

                currentTiltZ = Mathf.Lerp(currentTiltZ, targetTiltZ, Time.deltaTime * tiltDamping);
            }
            else
            {
                currentTiltZ = Mathf.Lerp(currentTiltZ, 0f, Time.deltaTime * tiltDamping);
            }


            Quaternion dynamicTilt = Quaternion.AngleAxis(currentTiltZ, cam.transform.forward);

            Quaternion parentRot = transform.parent != null ? transform.parent.rotation : Quaternion.identity;

            Quaternion baseLocalRot = Quaternion.Euler(baseTiltX, 0, 0);

            Quaternion targetWorldRot = dynamicTilt * parentRot * baseLocalRot;


            transform.localRotation = Quaternion.Inverse(parentRot) * targetWorldRot;
        }
        
        

    }
    
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        return cam.ScreenToWorldPoint(mousePos);
    }

    
    private void OnMouseUp()
    {
        if (UIManager.Instance.interactionState != UIManager.InteractionState.None) return;
        if (UIManager.Instance.settingsMenu.activeSelf) return;
        if (isPlaced) return;
        if (CardManager.instance.cardDrawInProgress) return;
        if (orbitCamera.cameraState == OrbitCamera.CameraState.Free) return;
        if (BoardManager.Instance.attackInProgress) return;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, BoardManager.Instance.playerSpecificLayer))
        {

            int position = CardManager.instance.playerHandVisuals.IndexOf(gameObject);

            Vector2Int tileCoords =
                BoardManager.Instance.CoordinatesOf<GameObject>(BoardManager.Instance.localBoard.TileTransforms,
                    hit.transform.gameObject);

            //Check if card can be placed, if so place it
            if (!TurnManager.instance.isYourTurn)
            {
                PlaceFailed(5);
                return;
            }
            
            if (BoardManager.Instance.localBoard.Visuals[tileCoords.x, tileCoords.y] != null)
            {
                PlaceFailed(6);
                return;
            }

            if (!ManaManager.instance.CanAfford(GetRangeCount())) 
            {
                PlaceFailed(1);
                return;
            }

            if (BoardManager.Instance.GetCardAmount(GameManager.instance.playerId) >=
                BoardManager.Instance.maxCardsPerPlayer)
            {
                PlaceFailed(3);
                return;
            }

            isDragged = false;
            isDraggedLocal = false;
            ManaManager.instance.RemoveManaPoints(GetRangeCount());
            BoardManager.Instance.PlaceCard(this.gameObject,
                CardManager.instance.playerHand[position],
                hit.transform.gameObject);
            if (currentHoveredTile != null)
            {
                //currentHoveredTile.GetComponent<tileColour>().TileRecieveSignal(0, true);
                currentHoveredTile = null;
            }
        }
        else
        {
            PlaceFailed(0);
            
        }
        
        BoardManager.Instance.ClearTiles();
        BoardManager.Instance.UpdateTileVisuals();
        
        if (sr.sortingOrder != 1)
        {
            sr.sortingOrder -= 10;
            foreach (var child in srChildren)
            {
                child.sortingOrder -= 10;
            }
        
            foreach (var child in canvasChildren)
            {
                child.sortingOrder -= 15;
            }
            
            canvas.sortingOrder -= 10;

        }
    }

    private void PlaceFailed(int error)
    {
        if (error > 0)
        {
            TextDialogue.instance.DialogueRecieveStatus(error);
        }
        
        if (currentHoveredTile != null)
        {
            currentHoveredTile.GetComponent<tileColour>().TileRecieveSignal(0, true);
            currentHoveredTile = null;
        }
        isDragged = false;
        isDraggedLocal = false;
        var tween = new PositionTween
        {
            to = returnPosition,
            duration = 0.5f,
            easeType = EaseType.ElasticOut
        };
            
        growTween = new LocalScaleTween()
        {
            to = normalScale,
            duration = animTime,
            easeType = EaseType.ElasticOut
        };

        gameObject.AddTween(growTween);

        gameObject.AddTween(tween);
    }

    private void OnMouseEnter()
    {
        if (UIManager.Instance.interactionState != UIManager.InteractionState.None) return;
        if (UIManager.Instance.settingsMenu.activeSelf) return;
        if (isPlaced) return;
        if (isDragged) return;
        if (CardManager.instance.cardDrawInProgress) return;
        if (orbitCamera.cameraState == OrbitCamera.CameraState.Free) return;
        if (BoardManager.Instance.attackInProgress) return;
        AudioManager.singleton.PlaySound("cardHover", true, 0.2f);
        var pos = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + 0.01f);
        transform.localPosition = pos;

        if (sr.sortingOrder == 1)
        {
            sr.sortingOrder += 10;
            foreach (var child in srChildren)
            {
                child.sortingOrder += 10;
            }
        
            foreach (var child in canvasChildren)
            {
                child.sortingOrder += 15;
            }
            
            canvas.sortingOrder += 10;
        }
        

        

        growTween = new LocalScaleTween
        {
            to = bigScale,
            duration = animTime,
            easeType = EaseType.ElasticOut
        };
        
        gameObject.AddTween(growTween);

    }
    

    private void OnMouseExit()
    {
        if (UIManager.Instance.interactionState != UIManager.InteractionState.None) return;
        if (UIManager.Instance.settingsMenu.activeSelf) return;
        if (isPlaced) return;
        if (isDragged) return;
        if (CardManager.instance.cardDrawInProgress) return;
        if (orbitCamera.cameraState == OrbitCamera.CameraState.Free) return;
        if (BoardManager.Instance.attackInProgress) return;
        var pos = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        transform.localPosition = pos;


        if (sr.sortingOrder != 1)
        {
            sr.sortingOrder -= 10;
            foreach (var child in srChildren)
            {
                child.sortingOrder -= 10;
            }
        
            foreach (var child in canvasChildren)
            {
                child.sortingOrder -= 15;
            }
            
            canvas.sortingOrder -= 10;

        }
        
        

        shrinkTween = new LocalScaleTween()
        {
            to = normalScale,
            duration = animTime,
            easeType = EaseType.ElasticOut
        };
        
        gameObject.AddTween(shrinkTween);
    }

    private int GetRangeCount()
    {
        return CardManager.instance.playerHand[CardManager.instance.playerHandVisuals.IndexOf(gameObject)].Range.Count;
    }
}
