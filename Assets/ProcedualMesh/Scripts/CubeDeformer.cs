using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CubeDeformer : MonoBehaviour
{
    Mesh deformingMesh;
    Vector3[] originalVertices, displacedVertices;
    Vector3[] vertexVelocities;

    public float springForce = 20f;
    public float damping = 5f;

    float uniformScale = 1f;

    void Start()
    {
        deformingMesh = GetComponent<MeshFilter>().mesh;
        originalVertices = deformingMesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];
        vertexVelocities = new Vector3[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i];
        }
    }

    public void AddDeformingForce(Vector3 point, float force)
    {
        Debug.DrawLine(Camera.main.transform.position, point);
        //transform point to local space
        point = transform.InverseTransformPoint(point);

        for (int i = 0; i < displacedVertices.Length; i++)
        {
            AddForceToVertex(i, point, force);
        }
    }

    void AddForceToVertex(int vi, Vector3 point, float force)
    {
        Vector3 pointToVertex = displacedVertices[vi] - point;
        float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
        pointToVertex *= uniformScale;
        // a = F / m , v = a * dT => v = F * dT
        // velocityMag is velocity magnitude
        float velocityMag = attenuatedForce * Time.deltaTime;
        // get speed direction
        vertexVelocities[vi] += pointToVertex.normalized * velocityMag;
    }

    void UpdateVertex(int vi)
    {
        Vector3 velocity = vertexVelocities[vi];
        Vector3 displacement = displacedVertices[vi] - originalVertices[vi];
        displacement *= uniformScale;
        velocity -= displacement * springForce * Time.deltaTime;
        velocity *= 1f - damping * Time.deltaTime;
        vertexVelocities[vi] = velocity;

        displacedVertices[vi] += velocity * (Time.deltaTime / uniformScale);
    }

    void Update()
    {
        uniformScale = transform.localScale.x;

        for (int i = 0; i < displacedVertices.Length; i++)
        {
            UpdateVertex(i);
        }
        deformingMesh.vertices = displacedVertices;
        deformingMesh.RecalculateNormals();
    }
}
