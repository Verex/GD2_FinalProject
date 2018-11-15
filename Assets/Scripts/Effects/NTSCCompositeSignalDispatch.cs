using System;

using UnityEngine;


[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class NTSCCompositeSignalDispatch : MonoBehaviour 
{
    public Material material;
    
    void Start() 
    {
        if(!SystemInfo.supportsImageEffects || 
            material == null || 
            material.shader == null || 
            !material.shader.isSupported)
        {
            enabled = false;
            return; //Don't try to dispatch this effect
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, material);
    }
}