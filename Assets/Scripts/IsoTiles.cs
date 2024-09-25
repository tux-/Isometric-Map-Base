using UnityEngine;

public class IsoTiles : MonoBehaviour
{
	public static IsoTiles Instance {
		get; private set;
	}

	[SerializeField]
	private int x = 16;

	[SerializeField]
	private int z = 16;

	private MeshFilter meshFilter;
	private Mesh mesh;

	[HideInInspector]
	public float[] heightmap;

	[HideInInspector]
	public int[] mapVertHeight;

	[HideInInspector]
	public Vector3[] vertices {
		get; private set;
	}

	public int seed = 0;

	[Range(0, 20)]
	public int hillyness = 10;

	private void Start ()
	{
		Instance = this;
	}

	public void GenerateTerrain (int seed = 0, int hillyness = 10)
	{
		heightmap = new float[(x + 1) * (z + 1)];
		for (int _z = 0, index = 0; _z <= z; _z++) {
			for (int _x = 0; _x <= x; _x++, index++) {
				float res = Mathf.PerlinNoise((float) (_x + seed) / 20, (float) (_z + seed) / 20) * hillyness;
				res = Mathf.Floor(res) / 2;

				heightmap[index] = res;
			}
		}
		UpdadeVisuals();
	}

	public void Raise (int index)
	{
		int pt = mapVertHeight[index];
		_Raise(pt);
		UpdadeVisuals();
	}

	private void _Raise (int index)
	{
		heightmap[index] += 0.5f;
		RaiseLower(index);
	}

	public void Lower (int index)
	{
		int pt = mapVertHeight[index];
		_Lower(pt);
		UpdadeVisuals();
	}

	private void _Lower (int index)
	{
		heightmap[index] -= 0.5f;
		RaiseLower(index);
	}

	private void RaiseLower (int origin)
	{
		bool isLeftX = ((origin % (x + 1)) == 0);
		bool isRightX = ((origin % (x + 1)) == x);
		bool isBottomZ = (origin <= x);
		bool isTopZ = (origin >= ((x + 1) * z));

		if (!isLeftX) {
			if (heightmap[origin - 1] < (heightmap[origin] - 0.5f)) {
				_Raise(origin - 1);
			}
			if (heightmap[origin - 1] > (heightmap[origin] + 0.5f)) {
				_Lower(origin - 1);
			}
		}

		if (!isRightX) {
			if (heightmap[origin + 1] < (heightmap[origin] - 0.5f)) {
				_Raise(origin + 1);
			}
			if (heightmap[origin + 1] > (heightmap[origin] + 0.5f)) {
				_Lower(origin + 1);
			}
		}

		if (!isBottomZ) {
			if (heightmap[origin - x - 1] < (heightmap[origin] - 0.5f)) {
				_Raise(origin - x - 1);
			}
			if (heightmap[origin - x - 1] > (heightmap[origin] + 0.5f)) {
				_Lower(origin - x - 1);
			}
		}

		if (!isTopZ) {
			if (heightmap[origin + x + 1] < (heightmap[origin] - 0.5f)) {
				_Raise(origin + x + 1);
			}
			if (heightmap[origin + x + 1] > (heightmap[origin] + 0.5f)) {
				_Lower(origin + x + 1);
			}
		}
	}

	public void UpdadeVisuals ()
	{
		if (meshFilter == null) {
			meshFilter = GetComponent<MeshFilter>();
		}
		if (meshFilter == null) {
			meshFilter = gameObject.AddComponent<MeshFilter>();
		}

		mesh = new Mesh();
		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		int tileCount = x * z;
		int[] triangles = new int[tileCount * 6];
		Vector2[] uvs = new Vector2[tileCount * 6];

		vertices = new Vector3[tileCount * 6];
		mapVertHeight = new int[tileCount * 6];
		for (int _z = 0, i = 0, index = 0; _z < z; _z++) {
			for (int _x = 0; _x < x; _x++, index++, i += 6) {
				vertices[i] = new Vector3(_x, heightmap[index], _z);
				mapVertHeight[i] = index;
				vertices[i + 1] = new Vector3(_x, heightmap[index + x + 1], _z + 1);
				mapVertHeight[i + 1] = index + x + 1;

				if (heightmap[index + x + 1] != heightmap[index + 1]) {
					vertices[i + 2] = new Vector3(_x + 1, heightmap[index + x + 2], _z + 1);
					mapVertHeight[i + 2] = index + x + 2;

					vertices[i + 3] = new Vector3(_x + 1, heightmap[index + x + 2], _z + 1);
					mapVertHeight[i + 3] = index + x + 2;
					vertices[i + 4] = new Vector3(_x + 1, heightmap[index + 1], _z);
					mapVertHeight[i + 4] = index + 1;
					vertices[i + 5] = new Vector3(_x, heightmap[index], _z);
					mapVertHeight[i + 5] = index;
				}
				else {
					vertices[i + 2] = new Vector3(_x + 1, heightmap[index + 1], _z);
					mapVertHeight[i + 2] = index + 1;

					vertices[i + 3] = new Vector3(_x, heightmap[index + x + 1], _z + 1);
					mapVertHeight[i + 3] = index + x + 1;
					vertices[i + 4] = new Vector3(_x + 1, heightmap[index + x + 2], _z + 1);
					mapVertHeight[i + 4] = index + x + 2;
					vertices[i + 5] = new Vector3(_x + 1, heightmap[index + 1], _z);
					mapVertHeight[i + 5] = index + 1;
				}

				triangles[i] = i;
				triangles[i + 1] = i + 1;
				triangles[i + 2] = i + 2;

				triangles[i + 3] = i + 3;
				triangles[i + 4] = i + 4;
				triangles[i + 5] = i + 5;

				uvs[i] = new Vector2(0, 0);
				uvs[i + 1] = new Vector2(1, 0);
				uvs[i + 2] = new Vector2(0, 1);

				uvs[i + 3] = new Vector2(1, 0);
				uvs[i + 4] = new Vector2(0, 1);
				uvs[i + 5] = new Vector2(1, 1);

				if (_x == x - 1) {
					index++;
				}
			}
		}


		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		meshFilter.mesh = mesh;

		mesh.RecalculateNormals();

		if (GetComponent<MeshRenderer>() == null) {
			gameObject.AddComponent<MeshRenderer>();
		}
	}
}
