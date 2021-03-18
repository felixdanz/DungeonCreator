using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
	[Header("Visualization")] [SerializeField]
	private GameObject tileFloorPrefab;

	[Header("Configuration")] [SerializeField]
	private int seed = 123456;

	[SerializeField] private Vector2Int dungeonSize = new Vector2Int(100, 100);
	[SerializeField] private Vector2Int minSectorSize = new Vector2Int(10, 10);
	[SerializeField] private Vector2Int minRoomSize = new Vector2Int(5, 5);
	[SerializeField] private int minWidthConnector = 1;
	[SerializeField] private int maxWidthConnector = 2;
	[SerializeField] private int maxSplitDepth = 5;

	private System.Random _rng;
	private Dungeon _dungeon;
	private GameObject _visualizedDungeon;


	public void CreateDungeon()
	{
		_rng = new System.Random(seed);

		// TODO(FD): check validity of parameters
		_dungeon = new Dungeon(
			_rng,
			Vector2Int.zero,
			dungeonSize,
			minSectorSize,
			minRoomSize,
			minWidthConnector,
			maxWidthConnector,
			maxSplitDepth,
			0,
			SplitMode.Horizontal);

		_dungeon.CreateRooms();
		_dungeon.CreateConnectors();

		var dungeonArray = CreateVisualizationGrid(
			_dungeon,
			dungeonSize.x,
			dungeonSize.y);

		VisualizeDungeon(
			dungeonArray,
			dungeonSize.x,
			dungeonSize.y);
		
		DEBUG_VisualizeConnectors(_dungeon);
	}

	public void DeleteDungeon()
	{
		if (_visualizedDungeon != null)
			DestroyImmediate(_visualizedDungeon);
	}

	private int[] CreateVisualizationGrid(Dungeon dungeon, int width, int height)
	{
		var gridArray = new int[width * height];

		var rooms = dungeon.GetRooms();
		var connectors = dungeon.GetConnectors();

		foreach (var (roomStart, roomEnd) in rooms)
		{
			for (var z = roomStart.y; z < roomEnd.y; z++)
			for (var x = roomStart.x; x < roomEnd.x; x++)
			{
				var gridIndex = z * width + x;
				gridArray[gridIndex] = 1;
			}
		}

		foreach (var connectorData in connectors)
		{
			// TODO(FD): overhaul this, unnecessary information
			var (connectorStartPos, connectorCenterPos, connectorEndPos) = connectorData;
			var (connectorStartBegin, connectorStartEnd) = connectorStartPos;
			var (connectorCenterBegin, connectorCenterEnd) = connectorCenterPos;
			var (connectorEndBegin, connectorEndEnd) = connectorEndPos;

			for (var z = connectorStartBegin.y; z <= connectorCenterEnd.y; z++)
			for (var x = connectorStartBegin.x; x <= connectorCenterEnd.x; x++)
			{
				var gridIndex = z * width + x;
				gridArray[gridIndex] = 1;
			}

			for (var z = connectorCenterBegin.y; z <= connectorEndEnd.y; z++)
			for (var x = connectorCenterBegin.x; x <= connectorEndEnd.x; x++)
			{
				var gridIndex = z * width + x;
				gridArray[gridIndex] = 1;
			}
		}

		return gridArray;
	}
	
	private void VisualizeDungeon(int[] visualizationGrid, int width, int height)
	{
		DeleteDungeon();

		_visualizedDungeon = new GameObject("Dungeon");

		for (var i = 0; i < visualizationGrid.Length; i++)
		{
			if (visualizationGrid[i] == 0)
				continue;

			var x = i % width;
			var z = i / width;

			var tileInstance = Instantiate(
				tileFloorPrefab,
				new Vector3(x, 0, z),
				Quaternion.identity);

			tileInstance.transform.parent = _visualizedDungeon.transform;
		}
	}

	private void DEBUG_VisualizeConnectors(Dungeon dungeon)
	{
		foreach (var connectorData in dungeon.GetConnectors())
		{
			var (connectorStartBegin, connectorStartEnd) = connectorData.Item1;
			var (connectorCenterBegin, connectorCenterEnd) = connectorData.Item2;
			var (connectorEndBegin, connectorEndEnd) = connectorData.Item3;
			
			Debug.DrawLine(
				new Vector3(connectorStartBegin.x, 1, connectorStartBegin.y),
				new Vector3(connectorCenterBegin.x, 1, connectorCenterBegin.y),
				Color.red,
				100.0f);
			
			Debug.DrawLine(
				new Vector3(connectorCenterBegin.x, 1, connectorCenterBegin.y),
				new Vector3(connectorEndBegin.x, 1, connectorEndBegin.y),
				Color.red,
				100.0f);
			
			Debug.DrawLine(
				new Vector3(connectorStartEnd.x, 1, connectorStartEnd.y),
				new Vector3(connectorCenterEnd.x, 1, connectorCenterEnd.y),
				Color.red,
				100.0f);
			
			Debug.DrawLine(
				new Vector3(connectorCenterEnd.x, 1, connectorCenterEnd.y),
				new Vector3(connectorEndEnd.x, 1, connectorEndEnd.y),
				Color.red,
				100.0f);
		}
	}
}
