using System;
using Tweens;
using UnityEngine;

public class CardDrag : MonoBehaviour
{

    public bool isDragged;
    private Vector3 mousePosition;
    private Vector3 returnPosition;
    private Camera cam;
    private SpriteRenderer sr;
    private Collider collider;

    private void Awake()
    {
        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
        collider = GetComponent<BoxCollider>();
    }

    private Vector3 GetMousePos()
    {
        return cam.WorldToScreenPoint(transform.position);
    }

    private void OnMouseDown()
    {
        mousePosition = Input.mousePosition - GetMousePos();
        isDragged = true;
        returnPosition = transform.position;
    }

    private void OnMouseDrag()
    {
        transform.position = Vector3.Slerp(transform.position, cam.ScreenToWorldPoint(Input.mousePosition - mousePosition), 0.18f);
    }

    private void OnMouseUp()
    {

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, BoardManager.Instance.playerSpecificLayer))
        {

            int position = CardManager.instance.playerHandVisuals.IndexOf(gameObject);
            
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

            gameObject.AddTween(tween);
        }

        

    }

    private void OnMouseEnter()
    {
        if (isDragged) return;
        var pos = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + 0.01f);
        transform.localPosition = pos;

        sr.sortingOrder = 1;
        collider.layerOverridePriority = 1;
    }

    private void OnMouseOver()
    {
        
    }

    private void OnMouseExit()
    {
        if (isDragged) return;
        var pos = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        transform.localPosition = pos;
        
        sr.sortingOrder = 0;
        collider.layerOverridePriority = 0;
    }
}
