using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TerrainCell : MonoBehaviour {
	public HeightMap heightmap;
	public Texture2D texture;
    public int width, depth;
    public float height;

	void Start () {
		gameObject.name = "Terrain Cell";
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer> ();
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter> ();

		meshRenderer.material.mainTexture = texture;
		meshFilter.mesh = GenerateMesh (heightmap);
	}

    public void UpdateMesh()
    {
        try
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            meshFilter.mesh = GenerateMesh(heightmap);
        }
        catch (MissingComponentException) { }
    }
	
	Mesh GenerateMesh(HeightMap heightmap) {
		Mesh mesh = new Mesh ();
		
		List<Vector3> verts = new List<Vector3> ();
		List<Vector2> uvs = new List<Vector2>();
		
		for (int z = -depth / 2; z < depth / 2; z++) {
			for (int x = -width / 2; x < width / 2; x++) {
                verts.AddRange (GenerateQuad (new Vector3 (x, 0, z),
				                              heightmap.GetHeight(x, z) * height,
				                              heightmap.GetHeight(x, z + 1) * height,
				                              heightmap.GetHeight(x + 1, z) * height,
				                              heightmap.GetHeight(x + 1, z + 1) * height
				                              ));
				uvs.AddRange (GenerateUvQuad (new Vector2 (x + width / 2, z + depth / 2)));
			}
		}
		
		mesh.Clear ();
		mesh.vertices = verts.ToArray();
		mesh.triangles = Enumerable.Range (0, verts.Count).ToArray ();
		mesh.RecalculateNormals ();
		mesh.uv = uvs.ToArray();
		
		return mesh;
	}
	
	List<Vector3> GenerateQuad(Vector3 offset, float top_left, float top_right, float bottom_left, float bottom_right) {
		Vector3 scale = new Vector3(1.0f / width, 0, 1.0f / depth);
		offset.Scale (scale);
		Vector3 tl = new Vector3 (0, top_left, 0) + offset;
		Vector3 tr = new Vector3 (0, top_right, scale.z) + offset;
		Vector3 bl = new Vector3 (scale.x, bottom_left, 0) + offset;
		Vector3 br = new Vector3 (scale.x, bottom_right, scale.z) + offset;
		
		return new List<Vector3>{tl, tr, br, tl, br, bl};
	}
	
	List<Vector2> GenerateUvQuad(Vector2 offset) {
		Vector2 scale = new Vector3(1.0f / width, 1.0f / depth);
		offset.Scale (scale);
		Vector2 tl = new Vector2 (0, 0) + offset;
		Vector2 tr = new Vector2 (0, scale.y) + offset;
		Vector2 bl = new Vector2 (scale.x, 0) + offset;
		Vector2 br = new Vector2 (scale.x, scale.y) + offset;
		
		return new List<Vector2>{tl, tr, br, tl, br, bl};
	}
}

public class HeightMap {
    struct HeightMapDatum {
		public float height;
	}

    HeightMapDatum[,] heightmap;
    int width, depth;
    public HeightMap xNeighbour;
    public HeightMap zNeighbour;
    public HeightMap xzNeighbour;

    public HeightMap(int width, int depth, Vector2 offset) {
        heightmap = new HeightMapDatum[width, depth];

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                heightmap[x, z].height = Mathf.PerlinNoise((x + offset.x) / 5, (z + offset.y) / 5);
            }
        }

        this.width = width;
        this.depth = depth;
    }

    public float GetHeight(int x, int z) {
        if (x < width / 2 && z < depth / 2)
            return heightmap[x + width / 2, z + depth / 2].height;

        if (x == width / 2 && z < depth / 2)
            try
            {
                return xNeighbour.heightmap[0, z + depth / 2].height;
            }
            catch (System.NullReferenceException)
            {
                return heightmap[width - 1, z + depth / 2].height;
            }

        if (x < width / 2 && z == depth / 2)
            try
            {
                return zNeighbour.heightmap[x + width / 2, 0].height;
            }
            catch (System.NullReferenceException)
            {
                return heightmap[x + width / 2, depth - 1].height;
            }
        
        if (x == width / 2 && z == depth / 2)
            try
            {
                return xzNeighbour.heightmap[0, 0].height;
            }
            catch (System.NullReferenceException)
            {
                return heightmap[width - 1, depth - 1].height;
            }

        return 4;
	}
}
