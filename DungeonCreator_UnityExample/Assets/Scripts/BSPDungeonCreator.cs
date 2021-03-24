using System;
using DungeonCreator;
using UnityEngine;

public class BSPDungeonCreator : MonoBehaviour
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

	public bool updateOnChange;

	private BSPDungeon _dungeon;
	private GameObject _visualizedDungeon;
	
	
	public void CreateDungeon()
	{
		_dungeon = new BSPDungeon(
			seed,
			(0, 0),
			(dungeonSize.x, dungeonSize.y),
			(minSectorSize.x, minSectorSize.y),
			(minRoomSize.x, minRoomSize.y),
			minWidthConnector,
			maxWidthConnector,
			maxSplitDepth);
	
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

	private void OnValidate()
	{
		#region dungenSize

		if (dungeonSize.x < 1) 
			dungeonSize.x = 1;
		
		if (dungeonSize.y < 1) 
			dungeonSize.y = 1;

		#endregion

		#region minSectorSize

		if (minSectorSize.x < 1) 
			minSectorSize.y = 1;

		if (minSectorSize.y < 1) 
			minSectorSize.y = 1;
		
		if (minSectorSize.x > dungeonSize.x) 
			minSectorSize.x = dungeonSize.x;
		
		if (minSectorSize.y > dungeonSize.y) 
			minSectorSize.y = dungeonSize.y;
		
		#endregion

		#region minRoomSize

		if (minRoomSize.x < 1)
			minRoomSize.x = 1;
		
		if (minRoomSize.y < 1)
			minRoomSize.y = 1;
		
		if (minRoomSize.x > minSectorSize.x) 
			minRoomSize.x = minSectorSize.x;
		
		if (minRoomSize.y > minSectorSize.y) 
			minRoomSize.y = minSectorSize.y;

		#endregion

		#region min/maxWidthConnector

		if (minWidthConnector < 1)
			minWidthConnector = 1;
		
		if (maxWidthConnector < minWidthConnector)
			maxWidthConnector = minWidthConnector;

		#endregion
	}
}
