/*==============================================================================
Copyright (c) 2018 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
==============================================================================*/

using UnityEngine;
using Vuforia;

/// <summary>
/// A component that renders a bounding box using lines.
/// </summary>
public class BoundingBoxRenderer : MonoBehaviour
{
    #region PRIVATE_MEMBERS

    private Material mLineMaterial = null;

    #endregion // PRIVATE_MEMBERS
    


    private void OnRenderObject()
    {
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);

        if (mLineMaterial == null)
        {
            // We "borrow" the default material from a primitive.
            // This ensures that, even on mobile platforms,
            // we always get a valid material at runtime,
            // as on mobile Unity can strip away unused shaders at build-time. 
            var tempObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var cubeRenderer = tempObj.GetComponent<MeshRenderer>();
            
            mLineMaterial = new Material(cubeRenderer.material);
            mLineMaterial.color = Color.white;
            
            Destroy(tempObj);
        }

        mLineMaterial.SetPass(0);
        mLineMaterial.color = Color.white;
        
        GL.Begin(GL.LINES);

        // Bottom XZ quad
        GL.Vertex3(-0.5f, -0.5f, -0.5f);
        GL.Vertex3( 0.5f, -0.5f, -0.5f);

        GL.Vertex3(0.5f, -0.5f, -0.5f);
        GL.Vertex3(0.5f, -0.5f,  0.5f);

        GL.Vertex3( 0.5f, -0.5f, 0.5f);
        GL.Vertex3(-0.5f, -0.5f, 0.5f);

        GL.Vertex3(-0.5f, -0.5f, 0.5f);
        GL.Vertex3(-0.5f, -0.5f, -0.5f);

        // Top XZ quad
        GL.Vertex3(-0.5f, 0.5f, -0.5f);
        GL.Vertex3(0.5f,  0.5f, -0.5f);

        GL.Vertex3(0.5f,  0.5f, -0.5f);
        GL.Vertex3(0.5f,  0.5f, 0.5f);

        GL.Vertex3(0.5f,  0.5f, 0.5f);
        GL.Vertex3(-0.5f, 0.5f, 0.5f);

        GL.Vertex3(-0.5f, 0.5f, 0.5f);
        GL.Vertex3(-0.5f, 0.5f, -0.5f);

        // Side lines
        GL.Vertex3(-0.5f, -0.5f, -0.5f);
        GL.Vertex3(-0.5f,  0.5f, -0.5f);

        GL.Vertex3(0.5f, -0.5f, -0.5f);
        GL.Vertex3(0.5f,  0.5f, -0.5f);

        GL.Vertex3(0.5f, -0.5f, 0.5f);
        GL.Vertex3(0.5f,  0.5f, 0.5f);

        GL.Vertex3(-0.5f, -0.5f, 0.5f);
        GL.Vertex3(-0.5f,  0.5f, 0.5f);

        GL.End();

        GL.PopMatrix();
    }
}
