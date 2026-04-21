using System;
using System.Collections;
using Tweens;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    
    [Header("Sensitivity")]
    [SerializeField] private Transform target;
    [SerializeField] private float sensitivity = 5f;
    [SerializeField] private float orbitRadius = 5f;

    [Header("Orbit Distance")]
    [SerializeField] private float minimumOrbitDistance = 2f;
    [SerializeField] private float maximumOrbitDistance = 10f;

    [Header("Tween")]
    [SerializeField] private float returnDuration;

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
        
    }

    void Start()
    {
        cameraStaticPostion = transform.position;
        cameraStaticRotation = transform.rotation;
        defaultPitch = transform.eulerAngles.x;
        defaultYaw = transform.eulerAngles.y;


        cameraState = CameraState.Static;
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update()
    {

        if (Input.GetKeyUp(KeyCode.C) && UIManager.Instance.interactionState == UIManager.InteractionState.None)
        {
            SwapCameraMode();
        }


        if (Input.GetMouseButton(1) && cameraState == CameraState.Free)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            pitch -= mouseY * sensitivity;

            bool isUpsideDown = pitch > 90f || pitch < -90f;

            // Invert yaw input if the camera is upside down
            if (isUpsideDown)
            {
                yaw -= mouseX * sensitivity;
            }
            else
            {
                yaw += mouseX * sensitivity;
            }

            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }
        
        if (cameraState == CameraState.Free)
        {
            orbitRadius -= Input.mouseScrollDelta.y / sensitivity;
            orbitRadius = Mathf.Clamp(orbitRadius, minimumOrbitDistance, maximumOrbitDistance);

            transform.position = target.position - transform.forward * orbitRadius;

            
        }

    }

    public void SwapCameraMode()
    {
        if (cameraState == CameraState.Static)
        {
            cameraState = CameraState.Free;
        }
        else
        {
            cameraState = CameraState.Static;
        }

        //If static camera  has just been enabled
        if (cameraState == CameraState.Static)
        {
            var positionTween = new PositionTween
            {
                to = cameraStaticPostion,
                duration = returnDuration,
                easeType = EaseType.ElasticOut,
            };

            var rotationTween = new RotationTween
            {
                to = cameraStaticRotation,
                duration = returnDuration,
                easeType = EaseType.ElasticOut
            };

            gameObject.AddTween(positionTween);
            gameObject.AddTween(rotationTween);
            UIManager.Instance.Canvas.SetActive(true);
            CardManager.instance.cardHoldPosition.SetActive(true);
                

        }
        //If free cam has just been enabled
        else
        {
            orbitRadius = 75;
            yaw = defaultYaw;
            pitch = defaultPitch;
                
            UIManager.Instance.DestroyCurrentInfoInstance();
            UIManager.Instance.Canvas.SetActive(false);
            CardManager.instance.cardHoldPosition.SetActive(false);
        }
    }
}

  