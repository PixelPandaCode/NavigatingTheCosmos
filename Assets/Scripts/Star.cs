using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class Star : MonoBehaviour
{
    public Material StarMat = null;
    public Material OutlineMat = null;
    public Color StarColor = Color.white;
    public float Intensity = 1.0f;
    // Start is called before the first frame update
    void Awake()
    {
        Renderer rend = GetComponent<Renderer>();
        Material[] instancedMaterials = new Material[rend.materials.Length];
        for (int i = 0; i < rend.materials.Length; i++)
        {
            instancedMaterials[i] = new Material(rend.materials[i]);
        }
        rend.materials = instancedMaterials;
        StarMat = instancedMaterials[0];
        OutlineMat = instancedMaterials[1];
    }

    public void Init()
    {
        if (StarMat != null)
        {
            StarMat.SetColor("_BaseColor", StarColor);
            StarMat.EnableKeyword("_EMISSION");
            StarMat.SetColor("_EmissionColor", StarColor * Intensity);
        }
    }

    public void ActivateOutline()
    {
        if (OutlineMat != null)
        {
            OutlineMat.SetFloat("_Width", 3.0f);
        }
    }

    public void DeactivateOutline()
    {
        if (OutlineMat != null)
        {
            OutlineMat.SetFloat("_Width", 0.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
