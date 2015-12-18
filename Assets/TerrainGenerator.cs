using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TerrainGenerator : MonoBehaviour
{
	public GameObject target;
    public GameObject particles;

	Dictionary<Vector2, GameObject> cells;
    float seed;
    

	void Start ()
    {
        seed = Random.Range(-100f, 100f);
        cells = new Dictionary<Vector2, GameObject>();
        UpdateCells(target.transform.position, 170, 50, 50, 100);
    }

    HeightMapDatum TerrainGen(float x, float z)
    {
        float quality = 0.5f + Mathf.Clamp(Mathf.PerlinNoise(-seed + x / 150, seed + z / 150), 0, 1) * 1.5f;
        float maxHeight = 0.1f + Mathf.Clamp(Mathf.PerlinNoise(seed + x / 200, -seed + z / 200), 0, 1) * 0.9f;
        float height = Mathf.Pow(Mathf.Clamp(Mathf.PerlinNoise(-seed + x / 21, seed + z / 21) * maxHeight, 0, maxHeight), 1 / quality);
        float moisture = Mathf.Clamp(Mathf.PerlinNoise(seed + x / 1000, -seed + z / 1000), 0, 1) / 2.25f;
        float specles = Random.Range(0f, Mathf.Clamp(Mathf.PerlinNoise(seed + x / 100, seed + z / 100) * 0.15f, 0.01f, 0.1f));

        height -= specles / 15;

        if (height <= 0)
        {
            height = 0.05f;
        }

        if (moisture > height)
        {
            height = Mathf.Max(height, moisture);
            moisture = Mathf.Min(moisture * 2, 0.6f);
        }

        return new HeightMapDatum(height, moisture, specles);
    }

	void UpdateCells(Vector3 center, float radius, int xScale, int zScale, int maxGenerate)
    {
        int generateCount = 0;
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
                        generateCount += 1;
                        if (generateCount > maxGenerate)
                            continue;
                        cell = new GameObject();

                        TerrainCell cellComponent = cell.AddComponent<TerrainCell>();
                        cell.transform.parent = transform;
                        cell.transform.position = new Vector3((x * transform.localScale.x) / (1 + transform.localScale.x),
                            0,
                            (z * transform.localScale.z) / (1 + transform.localScale.z));
                        cell.transform.localScale = Vector3.one;

                        GameObject newParticles = GameObject.Instantiate(particles);

                        
                        newParticles.transform.position = new Vector3(x * transform.localScale.x, 700, z * transform.localScale.z);
                        
                        newParticles.transform.parent = transform;


                        cellComponent.particles = newParticles;
                        cellComponent.width = xScale;
                        cellComponent.depth = zScale;
                        cellComponent.height = 1;
                        cellComponent.heightmap = new HeightMap(xScale, zScale, pos, TerrainGen);
                        
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
                Destroy(cell.GetComponent<TerrainCell>().particles);
                Destroy(cell);
				invalidPositions.Add(pair.Key);
			}
		}

		foreach (var pos in invalidPositions) {
			cells.Remove(pos);
		}
	}

	void Update () {
        UpdateCells (target.transform.position, 170, 50, 50, 1);
    }
}