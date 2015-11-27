using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TerrainGenerator : MonoBehaviour
{
	public GameObject target;

	Dictionary<Vector2, GameObject> cells;
    

	void Start ()
    {
		cells = new Dictionary<Vector2, GameObject>();
    }

	Texture2D GenerateTexture(HeightMap heightmap, Vector3 offset, int width, int depth)
    {
		Texture2D texture = new Texture2D (width, depth);
		Color[] colors = new Color[texture.width * texture.height];

		for (int z = 0; z < depth; z++)
        {
			for (int x = 0; x < width; x++)
            {
				colors[x + z * width] = new Color(
                    Mathf.PingPong((offset.x + x) / 100, 1),
                    Mathf.PingPong((offset.z + z) / 100, 1),
                    1 - heightmap.GetHeight((int)offset.x + x, (int)offset.z + z)
                );
			}
		}

		texture.SetPixels(colors);
		texture.Apply ();
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;
		return texture;
	}

	void UpdateCells(Vector3 center, float radius, int xScale, int zScale)
    {
        center = new Vector3(center.x / transform.localScale.x, center.y / transform.localScale.y, center.z / transform.localScale.z);
        HashSet<GameObject> validCells = new HashSet<GameObject> ();

        Vector2 centerPos = new Vector2(center.x, center.z);

		for (int z = Mathf.FloorToInt((center.z - radius) / zScale) * zScale; z <= Mathf.CeilToInt((center.z + radius) / zScale) * zScale; z += zScale)
        {
			for (int x = Mathf.FloorToInt((center.x - radius) / xScale) * xScale; x <= Mathf.CeilToInt((center.x + radius) / xScale) * xScale; x += xScale)
            {
                Vector2 pos = new Vector2(x, z);
                if (Vector2.Distance(centerPos, pos) < radius + Mathf.Max(xScale, zScale) * 1.42f)
                {
                    GameObject cell;
                    if (cells.ContainsKey(pos))
                    {
                        cell = cells[pos];
                    }
                    else
                    {
                        cell = new GameObject();
                        TerrainCell cellComponent = cell.AddComponent<TerrainCell>();

                        cell.transform.parent = transform;
                        cell.transform.position = new Vector3(x * transform.localScale.x, 0, z * transform.localScale.z);
                        cell.transform.localScale = new Vector3(xScale, 1, zScale);

                        cellComponent.width = xScale;
                        cellComponent.depth = zScale;
                        cellComponent.height = 1;
                        cellComponent.heightmap = new HeightMap(xScale, zScale, pos);
                        
                        try
                        {
                            TerrainCell neighbour = cells[new Vector2(x + xScale, z)].GetComponent<TerrainCell>();
                            cellComponent.heightmap.xNeighbour = neighbour.heightmap;
                        }
                        catch (KeyNotFoundException) { }

                        try
                        {
                            TerrainCell neighbour = cells[new Vector2(x, z + zScale)].GetComponent<TerrainCell>();
                            cellComponent.heightmap.zNeighbour = neighbour.heightmap;
                        }
                        catch (KeyNotFoundException) { }

                        try
                        {
                            TerrainCell neighbour = cells[new Vector2(x + xScale, z + zScale)].GetComponent<TerrainCell>();
                            cellComponent.heightmap.xzNeighbour = neighbour.heightmap;
                        }
                        catch (KeyNotFoundException) { }



                        try
                        {
                            TerrainCell neighbour = cells[new Vector2(x - xScale, z)].GetComponent<TerrainCell>();
                            neighbour.heightmap.xNeighbour = cellComponent.heightmap;
                            neighbour.UpdateMesh();
                        }
                        catch (KeyNotFoundException) { }

                        try
                        {
                            TerrainCell neighbour = cells[new Vector2(x, z - zScale)].GetComponent<TerrainCell>();
                            neighbour.heightmap.zNeighbour = cellComponent.heightmap;
                            neighbour.UpdateMesh();
                        }
                        catch (KeyNotFoundException) { }

                        try
                        {
                            TerrainCell neighbour = cells[new Vector2(x - xScale, z - zScale)].GetComponent<TerrainCell>();
                            neighbour.heightmap.xzNeighbour = cellComponent.heightmap;
                            neighbour.UpdateMesh();
                        }
                        catch (KeyNotFoundException) { }

                        cellComponent.texture = GenerateTexture(cellComponent.heightmap, new Vector3(-xScale / 2, 0, -zScale / 2), xScale, zScale);

                        cells[pos] = cell;
                    }
                    validCells.Add(cell);
                }
			}
		}

		List<Vector2> invalidPositions = new List<Vector2> ();

		foreach (var pair in cells) {
			GameObject cell = pair.Value;
			if (!validCells.Contains(cell))
			{
				Destroy(cell);
				invalidPositions.Add(pair.Key);
			}
		}

		foreach (var pos in invalidPositions) {
			cells.Remove(pos);
		}
	}

	void Update () {
        UpdateCells (target.transform.position, 170, 60, 60);
    }
}