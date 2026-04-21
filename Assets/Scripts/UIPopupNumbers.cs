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
        numbersTMP.SetText(amount.ToString());

        if (type < 2)
            icon.sprite = icons[type];
        else
            icon.sprite = null;

        //remind me to add code that makes negative numbers red
            
    }
    public static UIPopupNumbers Create(Vector3 position, float amount, int type)
    {
        Transform popupTransform = Instantiate(GameAssets.i.prefabPopupNumbers.transform, position, Quaternion.identity);

        UIPopupNumbers popupNumbers = popupTransform.GetComponent<UIPopupNumbers>();
        popupNumbers.Setup(amount, type); 

        return popupNumbers;
    }
    private void Update()
    {
        float moveY = 7.5f;
        transform.position += new Vector3(0, moveY) * Time.deltaTime;

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

        numbersTMP.transform.LookAt(Camera.main.transform);
        icon.transform.LookAt(Camera.main.transform);
    }
}