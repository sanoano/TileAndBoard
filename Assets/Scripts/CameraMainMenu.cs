using UnityEngine;

public class CameraMainMenu : MonoBehaviour
{//Controls the position of the camera in the main menu, for cosmetic purposes. Switches between preset values. Controlled by buttons in-scene.

    [SerializeField] private float cameraDistance = 8.0f; //Default distance the camera is from its pivot

    private Vector3 pivotPos0, pivotPos1, pivotPos2;
    private Quaternion pivotAngles0, pivotAngles1, pivotAngles2;

    void Awake()
    {//Here all the position values are assigned.

        //State 0 default pos, hovering over the island, looking down
        pivotAngles0 = Quaternion.Euler(23.57f, -42.6f, 0.0f);
        pivotPos0 = new Vector3(-0.79f, 0, 3.04f);

        //State 1 birds-eye view of the island
        pivotAngles1 = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        pivotPos1 = new Vector3(0.0f, 0.0f, 0.0f);

        //State 2 lower shot of the island
        pivotAngles2 = Quaternion.Euler(15.8f, -274.0f, 0.0f);
        pivotPos2 = new Vector3(0.74f, 2.34f, 0.0f);

        //Makes sure things are at State 0
        transform.rotation = pivotAngles0;
        transform.position = pivotPos0;
    }

    public void SetCameraState(int state)
    {
        //need 2 add some kind of lerp between the current and target transforms, but for now you get the idea
        if (state == 1)
        {
            transform.rotation = pivotAngles1;
            transform.position = pivotPos1;
        }
        else if (state == 2)
        {
            transform.rotation = pivotAngles2;
            transform.position = pivotPos2;
        }
        else
        {
            transform.rotation = pivotAngles0;
            transform.position = pivotPos0;
        }
    }
}
