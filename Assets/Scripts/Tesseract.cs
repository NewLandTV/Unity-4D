using System.Collections.Generic;
using UnityEngine;

public enum Axis4D
{
    XY,
    XZ,
    XW,
    YZ,
    YW,
    ZW
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Tesseract : MonoBehaviour
{
    // Renderer
    private MeshFilter meshFilter;
    private Mesh mesh;

    // Vertices, Triangles, Uvs
    private List<Vector4> originalVertices;
    private List<Vector4> rotatedVertices;
    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Vector2> uvs;
    private List<Axis4D> rotationOrder;
    private Dictionary<Axis4D, float> rotation;

    // Camera
    private Camera mainCamera;

    private Vector3 cameraPosition;

    // Tesseract Rotation
    private bool freezeRotation;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();

        mainCamera = Camera.main;
        cameraPosition = mainCamera.transform.position;
    }

    private void Start()
    {
        originalVertices = new List<Vector4>()
        {
            new Vector4(1f, 1f, 1f, 1f),
            new Vector4(1f, 1f, 1f, -1f),
            new Vector4(1f, 1f, -1f, 1f),
            new Vector4(1f, 1f, -1f, -1f),
            new Vector4(1f, -1f, 1f, 1f),
            new Vector4(1f, -1f, 1f, -1f),
            new Vector4(1f, -1f, -1f, 1f),
            new Vector4(1f, -1f, -1f,-1f),
            new Vector4(-1f, 1f, 1f, 1f),
            new Vector4(-1f, 1f, 1f, -1f),
            new Vector4(-1f, 1f, -1f, 1f),
            new Vector4(-1f, 1f, -1f, -1f),
            new Vector4(-1f,-1f, 1f, 1f),
            new Vector4(-1f, -1f, 1f, -1f),
            new Vector4(-1f, -1f, -1f, 1f),
            new Vector4(-1f, -1f, -1f, -1f)
        };

        rotationOrder = new List<Axis4D>
        {
            Axis4D.YZ,
            Axis4D.XW,
            Axis4D.YW,
            Axis4D.ZW,
            Axis4D.XY,
            Axis4D.XZ
        };

        rotation = new Dictionary<Axis4D, float>
        {
            { Axis4D.XY, 0f },
            { Axis4D.XZ, 0f },
            { Axis4D.XW, 0f },
            { Axis4D.YZ, 0f },
            { Axis4D.YW, 0f },
            { Axis4D.ZW, 0f }
        };

        mesh = meshFilter.sharedMesh;

        if (mesh == null)
        {
            meshFilter.mesh = new Mesh();

            mesh = meshFilter.sharedMesh;
        }

        ResetVertices();
    }

    private void Update()
    {
        cameraPosition.z += Input.GetAxis("Mouse ScrollWheel") * 2f;
        freezeRotation = Input.GetKeyDown(KeyCode.R) ? !freezeRotation : freezeRotation;
        mainCamera.transform.position = cameraPosition;
        mainCamera.orthographic = Input.GetKeyDown(KeyCode.Space) ? !mainCamera.orthographic : mainCamera.orthographic;
        mainCamera.orthographicSize = -(cameraPosition.z + 4f);

        DrawTesseract();
    }

    private void OnGUI()
    {
        GUI.contentColor = Color.green;

        GUI.Label(new Rect(25f, 25f, 100f, 30f), "XY");

        rotation[Axis4D.XY] = GUI.HorizontalSlider(new Rect(25f, 50f, 100f, 30f), Mathf.Repeat(rotation[Axis4D.XY], 360f), 0f, 360f);

        GUI.Label(new Rect(25f, 75f, 100f, 30f), "XZ");

        rotation[Axis4D.XZ] = GUI.HorizontalSlider(new Rect(25f, 100f, 100f, 30f), Mathf.Repeat(rotation[Axis4D.XZ], 360f), 0f, 360f);

        GUI.Label(new Rect(25f, 125f, 100f, 30f), "XW");

        rotation[Axis4D.XW] = GUI.HorizontalSlider(new Rect(25f, 150f, 100f, 30f), Mathf.Repeat(rotation[Axis4D.XW], 360f), 0f, 360f);

        GUI.Label(new Rect(25f, 175f, 100f, 30f), "YZ");

        rotation[Axis4D.YZ] = GUI.HorizontalSlider(new Rect(25f, 200f, 100f, 30f), Mathf.Repeat(rotation[Axis4D.YZ], 360f), 0f, 360f);

        GUI.Label(new Rect(25f, 225f, 100f, 30f), "YW");

        rotation[Axis4D.YW] = GUI.HorizontalSlider(new Rect(25f, 250f, 100f, 30f), Mathf.Repeat(rotation[Axis4D.YW], 360f), 0f, 360f);

        GUI.Label(new Rect(25f, 275f, 100f, 30f), "ZW");

        rotation[Axis4D.ZW] = GUI.HorizontalSlider(new Rect(25f, 300f, 100f, 30f), Mathf.Repeat(rotation[Axis4D.ZW], 360f), 0f, 360f);

        if (!freezeRotation)
        {
            Rotate(Axis4D.XY, 0.1f);
            Rotate(Axis4D.XZ, 0.15f);
            Rotate(Axis4D.XW, 0.6f);
            Rotate(Axis4D.YW, 0.3f);
            Rotate(Axis4D.YZ, 0.45f);
            Rotate(Axis4D.ZW, 0.5f);
        }

        ApplyRotationToVertices();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.64f, 0f);

        for (int i = 0; i < rotatedVertices.Count; i++)
        {
            Gizmos.DrawSphere(rotatedVertices[i], 0.1f);
        }
    }

    private void ResetVertices()
    {
        rotatedVertices = new List<Vector4>();

        rotatedVertices.AddRange(originalVertices);
    }

    private void DrawTesseract()
    {
        mesh.Clear();

        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

        CreatePlane(rotatedVertices[0], rotatedVertices[1], rotatedVertices[5], rotatedVertices[4]);
        CreatePlane(rotatedVertices[0], rotatedVertices[2], rotatedVertices[6], rotatedVertices[4]);
        CreatePlane(rotatedVertices[0], rotatedVertices[8], rotatedVertices[12], rotatedVertices[4]);
        CreatePlane(rotatedVertices[0], rotatedVertices[2], rotatedVertices[3], rotatedVertices[1]);
        CreatePlane(rotatedVertices[0], rotatedVertices[1], rotatedVertices[9], rotatedVertices[8]);
        CreatePlane(rotatedVertices[0], rotatedVertices[2], rotatedVertices[10], rotatedVertices[8]);
        CreatePlane(rotatedVertices[1], rotatedVertices[3], rotatedVertices[7], rotatedVertices[5]);
        CreatePlane(rotatedVertices[1], rotatedVertices[9], rotatedVertices[13], rotatedVertices[5]);
        CreatePlane(rotatedVertices[1], rotatedVertices[3], rotatedVertices[9], rotatedVertices[11]);
        CreatePlane(rotatedVertices[2], rotatedVertices[3], rotatedVertices[7], rotatedVertices[6]);
        CreatePlane(rotatedVertices[2], rotatedVertices[3], rotatedVertices[10], rotatedVertices[11]);
        CreatePlane(rotatedVertices[2], rotatedVertices[10], rotatedVertices[14], rotatedVertices[6]);
        CreatePlane(rotatedVertices[3], rotatedVertices[11], rotatedVertices[15], rotatedVertices[7]);
        CreatePlane(rotatedVertices[4], rotatedVertices[12], rotatedVertices[13], rotatedVertices[5]);
        CreatePlane(rotatedVertices[4], rotatedVertices[6], rotatedVertices[14], rotatedVertices[12]);
        CreatePlane(rotatedVertices[4], rotatedVertices[6], rotatedVertices[7], rotatedVertices[5]);
        CreatePlane(rotatedVertices[5], rotatedVertices[7], rotatedVertices[15], rotatedVertices[13]);
        CreatePlane(rotatedVertices[6], rotatedVertices[7], rotatedVertices[14], rotatedVertices[15]);
        CreatePlane(rotatedVertices[8], rotatedVertices[10], rotatedVertices[14], rotatedVertices[12]);
        CreatePlane(rotatedVertices[8], rotatedVertices[9], rotatedVertices[13], rotatedVertices[12]);
        CreatePlane(rotatedVertices[8], rotatedVertices[9], rotatedVertices[10], rotatedVertices[11]);
        CreatePlane(rotatedVertices[9], rotatedVertices[11], rotatedVertices[15], rotatedVertices[13]);
        CreatePlane(rotatedVertices[10], rotatedVertices[11], rotatedVertices[15], rotatedVertices[14]);
    }

    private void CreatePlane(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        Vector2 uv0 = new Vector2(0f, 0f);
        Vector2 uv1 = new Vector2(1f, 0f);
        Vector2 uv2 = new Vector2(0.5f, 1f);

        List<Vector3> newVertices = new List<Vector3>()
        {
            p1, p3, p2,
            p1, p2, p4,
            p2, p3, p4,
            p1, p4, p3
        };

        vertices.AddRange(newVertices);

        mesh.vertices = vertices.ToArray();

        int t = triangles.Count;

        for (int i = 0; i < 12; i++)
        {
            triangles.Add(i + t);
        }

        mesh.SetTriangles(triangles.ToArray(), 0);
        uvs.AddRange(new List<Vector2>()
        {
            uv0, uv1, uv2,
            uv0, uv1, uv2,
            uv0, uv1, uv2,
            uv0, uv1, uv2
        });

        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
    }

    private Vector4 GetRotatedVertex(Axis4D axis, Vector4 vector, float sin, float cos)
    {
        switch (axis)
        {
            case Axis4D.XY:
                return RotateAroundXY(vector, sin, cos);
            case Axis4D.XZ:
                return RotateAroundXZ(vector, sin, cos);
            case Axis4D.XW:
                return RotateAroundXW(vector, sin, cos);
            case Axis4D.YZ:
                return RotateAroundYZ(vector, sin, cos);
            case Axis4D.YW:
                return RotateAroundYW(vector, sin, cos);
            case Axis4D.ZW:
                return RotateAroundZW(vector, sin, cos);
        }

        return Vector4.zero;
    }

    private Vector4 RotateAroundXY(Vector4 vector, float sin, float cos)
    {
        float tempX = cos * vector.x + sin * vector.y;
        float tempY = -sin * vector.x + cos * vector.y;

        return new Vector4(tempX, tempY, vector.z, vector.w);
    }

    private Vector4 RotateAroundXZ(Vector4 vector, float sin, float cos)
    {
        float tempX = cos * vector.x + sin * vector.z;
        float tempZ = -sin * vector.x + cos * vector.z;

        return new Vector4(tempX, vector.y, tempZ, vector.w);
    }

    private Vector4 RotateAroundXW(Vector4 vector, float sin, float cos)
    {
        float tempX = cos * vector.x + sin * vector.w;
        float tempW = -sin * vector.x + cos * vector.w;

        return new Vector4(tempX, vector.y, vector.z, tempW);
    }

    private Vector4 RotateAroundYZ(Vector4 vector, float sin, float cos)
    {
        float tempY = cos * vector.y + sin * vector.z;
        float tempZ = -sin * vector.y + cos * vector.z;

        return new Vector4(vector.x, tempY, tempZ, vector.w);
    }

    private Vector4 RotateAroundYW(Vector4 vector, float sin, float cos)
    {
        float tempY = cos * vector.y - sin * vector.w;
        float tempW = sin * vector.y + cos * vector.w;

        return new Vector4(vector.x, tempY, vector.z, tempW);
    }

    private Vector4 RotateAroundZW(Vector4 vector, float sin, float cos)
    {
        float tempZ = cos * vector.z - sin * vector.w;
        float tempW = sin * vector.z + cos * vector.w;

        return new Vector4(vector.x, vector.y, tempZ, tempW);
    }

    private void Rotate(Axis4D axis, float theta)
    {
        AddToRotationDictionary(axis, theta);
        ApplyRotationToVertices();
    }

    private void AddToRotationDictionary(Axis4D axis, float theta)
    {
        rotation[axis] += theta;
    }

    private void ApplyRotationToVertices()
    {
        ResetVertices();

        foreach (Axis4D axis in rotationOrder)
        {
            float sin = Mathf.Sin(rotation[axis] * Mathf.Deg2Rad);
            float cos = Mathf.Cos(rotation[axis] * Mathf.Deg2Rad);

            for (int i = 0; i < rotatedVertices.Count; i++)
            {
                rotatedVertices[i] = GetRotatedVertex(axis, rotatedVertices[i], sin, cos);
            }
        }
    }
}
