using DungeonCreator;
using UnityEngine;

public class HeightMapDungeonCreator : MonoBehaviour
{
	[Header("Visualization")] 
	
	[SerializeField] private GameObject tileFloorRoomPrefab;
	[SerializeField] private GameObject tileFloorConnectorPrefab;
	
	[Header("Configuration")] 
	
	[SerializeField] private int seed = 123456;
	
	[SerializeField] private int width = 100;
	[SerializeField] private int height = 100;
	[SerializeField] private float scale = 0.3f;
	[SerializeField] private int octaves = 4;
	[SerializeField] private float persistence = 0.5f;
	[SerializeField] private float lacunarity = 1.0f;
	[SerializeField] private int offsetX = 1234;
	[SerializeField] private int offsetY = 4321;
	[SerializeField] private float requiredHeightForFloor = 0.5f;

	public bool updateOnChange;
	
	private HeightMapDungeon _dungeon;
	private GameObject _visualizedDungeon;


	public void CreateDungeon()
	{
		_dungeon = new HeightMapDungeon(
			width,
			height,
			seed,
			scale,
			octaves,
			persistence,
			lacunarity,
			(offsetX, offsetY),
			requiredHeightForFloor);

		VisualizeDungeon(
			_dungeon.ToIntArray(),
			width,
			height);
	}
	
	public void DeleteDungeon()
	{
		if (_visualizedDungeon != null)
			DestroyImmediate(_visualizedDungeon);
	}
	
	private void VisualizeDungeon(int[] dungeonArray, int width, int height)
	{
		DeleteDungeon();
	
		_visualizedDungeon = new GameObject("Dungeon");
	
		for (var i = 0; i < dungeonArray.Length; i++)
		{
			if (dungeonArray[i] == 0)
				continue;
	
			var x = i % width;
			var z = i / width;
	
			if (dungeonArray[i] == 1)
			{
				var tileInstance = Instantiate(
					tileFloorRoomPrefab,
					new Vector3(x, 0, z),
					Quaternion.identity);
	
				tileInstance.transform.parent = _visualizedDungeon.transform;
			}
		}
	}

	private void OnValidate()
	{
		if (width < 1)
			width = 1;

		if (height < 1)
			height = 1;

		if (octaves < 1)
			octaves = 1;

		if (persistence < 0.0f)
			persistence = 0.0f;

		if (persistence > 1.0f)
			persistence = 1.0f;

		if (lacunarity < 1.0f)
			lacunarity = 1.0f;
	}
}