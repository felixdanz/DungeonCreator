using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
	[Header("Visualization")] 
	
	[SerializeField] private GameObject tileFloorRoomPrefab;
	[SerializeField] private GameObject tileFloorConnectorPrefab;
	
	[Header("Configuration")] 
	
	[SerializeField] private int seed = 123456;
	
	[SerializeField] private Vector2Int dungeonSize = new Vector2Int(100, 100);
	[SerializeField] private Vector2Int minSectorSize = new Vector2Int(10, 10);
	[SerializeField] private Vector2Int minRoomSize = new Vector2Int(5, 5);
	[SerializeField] private int minWidthConnector = 1;
	[SerializeField] private int maxWidthConnector = 2;
	[SerializeField] private int maxSplitDepth = 5;
	
	private System.Random _rng;
	private BSP.Dungeon _dungeon;
	private GameObject _visualizedDungeon;
	
	
	public void CreateDungeon()
	{
		_rng = new System.Random(seed);
		
		_dungeon = new BSP.Dungeon(
			_rng,
			new BSP.Vector2Int(0, 0),
			new BSP.Vector2Int(dungeonSize.x, dungeonSize.y),
			new BSP.Vector2Int(minSectorSize.x, minSectorSize.y),
			new BSP.Vector2Int(minRoomSize.x, minRoomSize.y),
			minWidthConnector,
			maxWidthConnector,
			maxSplitDepth,
			0,
			BSP.SplitMode.Horizontal);

		_dungeon.CreateRooms();
		_dungeon.CreateConnectors();

		VisualizeDungeon(
			_dungeon.ToIntArray(),
			dungeonSize.x,
			dungeonSize.y);
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
			else if (dungeonArray[i] == 2)
			{
				var tileInstance = Instantiate(
					tileFloorConnectorPrefab,
					new Vector3(x, 0, z),
					Quaternion.identity);

				tileInstance.transform.parent = _visualizedDungeon.transform;
			}
		}
	}
}
