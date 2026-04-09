using UnityEngine;

public class envCastleRotate : MonoBehaviour
{// Rotates the castle object in the background slowly. Script added to both the pivot and castle object itself, but adjust the castle's rotateSpeed to something close to 0 
    [SerializeField] private float rotateSpeed = 5.0f;

    float yRotation = 0.0f;

    void Update()
    {
        yRotation += rotateSpeed * Time.deltaTime;

        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}
