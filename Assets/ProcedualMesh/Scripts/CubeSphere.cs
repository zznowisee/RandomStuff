using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeSphere : MonoBehaviour
{
    public int gridSize;
    public float radius;
    public int resolution = 10;

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
        mesh.name = "Procedural Sphere";

        int conrnerVertices = 8;
        int edgeVertices = (gridSize + gridSize + gridSize - 3) * 4;
        int faceVertices = 2 * ((gridSize - 1) * (gridSize - 1) + (gridSize - 1) * (gridSize - 1) + (gridSize - 1) * (gridSize - 1));

        vertices = new Vector3[conrnerVertices + edgeVertices + faceVertices];
        normals = new Vector3[vertices.Length];
        cubeUV = new Color32[vertices.Length];

        int vi = 0;
        for (int y = 0; y <= gridSize; y++)
        {
            for (int x = 0; x <= gridSize; x++)
            {
                SetVertex(vi++, x, y, 0);
            }
            for (int z = 1; z <= gridSize; z++)
            {
                SetVertex(vi++, gridSize, y, z);
            }
            for (int x = gridSize - 1; x >= 0; x--)
            {
                SetVertex(vi++, x, y, gridSize);
            }
            for (int z = gridSize - 1; z > 0; z--)
            {
                SetVertex(vi++, 0, y, z);
            }
        }

        for (int z = 1; z < gridSize; z++)
        {
            for (int x = 1; x < gridSize; x++)
            {
                SetVertex(vi++, x, gridSize, z);
            }
        }
        for (int z = 1; z < gridSize; z++)
        {
            for (int x = 1; x < gridSize; x++)
            {
                SetVertex(vi++, x, 0, z);
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.colors32 = cubeUV;

        int quads = (gridSize * gridSize + gridSize * gridSize + gridSize * gridSize) * 2;
        int[] triangles = new int[quads * 6];
        int[] trisX = new int[(gridSize * gridSize) * 6 * 2];
        int[] trisY = new int[(gridSize * gridSize) * 6 * 2];
        int[] trisZ = new int[(gridSize * gridSize) * 6 * 2];

        //ti : triangleIndex, vi : vertexIndex, qi : quadIndex, ring : a ring total index
        int ring = (gridSize + gridSize) * 2;
        vi = 0;
        int tx = 0, ty = 0, tz = 0;

        for (int y = 0; y < gridSize; y++, vi++)
        {
            /*for (int qi = 0; qi < ring - 1; qi++, vi++)
            {
                ti = SetQuad(triangles, ti, vi, vi + 1, vi + ring, vi + ring + 1);
            }
            ti = SetQuad(triangles, ti, vi, vi + 1 - ring, vi + ring, vi + 1);*/

            for (int qi = 0; qi < gridSize; qi++, vi++)
            {
                tz = SetQuad(trisZ, tz, vi, vi + 1, vi + ring, vi + ring + 1);
            }
            for (int qi = 0; qi < gridSize; qi++, vi++)
            {
                tx = SetQuad(trisX, tx, vi, vi + 1, vi + ring, vi + ring + 1);
            }
            for (int qi = 0; qi < gridSize; qi++, vi++)
            {
                tz = SetQuad(trisZ, tz, vi, vi + 1, vi + ring, vi + ring + 1);
            }
            for (int qi = 0; qi < gridSize - 1; qi++, vi++)
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
        gameObject.AddComponent<SphereCollider>();
    }

    void SetVertex(int vi, int x, int y, int z)
    {
        Vector3 v = new Vector3(x, y, z) * 2f / gridSize - Vector3.one;
        float x2 = v.x * v.x;
        float y2 = v.y * v.y;
        float z2 = v.z * v.z;
        Vector3 s;
        s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
        s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
        s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
        normals[vi] = s;
        vertices[vi] = normals[vi] * radius;
        cubeUV[vi] = new Color32((byte)x, (byte)y, (byte)z, 0);
    }

    int CreateTopFace(int[] triangles, int t, int ring)
    {
        // start vertex index
        int vi = ring * gridSize;
        for (int x = 0; x < gridSize - 1; x++, vi++)
        {
            t = SetQuad(triangles, t, vi, vi + 1, vi + ring - 1, vi + ring);
        }
        t = SetQuad(triangles, t, vi, vi + 1, vi + ring - 1, vi + 2);

        int vMin = ring * (gridSize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = vi + 2;

        //turned into a loop to take care of all but the last row
        for (int z = 1; z < gridSize - 1; z++, vMin--, vMid++, vMax++)
        {
            //first quad in middle row
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + gridSize - 1);
            //middle quad in middle row
            for (int x = 1; x < gridSize - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vMid + gridSize - 1, vMid + gridSize);
            }
            //last quad in middle row
            t = SetQuad(triangles, t, vMid, vMax, vMid + gridSize - 1, vMax + 1);
        }

        int vTop = vMin - 2;
        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
        for (int x = 1; x < gridSize - 1; x++, vTop--, vMid++)
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
        int vMid = vertices.Length - (gridSize - 1) * (gridSize - 1);
        t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
        for (int x = 1; x < gridSize - 1; x++, vi++, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vi, vi + 1);
        }
        t = SetQuad(triangles, t, vMid, vi + 2, vi, vi + 1);

        int vMin = ring - 2;
        vMid -= gridSize - 2;
        int vMax = vi + 2;

        for (int z = 1; z < gridSize - 1; z++, vMin--, vMid++, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid + gridSize - 1, vMin + 1, vMid);
            for (int x = 1; x < gridSize - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid + gridSize - 1, vMid + gridSize, vMid, vMid + 1);
            }
            t = SetQuad(triangles, t, vMid + gridSize - 1, vMax + 1, vMid, vMax);
        }

        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
        for (int x = 1; x < gridSize - 1; x++, vTop--, vMid++)
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

    private void OnDrawGizmosSelected()
    {
        float step = 2f / resolution;
        for (int i = 0; i <= resolution; i++)
        {
            // i * step - 1 将值域变成-1 ~ 1
            ShowPoint(i * step - 1, -1f);
            ShowPoint(i * step - 1, 1f);
        }
        for (int i = 1; i < resolution; i++)
        {
            ShowPoint(-1f, i * step - 1f);
            ShowPoint(1f, i * step - 1f);
        }
    }

    void ShowPoint(float x, float y)
    {
        //正方形上的点的坐标
        Vector2 square = new Vector2(x, y);
        //圆上的点的坐标
        Vector2 circle;
        circle.x = square.x * Mathf.Sqrt(1f - square.y * square.y * 0.5f);
        circle.y = square.y * Mathf.Sqrt(1f - square.x * square.x * 0.5f);

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(square, 0.025f);

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(circle, 0.025f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(square, circle);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(circle, Vector2.zero);
    }
}
