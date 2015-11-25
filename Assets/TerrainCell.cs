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
	
	Mesh GenerateMesh(HeightMap heightmap) {
		Mesh mesh = new Mesh ();
		
		List<Vector3> verts = new List<Vector3> ();
		List<Vector2> uvs = new List<Vector2>();
		
		for (int z = 0; z < depth; z++) {
			for (int x = 0; x < width; x++) {
                int xPos = (int)(transform.position.x / transform.parent.localScale.x) + x;
                int zPos = (int)(transform.position.z / transform.parent.localScale.z) + z;
                verts.AddRange (GenerateQuad (new Vector3 (x - width / 2, 0, z - depth / 2),
				                              heightmap.GetHeight(xPos, zPos) * height,
				                              heightmap.GetHeight(xPos, zPos + 1) * height,
				                              heightmap.GetHeight(xPos + 1, zPos) * height,
				                              heightmap.GetHeight(xPos + 1, zPos + 1) * height
				                              ));
				uvs.AddRange (GenerateUvQuad (new Vector2 (x, z)));
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
	
	public HeightMap(int width, int depth) {
		heightmap = new HeightMapDatum[width, depth];

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                heightmap[x, z].height = Mathf.PerlinNoise((float)x / 5, (float)z / 5);
            }
        }

        this.width = width;
        this.depth = depth;
    }

	public float GetHeight(int x, int z) {
        try
        {
            return heightmap[x + width / 2, z + depth / 2].height;
        }
        catch (System.IndexOutOfRangeException)
        {
            return 0;
        }
	}
}
