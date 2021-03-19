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
			var (connectorStartPos, connectorCenterPos, connectorEndPos) = connectorData;
			var (connectorStartBegin, connectorStartEnd) = connectorStartPos;
			var (connectorCenterBegin, connectorCenterEnd) = connectorCenterPos;
			var (connectorEndBegin, connectorEndEnd) = connectorEndPos;

			var (startIndexZ, targetIndexZ) = connectorStartBegin.y <= connectorCenterEnd.y
				? (connectorStartBegin.y, connectorCenterEnd.y)
				: (connectorCenterEnd.y, connectorStartBegin.y);
			
			var (startIndexX, targetIndexX) = connectorStartBegin.x <= connectorCenterEnd.x
				? (connectorStartBegin.x, connectorCenterEnd.x)
				: (connectorCenterEnd.x, connectorStartBegin.x);
			
			for (var z = startIndexZ; z <= targetIndexZ; z++)
			for (var x = startIndexX; x <= targetIndexX; x++)
			{
				var gridIndex = z * width + x;
				gridArray[gridIndex] = 1;
			}
			
			(startIndexZ, targetIndexZ) = connectorCenterBegin.y <= connectorEndEnd.y
				? (connectorCenterBegin.y, connectorEndEnd.y)
				: (connectorEndEnd.y, connectorCenterBegin.y);
			
			(startIndexX, targetIndexX) = connectorCenterBegin.x <= connectorEndEnd.x
				? (connectorCenterBegin.x, connectorEndEnd.x)
				: (connectorEndEnd.x, connectorCenterBegin.x);

			for (var z = startIndexZ; z <= targetIndexZ; z++)
			for (var x = startIndexX; x <= targetIndexX; x++)
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
}
