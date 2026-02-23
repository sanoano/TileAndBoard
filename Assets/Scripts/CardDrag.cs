using System;
using Tweens;
using UnityEngine;

public class CardDrag : MonoBehaviour
{

    public static bool isDragged;
    private Vector3 mousePosition;
    private Vector3 returnPosition;
    private Camera cam;
    private SpriteRenderer sr;
    private Collider collider;

    private LocalScaleTween growTween;
    private LocalScaleTween shrinkTween;

    public bool isPlaced;

    private Vector3 normalScale;
    private Vector3 bigScale;
    private Vector3 smallScale;

    [Header("Properties")] [SerializeField]
    private float animTime;

    private void Awake()
    {
        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
        collider = GetComponent<BoxCollider>();
        animTime = 0.25f;
        normalScale = transform.localScale;
        bigScale = new Vector3(transform.localScale.x + 2, transform.localScale.y + 2, transform.localScale.z + 2);
        smallScale = new Vector3(transform.localScale.x - 2, transform.localScale.y - 2, transform.localScale.z - 2);
        isDragged = false;
    }

    private Vector3 GetMousePos()
    {
        return cam.WorldToScreenPoint(transform.position);
    }

    private void OnMouseDown()
    {
        if (UIManager.Instance.interactionState != UIManager.InteractionState.None) return;
        if (isPlaced) return;
        if (CardManager.instance.cardDrawInProgress) return;
        mousePosition = Input.mousePosition - GetMousePos();
        isDragged = true;
        returnPosition = transform.position;

        shrinkTween = new LocalScaleTween()
        {
            to = smallScale,
            duration = animTime,
            easeType = EaseType.ElasticOut
        };

        gameObject.AddTween(shrinkTween);
    }

    private void OnMouseDrag()
    {
        if (UIManager.Instance.interactionState != UIManager.InteractionState.None) return;
        if (isPlaced) return;
        if (CardManager.instance.cardDrawInProgress) return;
        transform.position = Vector3.Slerp(transform.position, cam.ScreenToWorldPoint(Input.mousePosition - mousePosition), 0.18f);
    }

    private void OnMouseUp()
    {
        if (UIManager.Instance.interactionState != UIManager.InteractionState.None) return;
        if (isPlaced) return;
        if (CardManager.instance.cardDrawInProgress) return;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, BoardManager.Instance.playerSpecificLayer))
        {

            int position = CardManager.instance.playerHandVisuals.IndexOf(gameObject);

            Vector2Int tileCoords =
                BoardManager.Instance.CoordinatesOf<GameObject>(BoardManager.Instance.localBoard.TileTransforms,
                    hit.transform.gameObject);

            if (BoardManager.Instance.localBoard.Visuals[tileCoords.x, tileCoords.y] == null)
            {
                isDragged = false;
                BoardManager.Instance.PlaceCard(this.gameObject,
                    CardManager.instance.playerHand[position],
                    hit.transform.gameObject);
                
            }
            else
            {
                isDragged = false;
                var tween = new PositionTween
                {
                    to = returnPosition,
                    duration = 0.5f,
                    easeType = EaseType.ElasticOut
                };
                print("balls");
                growTween = new LocalScaleTween()
                {
                    to = normalScale,
                    duration = animTime,
                    easeType = EaseType.ElasticOut
                };

                gameObject.AddTween(growTween);
                gameObject.AddTween(tween);
            }
        }
        else
        {
            isDragged = false;
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
        
    }

    private void OnMouseEnter()
    {
        if (UIManager.Instance.interactionState != UIManager.InteractionState.None) return;
        if (isPlaced) return;
        if (isDragged) return;
        if (CardManager.instance.cardDrawInProgress) return;
        var pos = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + 0.01f);
        transform.localPosition = pos;

        sr.sortingOrder = 1;
        collider.layerOverridePriority = 1;

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
        if (isPlaced) return;
        if (isDragged) return;
        if (CardManager.instance.cardDrawInProgress) return;
        var pos = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        transform.localPosition = pos;
        
        sr.sortingOrder = 0;
        collider.layerOverridePriority = 0;

        shrinkTween = new LocalScaleTween()
        {
            to = normalScale,
            duration = animTime,
            easeType = EaseType.ElasticOut
        };

        gameObject.AddTween(shrinkTween);
    }
}
