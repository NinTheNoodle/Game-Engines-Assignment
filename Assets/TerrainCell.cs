using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TerrainCell : MonoBehaviour {
	public HeightMap heightmap;
	public Texture2D texture;
    public int width, depth;
    public float height;
    public GameObject particles;

	void Start () {
		gameObject.name = "Terrain Cell";
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer> ();
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter> ();
        MeshCollider colliderFilter = gameObject.AddComponent<MeshCollider>();

        UpdateMesh();
    }

    public void UpdateMesh()
    {
        try
        {
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            MeshCollider colliderFilter = gameObject.GetComponent<MeshCollider>();

            texture = GenerateTexture(heightmap, width + 1, depth + 1);

            meshRenderer.material.mainTexture = texture;
            meshFilter.mesh = GenerateMesh(heightmap, texture);
            colliderFilter.sharedMesh = meshFilter.mesh;
            //meshRenderer.material.SetColor(new Color(0, 27 / 256f, 13 / 256f));
        }
        catch (MissingComponentException) { }
    }

    Texture2D GenerateTexture(HeightMap heightmap, int width, int depth)
    {
        Texture2D texture = new Texture2D(width, depth);
        Color[] colors = new Color[texture.width * texture.height];

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                HeightMapDatum datum = heightmap.GetData(x, z);
                float height = datum.height;
                float moisture = datum.moisture;
                float specles = datum.specles;

                Color low = new Color(73 / 256f, 38 / 256f, 21 / 256f);
                Color high = new Color(67 / 256f, 103 / 256f, 13 / 256f);
                Color specled = new Color(0, 0, 0);

                Color highMoist = new Color(127 / 256f, 183 / 256f, 142 / 256f);
                Color lowMoist = new Color(0, 27 / 256f, 28 / 256f);
                
                Color dry = Color.Lerp(low, high, height);
                Color moist = Color.Lerp(lowMoist, highMoist, height);
                specled = Color.Lerp(dry, specled, specles);

                colors[x + z * width] = Color.Lerp(specled, moist, moisture);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        //texture.filterMode = FilterMode.Point;
        return texture;
    }

    Mesh GenerateMesh(HeightMap heightmap, Texture2D texture) {
		Mesh mesh = new Mesh ();
        int w = width + 1;
        int d = depth + 1;

        Vector2[] uvs = new Vector2[w * d];
        Vector3[] verts = new Vector3[w * d];
        int[] triangles = new int[w * d * 6];

        for (int z = 0; z < d; z++) {
			for (int x = 0; x < w; x++) {
                float height = heightmap.GetData(x, z).height;
                verts[x + z * w] = new Vector3(x - width / 2, height, z - depth / 2) + transform.position;
                uvs[x + z * w] = new Vector2((float)x / texture.width, (float)z / texture.height);
            }
		}

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[(x + z * width) * 6] = (x) + (z) * w;
                triangles[(x + z * width) * 6 + 1] = (x + 1) + (z + 1) * w;
                triangles[(x + z * width) * 6 + 2] = (x + 1) + (z) * w;
                triangles[(x + z * width) * 6 + 3] = (x) + (z) * w;
                triangles[(x + z * width) * 6 + 4] = (x) + (z + 1) * w;
                triangles[(x + z * width) * 6 + 5] = (x + 1) + (z + 1) * w;
            }
        }

        mesh.Clear ();
		mesh.vertices = verts;
		mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals ();
		
		return mesh;
	}
}

public struct HeightMapDatum
{
    public HeightMapDatum(float height, float moisture, float specles)
    {
        this.height = height;
        this.moisture = moisture;
        this.specles = specles;
    }

    public float height;
    public float moisture;
    public float specles;
}

public class HeightMap {
    HeightMapDatum[,] heightmap;
    int width, depth;
    public HeightMap xNeighbour;
    public HeightMap zNeighbour;
    public HeightMap xzNeighbour;
    public delegate HeightMapDatum Generator(float x, float z);
    Generator generator;

    public HeightMap(int width, int depth, Vector2 offset, Generator generator) {
        heightmap = new HeightMapDatum[width, depth];
        this.generator = generator;
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                heightmap[x, z] = generator(x + offset.x, z + offset.y);
            }
        }

        this.width = width;
        this.depth = depth;
    }

    public HeightMapDatum GetData(int x, int z)
    {
        if (x < width && z < depth)
            return heightmap[x, z];

        if (x == width && z < depth)
            try
            {
                return xNeighbour.heightmap[0, z];
            }
            catch (System.NullReferenceException)
            {
                return heightmap[width - 1, z];
            }

        if (x < width && z == depth)
            try
            {
                return zNeighbour.heightmap[x, 0];
            }
            catch (System.NullReferenceException)
            {
                return heightmap[x, depth - 1];
            }

        if (x == width && z == depth)
            try
            {
                return xzNeighbour.heightmap[0, 0];
            }
            catch (System.NullReferenceException)
            {
                return heightmap[width - 1, depth - 1];
            }

        throw new System.IndexOutOfRangeException();
    }
}
