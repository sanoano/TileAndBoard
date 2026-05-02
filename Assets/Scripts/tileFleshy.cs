using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class tileFleshy : MonoBehaviour
{// Gives the fleshies their weird pulsating look

    [SerializeField] private float scale;
    [SerializeField] private float waveSpeed;
    [SerializeField] private float waveHeight;
    //[SerializeField] private float flashLenghth = 0.5f;
    private Renderer renderer;
    private bool doOnce = false;

    private void Start()
    {
        renderer = gameObject.GetComponent<Renderer>();
    }
    void Update()
    {
        CalcNoise();

        if (Input.GetKeyDown("p"))
        {
            StartCoroutine(Pulse(0.35f));
            //StartSinglePulse(350.0f);
        }
            
    }

    void CalcNoise()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        Vector3[] verts = filter.mesh.vertices;

        for (int i = 0; i < verts.Length; i++)
        {
            float pX = (verts[i].x * scale) + (Time.time * waveSpeed);
            float pZ = (verts[i].z * scale) + (Time.time * waveSpeed);

            verts[i].y = Mathf.PerlinNoise(pX, pZ) * waveHeight;
        }

        filter.mesh.vertices = verts;
        filter.mesh.RecalculateNormals();
        filter.mesh.RecalculateBounds();
    }

    public void StartSinglePulse(float waitInMiliseconds)
    {
        Pulse(waitInMiliseconds / 1000);
    }

    public IEnumerator Pulse(float waitTime)
    {
        if (!doOnce)
        {
            doOnce = true;

            renderer.material.SetInt("_isFlashing", 1);
            waveSpeed = waveSpeed * 2;

            yield return new WaitForSeconds(waitTime);

            renderer.material.SetInt("_isFlashing", 0);
            waveSpeed = waveSpeed / 2;

            doOnce = false;
        }

        yield return null;
    }
}