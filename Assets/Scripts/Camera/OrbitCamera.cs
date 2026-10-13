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
    [SerializeField] private Vector3 acrossBoardOffset = new Vector3(0f, 65f, -90f);
    [SerializeField] private Vector3 acrossBoardLookOffset = new Vector3(0f, 0f, -21f);

    private float yaw;
    private float pitch;
    private float defaultYaw;
    private float defaultPitch;

    private Vector3 cameraStaticPostion;
    private Quaternion cameraStaticRotation;
    private Vector3 sideViewPosition;
    private Quaternion sideViewRotation;
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
        sideViewPosition = transform.position;
        sideViewRotation = transform.rotation;
        cameraState = CameraState.Static;
        ApplyDefaultView(Settings.UsesAcrossBoardView);
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    public void ApplyDefaultView(bool acrossBoard)
    {
        cameraStaticPostion = sideViewPosition;
        cameraStaticRotation = sideViewRotation;

        if (acrossBoard)
        {
            Vector3 offset = acrossBoardOffset;
            Vector3 lookOffset = acrossBoardLookOffset;
            if (GameManager.instance.playerId == Player.PlayerId.Player2)
            {
                offset = Quaternion.Euler(0f, 180f, 0f) * offset;
                lookOffset = Quaternion.Euler(0f, 180f, 0f) * lookOffset;
            }

            cameraStaticPostion = target.position + offset;
            cameraStaticRotation = Quaternion.LookRotation(target.position + lookOffset - cameraStaticPostion);
        }

        defaultPitch = cameraStaticRotation.eulerAngles.x;
        defaultYaw = cameraStaticRotation.eulerAngles.y;

        if (cameraState == CameraState.Static)
        {
            gameObject.CancelTweens();
            transform.SetPositionAndRotation(cameraStaticPostion, cameraStaticRotation);
        }

        BoardManager.Instance.UpdatePlacedCardOrientations();
        UIManager.Instance.SetHealthDisplayView(acrossBoard);
    }

    public Quaternion GetPlacedCardRotation(Player.PlayerId owner)
    {
        if (Settings.UsesAcrossBoardView)
            return Quaternion.Euler(90f, GameManager.instance.playerId == Player.PlayerId.Player1 ? 0f : 180f, 0f);

        return sideViewRotation * Quaternion.Euler(45f, 0f,
            owner == GameManager.instance.playerId ? -90f : 90f);
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
        gameObject.CancelTweens();
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
                
            //UIManager.Instance.DestroyCurrentInfoInstance();
            //UIManager.Instance.Canvas.SetActive(false);
            //CardManager.instance.cardHoldPosition.SetActive(false);
        }
    }
}


