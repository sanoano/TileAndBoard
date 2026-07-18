using UnityEngine;

public class envPassiveRotate : MonoBehaviour
{// Rotates things on their Y axis. Just slap it on and set the speed as you please. Or don't idgaf.
    [SerializeField] private float rotateSpeed = 5.0f;

    private float yRotation;// = 0.0f;
    private float startingRot;// = 0.0f;

    private void Awake()
    {
        startingRot = transform.eulerAngles.y;
    }
    void Update()
    {
        yRotation += rotateSpeed * Time.deltaTime;

        transform.rotation = Quaternion.Euler(0f, startingRot + yRotation, 0f);
    }
}
