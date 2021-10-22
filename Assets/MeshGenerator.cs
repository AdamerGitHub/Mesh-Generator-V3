using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Color[] colours;

    public NoiseSample[] noizeSamples;

    public float noizeHeight;

    public int xSize = 20;
    public int zSize = 20;

    public Gradient gradient;

    float minTerrainHeight;
    float maxTerrainHeight;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        StartCoroutine(IfUpdatedSettings());
    }

    private void Update()
    {
        UpdateMesh();
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        float y = 0;

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for(int x = 0; x <= xSize; x++)
            {
                
                if (!float.IsNaN(AddAllSampleNoizes(x, z)))
                {
                    y = AddAllSampleNoizes(x, z) * noizeHeight;
                }
                vertices[i] = new Vector3(x, y, z);

                if(y > maxTerrainHeight)
                    maxTerrainHeight = y;

                if(y < minTerrainHeight)
                    minTerrainHeight = y;

                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;

                UpdateMesh();
            }
            vert++;
        }

        colours = new Color[vertices.Length];
        uvs = new Vector2[vertices.Length];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[i].y);
                colours[i] = gradient.Evaluate(height);
                uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);
                i++;
            }
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colours;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
    }

    // Admin Update Settings
    IEnumerator IfUpdatedSettings()
    {
        int oldxSize = xSize;
        int oldzSize = zSize;
        Gradient oldGradient = new Gradient();
        oldGradient.SetKeys(gradient.colorKeys, gradient.alphaKeys);
        float[] oldPercent = new float[noizeSamples.Length];
        //float[] oldHeight = new float[noizeSamples.Length];
        float[] oldScale = new float[noizeSamples.Length];
        float[] oldxStrength = new float[noizeSamples.Length];
        float[] oldzStrength = new float[noizeSamples.Length];

        for (int i = 0; i < noizeSamples.Length; i++)
        {
            oldPercent[i] = noizeSamples[i].percent;
            //oldHeight[i] = noizeSamples[i].height;
            oldScale[i] = noizeSamples[i].scale;
            oldxStrength[i] = noizeSamples[i].xStrength;
            oldzStrength[i] = noizeSamples[i].zStrength;
        }

        while(vertices != null)
        {

            if (xSize != oldxSize)
            {
                CreateShape();
                oldxSize = xSize;
            }
            if (zSize != oldzSize)
            {
                CreateShape();
                oldzSize = zSize;
            }
            //if(gradient.colorKeys != oldGradient.colorKeys)
            //{

            bool isGradChanged = false;

            if (gradient.colorKeys.Length != oldGradient.colorKeys.Length)
            {
                isGradChanged = true;
            }

            for (int i = 0; i < gradient.alphaKeys.Length; i++)
            {
                if (gradient.alphaKeys[i].time != oldGradient.alphaKeys[i].time)
                {
                    isGradChanged = true;
                    break;
                }

                if (gradient.alphaKeys[i].alpha != oldGradient.alphaKeys[i].alpha)
                {
                    isGradChanged = true;
                    break;
                }
            }

            for (int i = 0; i < gradient.colorKeys.Length; i++)
            {
                if(gradient.colorKeys[i].time != oldGradient.colorKeys[i].time)
                {
                    isGradChanged = true;
                    break;
                }

                if (gradient.colorKeys[i].color != oldGradient.colorKeys[i].color)
                {
                    isGradChanged = true;
                    break;
                }
            }

            if (isGradChanged)
            {
                CreateShape();
                oldGradient = new Gradient();
                oldGradient.SetKeys(gradient.colorKeys, gradient.alphaKeys);
            }
            
            //}

            for (int i = 0; i < noizeSamples.Length; i++)
            {
                if (oldPercent[i] != noizeSamples[i].percent)
                {
                    CreateShape();
                    oldPercent[i] = noizeSamples[i].percent;
                }
                /*if (oldHeight[i] != noizeSamples[i].height)
                {
                    CreateShape();
                    oldHeight[i] = noizeSamples[i].height;
                }*/
                if (oldScale[i] != noizeSamples[i].scale)
                {
                    CreateShape();
                    oldScale[i] = noizeSamples[i].scale;
                }
                if (oldxStrength[i] != noizeSamples[i].xStrength)
                {
                    CreateShape();
                    oldxStrength[i] = noizeSamples[i].xStrength;
                }
                if (oldzStrength[i] != noizeSamples[i].zStrength)
                {
                    CreateShape();
                    oldzStrength[i] = noizeSamples[i].zStrength;
                }
            }
            yield return null;
        }
    }

    float AddAllSampleNoizes(float x, float z)
    {
        float sumNoizeSamples = noizeSamples[0].CalcPerlinNoize(x, z);

        for (int i = 1; i < noizeSamples.Length; i++)
        {
            sumNoizeSamples += noizeSamples[i].CalcPerlinNoize(x, z);
        }
        return sumNoizeSamples;
    }

    /*
    // Draw a sphere in a vertices
    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(vertices[i], 0.1f);
            
        }
    }
    */
    [System.Serializable]
    public class NoiseSample
    {
        [Range(0, 1)]
        public float percent = 1f;
        //public float height = 2f;
        [Range(0, 1)]
        public float scale = 1f;

        public float xStrength = 0.3f;
        public float zStrength = 0.3f;

        public float CalcPerlinNoize(float x, float z)
        {
            float perlinNoise = Mathf.PerlinNoise(x * xStrength / scale, z * zStrength / scale) /** height*/;
            perlinNoise *= percent;

            return perlinNoise;
        }
    }

}
