using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Cube : MonoBehaviour
{
    public int xSize, ySize, zSize;
    public int roundness;

    Vector3[] vertices;
    Vector3[] normals;
    WaitForSeconds wait = new WaitForSeconds(0.05f);
    Mesh mesh;

    Color32[] cubeUV;

    private void Awake()
    {
        Generate();
    }

    #region Generate Grid
    /*void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices[i] = new Vector3(x, y);
                uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
                tangents[i] = tangent;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.tangents = tangents;

        int[] triangles = new int[xSize * ySize * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        {
            for (int x = 0; x < xSize; ti += 6, x++, vi++)
            {
                //first triangle
                triangles[ti] = vi;
                triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 2] = vi + 1;
                //second triangle
                triangles[ti + 3] = vi + 1;
                triangles[ti + 4] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }

        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }*/
    #endregion

    void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Cube";

        int conrnerVertices = 8;
        int edgeVertices = (xSize + ySize + zSize - 3) * 4;
        int faceVertices = 2 * ((xSize - 1) * (ySize - 1) + (xSize - 1) * (zSize - 1) + (ySize - 1) * (zSize - 1));

        vertices = new Vector3[conrnerVertices + edgeVertices + faceVertices];
        normals = new Vector3[vertices.Length];
        cubeUV = new Color32[vertices.Length];

        int vi = 0;
        for (int y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                SetVertex(vi++, x, y, 0);
            }
            for (int z = 1; z <= zSize; z++)
            {
                SetVertex(vi++, xSize, y, z);
            }
            for (int x = xSize - 1; x >= 0; x--)
            {
                SetVertex(vi++, x, y, zSize);
            }
            for (int z = zSize - 1; z > 0; z--)
            {
                SetVertex(vi++, 0, y, z);
            }
        }

        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                SetVertex(vi++, x, ySize, z);
            }
        }
        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                SetVertex(vi++, x, 0, z);
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.colors32 = cubeUV;

        int quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2;
        int[] triangles = new int[quads * 6];
        int[] trisX = new int[(ySize * zSize) * 6 * 2];
        int[] trisY = new int[(xSize * zSize) * 6 * 2];
        int[] trisZ = new int[(ySize * xSize) * 6 * 2];

        //ti : triangleIndex, vi : vertexIndex, qi : quadIndex, ring : a ring total index
        int ring = (xSize + zSize) * 2;
        vi = 0;
        int tx = 0, ty = 0, tz = 0;

        for (int y = 0; y < ySize; y++, vi++)
        {
            /*for (int qi = 0; qi < ring - 1; qi++, vi++)
            {
                ti = SetQuad(triangles, ti, vi, vi + 1, vi + ring, vi + ring + 1);
            }
            ti = SetQuad(triangles, ti, vi, vi + 1 - ring, vi + ring, vi + 1);*/

            for (int qi = 0; qi < xSize; qi++, vi++)
            {
                tz = SetQuad(trisZ, tz, vi, vi + 1, vi + ring, vi + ring + 1);
            }
            for (int qi = 0; qi < zSize; qi++, vi++)
            {
                tx = SetQuad(trisX, tx, vi, vi + 1, vi + ring, vi + ring + 1);
            }
            for (int qi = 0; qi < xSize; qi++, vi++)
            {
                tz = SetQuad(trisZ, tz, vi, vi + 1, vi + ring, vi + ring + 1);
            }
            for (int qi = 0; qi < zSize - 1; qi++, vi++)
            {
                tx = SetQuad(trisX, tx, vi, vi + 1, vi + ring, vi + ring + 1);
            }
            tx = SetQuad(trisX, tx, vi, vi - ring + 1, vi + ring, vi + 1);
        }

        ty = CreateTopFace(trisY, ty, ring);
        ty = CreateBottomFace(trisY, ty, ring);

        //mesh.triangles = triangles;
        mesh.subMeshCount = 3;
        mesh.SetTriangles(trisZ, 0);
        mesh.SetTriangles(trisX, 1);
        mesh.SetTriangles(trisY, 2);

        CreateColliders();
    }

    void CreateColliders()
    {
        AddBoxCollider(xSize, ySize - roundness * 2, zSize - roundness * 2);
        AddBoxCollider(xSize - roundness * 2, ySize, zSize - roundness * 2);
        AddBoxCollider(xSize - roundness * 2, ySize - roundness * 2, zSize);

        Vector3 min = Vector3.one * roundness;
        Vector3 half = new Vector3(xSize, ySize, zSize) * 0.5f;
        Vector3 max = new Vector3(xSize, ySize, zSize) - min;

        AddCapsuleCollider(0, half.x, min.y, min.z);
        AddCapsuleCollider(0, half.x, min.y, max.z);
        AddCapsuleCollider(0, half.x, max.y, min.z);
        AddCapsuleCollider(0, half.x, max.y, max.z);

        AddCapsuleCollider(1, min.x, half.y, min.z);
        AddCapsuleCollider(1, min.x, half.y, max.z);
        AddCapsuleCollider(1, max.x, half.y, min.z);
        AddCapsuleCollider(1, max.x, half.y, max.z);

        AddCapsuleCollider(2, min.x, min.y, half.z);
        AddCapsuleCollider(2, min.x, max.y, half.z);
        AddCapsuleCollider(2, max.x, min.y, half.z);
        AddCapsuleCollider(2, max.x, max.y, half.z);
    }

    void AddBoxCollider(float x, float y, float z)
    {
        BoxCollider c = gameObject.AddComponent<BoxCollider>();
        c.size = new Vector3(x, y, z);
    }

    void AddCapsuleCollider(int direction, float x, float y, float z)
    {
        CapsuleCollider c = gameObject.AddComponent<CapsuleCollider>();
        c.center = new Vector3(x, y, z);
        c.direction = direction;
        c.radius = roundness;
        c.height = c.center[direction] * 2f;
    }

    void SetVertex(int vi, int x, int y, int z)
    {
        Vector3 inner = vertices[vi] = new Vector3(x, y, z);

        if (x < roundness)
        {
            inner.x = roundness;
        }
        else if (x > xSize - roundness)
        {
            inner.x = xSize - roundness;
        }

        if (y < roundness)
        {
            inner.y = roundness;
        }
        else if (y > ySize - roundness)
        {
            inner.y = ySize - roundness;
        }

        if (z < roundness)
        {
            inner.z = roundness;
        }
        else if (z > zSize - roundness)
        {
            inner.z = zSize - roundness;
        }

        normals[vi] = (vertices[vi] - inner).normalized;
        vertices[vi] = inner + normals[vi] * roundness;
        cubeUV[vi] = new Color32((byte)x, (byte)y, (byte)z, 0);
    }

    int CreateTopFace(int[] triangles, int t, int ring)
    {
        // start vertex index
        int vi = ring * ySize;
        for (int x = 0; x < xSize - 1; x++, vi++)
        {
            t = SetQuad(triangles, t, vi, vi + 1, vi + ring - 1, vi + ring);
        }
        t = SetQuad(triangles, t, vi, vi + 1, vi + ring - 1, vi + 2);

        int vMin = ring * (ySize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = vi + 2;

        //turned into a loop to take care of all but the last row
        for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
        {
            //first quad in middle row
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
            //middle quad in middle row
            for (int x = 1; x < xSize - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
            }
            //last quad in middle row
            t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
        }

        int vTop = vMin - 2;
        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
        }
        t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

        return t;
    }
     
    int CreateBottomFace(int[] triangles, int t, int ring)
    {
        // start vertex index
        int vi = 1;
        int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
        t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
        for (int x = 1; x < xSize - 1; x++, vi++, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vi, vi + 1);
        }
        t = SetQuad(triangles, t, vMid, vi + 2, vi, vi + 1);

        int vMin = ring - 2;
        vMid -= xSize - 2;
        int vMax = vi + 2;

        for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
            for (int x = 1; x < xSize - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
            }
            t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
        }

        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
        }
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

        return t;
    }

    //set single quad 
    static int SetQuad(int[] tris, int ti, int v00, int v10, int v01, int v11)
    {
        tris[ti] = v00;
        tris[ti + 1] = tris[ti + 4] = v01;
        tris[ti + 2] = tris[ti + 3] = v10;
        tris[ti + 5] = v11;
        return ti + 6;
    }

/*    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(vertices[i], 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(vertices[i], normals[i]);
        }
    }*/
}
