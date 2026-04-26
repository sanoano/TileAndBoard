using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class UIPopupNumbers : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numbersTMP;
    [SerializeField] private Image icon;
    private float disappearTimer = .5f;
    private Color textColor;
    [SerializeField] private Sprite[] icons;//0 is HP, 1 is Manna. LP don't have no icon

    public void Setup(float amount, int type)
    {
        if (type == 0 || type == 2)
            if (amount > 0)
                numbersTMP.text = "-" + amount.ToString();//need 2 fix this if it's a positive value ie successfully defended
            else
                numbersTMP.text = amount.ToString();
        else
                numbersTMP.text = "+" + amount.ToString();



        if (type < 2)
        {
            icon.enabled = true;
            icon.sprite = icons[type];
        }
        else
            icon.enabled = false;

        //remind me to add code that makes negative numbers red
            
    }
    public static UIPopupNumbers Create(Vector3 position, Transform parent, float amount, int type)
    {
        Transform popupTransform = Instantiate(GameAssets.i.prefabPopupNumbers.transform, parent, true);
        popupTransform.localPosition = Vector3.zero;

        UIPopupNumbers popupNumbers = popupTransform.GetComponent<UIPopupNumbers>();
        popupNumbers.Setup(amount, type); 

        return popupNumbers;
    }
    private void Update()
    {
        float moveY = 7.5f;
        transform.position += new Vector3(0.0f, moveY, 0.0f) * Time.deltaTime;

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            //start disappearing
            float disappearSpeed = 6f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            numbersTMP.color = textColor;
            icon.color = textColor;
            if (textColor.a < 0) 
            {
                Destroy(gameObject);
            }
        }

        gameObject.transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
    }
}