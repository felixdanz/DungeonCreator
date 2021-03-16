using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
	[Header("Visualization")]
	[SerializeField] private GameObject tileFloorPrefab;
	[SerializeField] private GameObject tileWallPrefab;
	[SerializeField] private GameObject tileCornerPrefab;
	
	[Header("Configuration")]
	[SerializeField] private int seed = 123456;
	[SerializeField] private Vector2Int dungeonSize = new Vector2Int(100, 100);
	[SerializeField] private Vector2Int minSectorSize = new Vector2Int(10, 10);
	[SerializeField] private Vector2Int minRoomSize = new Vector2Int(5, 5);
	[SerializeField] private int maxSplitDepth = 5;

	private System.Random _rng;
	private Dungeon _dungeon;
	private GameObject _visualizedDungeon;


	public void CreateDungeon()
	{
		_rng = new System.Random(seed);
		
		_dungeon = new Dungeon(
			_rng, 
			Vector2Int.zero, 
			dungeonSize, 
			minSectorSize,
			minRoomSize,
			maxSplitDepth, 
			0,
			SplitMode.Horizontal);
		
		_dungeon.CreateRooms();
		
		VisualizeDungeon(_dungeon);
	}

	public void DeleteDungeon()
	{
		if (_visualizedDungeon != null)
			DestroyImmediate(_visualizedDungeon);
	}

	private void VisualizeDungeon(Dungeon dungeon)
	{
		DeleteDungeon();
		
		_visualizedDungeon = new GameObject("Dungeon");

		var rooms = dungeon.GetRooms();
		
		foreach (var (roomStart, roomEnd) in rooms)
		{
			var xMin = roomStart.x;
			var xMax = roomEnd.x;
			var zMin = roomStart.y;
			var zMax = roomEnd.y;


			var roomParent = new GameObject($"room");
			roomParent.transform.parent = _visualizedDungeon.transform;
	
			var cornerPositions = new Vector3[]
			{
				new Vector3(xMin, 0, zMin),
				new Vector3(xMin, 0, zMax),
				new Vector3(xMax, 0, zMax),
				new Vector3(xMax, 0, zMin),
			};
	
			var rotations = new Vector3[]
			{
				new Vector3(0, 0, 0),
				new Vector3(0, 90, 0),
				new Vector3(0, 180, 0),
				new Vector3(0, 270, 0),
			};
	
			for (var i = 0; i < 4; i++)
			{
				var tileCornerInstance = Instantiate(
					tileCornerPrefab, 
					cornerPositions[i],
					Quaternion.Euler(rotations[i]));
				
				tileCornerInstance.transform.parent = roomParent.transform;
			}
			
			for (var x = xMin + 1; x <= xMax - 1; x++)
			{
				var tileWallInstanceA = Instantiate(
					tileWallPrefab, 
					new Vector3(x, 0, zMin),
					Quaternion.Euler(rotations[0]));
				
				var tileWallInstanceB = Instantiate(
					tileWallPrefab, 
					new Vector3(x, 0, zMax),
					Quaternion.Euler(rotations[2]));
				
				tileWallInstanceA.transform.parent = roomParent.transform;
				tileWallInstanceB.transform.parent = roomParent.transform;
			}
			
			for (var z = zMin + 1; z <= zMax - 1; z++)
			{
				var tileWallInstanceA = Instantiate(
					tileWallPrefab, 
					new Vector3(xMin, 0, z),
					Quaternion.Euler(rotations[1]));
				
				var tileWallInstanceB = Instantiate(
					tileWallPrefab, 
					new Vector3(xMax, 0, z),
					Quaternion.Euler(rotations[3]));
				
				tileWallInstanceA.transform.parent = roomParent.transform;
				tileWallInstanceB.transform.parent = roomParent.transform;
			}
	
			for (var z = zMin + 1; z <= zMax - 1; z++)
			for (var x = xMin + 1; x <= xMax - 1; x++)
			{
				var tilePosition = new Vector3(x, 0, z);
				var tileFloorInstance = Instantiate(tileFloorPrefab, tilePosition, Quaternion.identity);
				tileFloorInstance.transform.parent = roomParent.transform;
			}
		}
	}
}
