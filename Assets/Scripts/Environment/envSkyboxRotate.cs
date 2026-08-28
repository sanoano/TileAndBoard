using UnityEngine;

public class envSkyboxRotate : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 0.9f;
    private Material runtimeSkybox;

    private void Start()
    {
        runtimeSkybox = new Material(RenderSettings.skybox);
        RenderSettings.skybox = runtimeSkybox;

        runtimeSkybox.SetFloat("_Rotation", 0);
    }

    void Update()
    {
        runtimeSkybox.SetFloat("_Rotation", Time.time * rotateSpeed);
    }
}
