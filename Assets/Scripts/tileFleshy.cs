using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class fxFleshies : MonoBehaviour
{// Gives the fleshies their weird pulsating look

    [SerializeField] private float scale;
    [SerializeField] private float waveSpeed;
    [SerializeField] private float waveHeight;
    private Renderer renderer;
    private bool doOnce;

    private void Start()
    {
        renderer = gameObject.GetComponent<Renderer>();
    }
    void Update()
    {
        CalcNoise();

        if (Input.GetKeyDown("p"))
        {
            StartCoroutine(Pulse());
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

    public IEnumerator Pulse()
    {
        if (!doOnce)
        {
            doOnce = true;

            renderer.material.SetInt("_isFlashing", 1);
            waveSpeed = waveSpeed * 2;

            yield return new WaitForSeconds(0.5f);

            renderer.material.SetInt("_isFlashing", 0);
            waveSpeed = waveSpeed / 2;

            doOnce = false;
        }

        yield return null;
    }
}