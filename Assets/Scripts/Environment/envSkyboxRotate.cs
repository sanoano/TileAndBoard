using UnityEngine;

public class envSkyboxRotate : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 0.9f;
    private void Start()
    {
        RenderSettings.skybox.SetFloat("_Rotation", 0);
    }

    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotateSpeed);
    }
}
