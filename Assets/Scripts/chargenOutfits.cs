using System;
using UnityEngine;
using UnityEngine.UI;

public class chargenOutfits : MonoBehaviour
{//Stores info on what meshes get enabled for each outfit and visually loads it on the character

    [SerializeField] GameObject[] torso0, torso1;
    [SerializeField] GameObject[] head0, head1, head2;
    [SerializeField] Image sprite;

    [SerializeField] Material skinMat, eyeMat, hairMat;

    private GameObject[][] torsoList, headList;
    private int currentHead, currentTorso;

    [Header("Data Files")]
    public Sprite[] sprites => GameAssets.i.characterSprites;

    void Awake()
    {
        /*torsoList = new GameObject[][] { torso0, torso1 };
        headList = new GameObject[][] { head0, head1, head2 };*/
    }

    public void loadTorso(int index)
    {
        foreach (GameObject[] torso in torsoList)
            foreach (GameObject segment in torso)
                segment.SetActive(false);


        foreach (GameObject segment in torsoList[index])
        {
            segment.SetActive(true);

            SkinnedMeshRenderer segmentRenderer = segment.GetComponent<SkinnedMeshRenderer>();

            if (segmentRenderer != null)
            {
                Material[] materials = segmentRenderer.materials;

                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i].name.Contains(skinMat.name))
                        materials[i].SetColor("_BaseColor", chargen.instance.skinColour);

                    segmentRenderer.materials = materials;
                }
            }
        }

        currentTorso = index;
    }

    public void loadHead(int index)
    {
        //Debug.Log(index);

        foreach (GameObject[] head in headList)
            foreach (GameObject segment in head)
                segment.SetActive(false);


        foreach (GameObject segment in headList[index])
        {
            segment.SetActive(true);

            SkinnedMeshRenderer segmentRenderer = segment.GetComponent<SkinnedMeshRenderer>();

            if (segmentRenderer != null)
            {
                Material[] materials = segmentRenderer.materials;

                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i].name.Contains(skinMat.name))
                        materials[i].SetColor("_BaseColor", chargen.instance.skinColour);
                    else if (materials[i].name.Contains(hairMat.name))
                        materials[i].SetColor("_BaseColor", chargen.instance.hairColour);
                    else if (materials[i].name.Contains(eyeMat.name))
                        materials[i].SetColor("_BaseColor", chargen.instance.eyeColour);

                    segmentRenderer.materials = materials;
                }
            }
        }

        currentHead = index;
    }

    public void loadSprite(int index)
    {
        sprite.sprite = sprites[index];
    }

    public void ChangeColour(int type, Color colour)//0 for skin, 1 for eyes, 2 for hair
    {
        if (type == 0)
        {
            foreach (GameObject segment in torsoList[currentTorso])
            {
                SkinnedMeshRenderer segmentRenderer = segment.GetComponent<SkinnedMeshRenderer>();

                if (segmentRenderer != null)
                {
                    Material[] materials = segmentRenderer.materials;

                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i].name.Contains(skinMat.name))
                            materials[i].SetColor("_BaseColor", colour);

                        segmentRenderer.materials = materials;
                    }
                }
            }

            foreach (GameObject segment in headList[currentHead])
            {
                segment.SetActive(true);

                SkinnedMeshRenderer segmentRenderer = segment.GetComponent<SkinnedMeshRenderer>();

                if (segmentRenderer != null)
                {
                    Material[] materials = segmentRenderer.materials;

                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i].name.Contains(skinMat.name))
                            materials[i].SetColor("_BaseColor", colour);

                        segmentRenderer.materials = materials;
                    }
                }
            }

            chargen.instance.skinColour = colour;
        }
        else if (type == 1)
        {
            foreach (GameObject segment in headList[currentHead])
            {
                SkinnedMeshRenderer segmentRenderer = segment.GetComponent<SkinnedMeshRenderer>();

                if (segmentRenderer != null)
                {
                    Material[] materials = segmentRenderer.materials;

                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i].name.Contains(eyeMat.name))
                            materials[i].SetColor("_BaseColor", colour);

                        segmentRenderer.materials = materials;
                    }
                }
            }

            chargen.instance.eyeColour = colour;
        }
        else if (type == 2)
        {
            foreach (GameObject segment in headList[currentHead])
            {
                SkinnedMeshRenderer segmentRenderer = segment.GetComponent<SkinnedMeshRenderer>();

                if (segmentRenderer != null)
                {
                    Material[] materials = segmentRenderer.materials;

                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i].name.Contains(hairMat.name))
                            materials[i].SetColor("_BaseColor", colour);

                        segmentRenderer.materials = materials;
                    }
                }
            }

            chargen.instance.hairColour = colour;
        }
    }

    //Variables
    public int torsoAmount()
    {
        return torsoList.Length;
    }
    public int headAmount()
    {
        return headList.Length;
    }
    public int spriteAmount()
    {
        return sprites.Length;
    }
}
