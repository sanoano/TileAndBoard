using System;
using System.Collections;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float sensitivity = 5f;
    [SerializeField] private float orbitRadius = 5f;

    [SerializeField] private float minimumOrbitDistance = 2f;
    [SerializeField] private float maximumOrbitDistance = 10f;

    private float yaw;
    private float pitch;
    private float defaultYaw;
    private float defaultPitch;

    private Vector3 cameraStaticPostion;
    private Quaternion cameraStaticRotation;
    public CameraState cameraState;

    public enum CameraState
    {
        Static,
        Free
    }

    private void Awake()
    {
        cameraStaticPostion = transform.position;
        cameraStaticRotation = transform.rotation;
        defaultPitch = transform.eulerAngles.x;
        defaultYaw = transform.eulerAngles.y;


        cameraState = CameraState.Static;
    }

    void Start() 
    {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update() 
    {

        if (Input.GetKeyUp(KeyCode.C))
        {
            if (cameraState == CameraState.Static)
            {
                cameraState = CameraState.Free;
            }
            else
            {
                cameraState = CameraState.Static;
            }

            if (cameraState == CameraState.Static)
            {
                StopAllCoroutines();
                StartCoroutine(lerpCamera());
                
            }
            else
            {
                orbitRadius = 35;
                yaw = defaultYaw;
                pitch = defaultPitch;
            }

        }
        
        
        if (Input.GetMouseButton(1) && cameraState == CameraState.Free) 
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            pitch -= mouseY * sensitivity;

            bool isUpsideDown = pitch > 90f || pitch < -90f;

            // Invert yaw input if the camera is upside down
            if (isUpsideDown) {
                yaw -= mouseX * sensitivity;
            } else {
                yaw += mouseX * sensitivity;
            }

            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }

        if (cameraState == CameraState.Free)
        {
            orbitRadius -= Input.mouseScrollDelta.y / sensitivity;
            orbitRadius = Mathf.Clamp(orbitRadius, minimumOrbitDistance, maximumOrbitDistance);

            transform.position = target.position - transform.forward * orbitRadius;
            
            UIManager.Instance.DestroyCurrentInfoInstance();
        }
        
    }

    public IEnumerator lerpCamera()
    {

        var animSpeed = 0.5f;

        float progress = 0.0f;  //This value is used for LERP

        while (progress < 1.0f)
        {
            transform.position = Vector3.Lerp(transform.position, cameraStaticPostion, progress);
            transform.rotation = Quaternion.Lerp(transform.rotation, cameraStaticRotation, progress);
            yield return new WaitForEndOfFrame();
            progress += Time.deltaTime * animSpeed;
        }

        //Set final transform
        transform.position = cameraStaticPostion;
        transform.rotation = cameraStaticRotation;
    }
}