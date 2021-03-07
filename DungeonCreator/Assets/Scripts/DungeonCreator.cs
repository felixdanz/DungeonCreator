using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
	[Header("Visualization")]
	[SerializeField] private GameObject groundTilePrefab;
	[Header("Configuration")]
	[SerializeField] private int seed = 123456;
	[SerializeField] private Vector2Int dungeonSize = new Vector2Int(100, 100);
	[SerializeField] private Vector2Int minRoomSize = new Vector2Int(5, 5);
	[SerializeField] private int maxSliceDepth = 10;

	private System.Random _rng;
	private GameObject _dungeonObject;


	public void GenerateDungeon()
	{
		_rng = new System.Random(seed);
		
		var roomList = new List<DungeonRoom>();
		var initialRoom = new DungeonRoom()
		{
			XMin = 0,
			XMax= dungeonSize.x,
			ZMin = 0,
			ZMax = dungeonSize.y,
		};
		
		GenerateRooms(
			initialRoom,
			SliceMode.Vertical, 
			0,
			ref roomList);
		
		VisualizeDungeon(roomList);
	}

	public void DeleteDungeon()
	{
		if (_dungeonObject != null)
			DestroyImmediate(_dungeonObject);
	}
	
	private void GenerateRooms(
		DungeonRoom roomToSlice,
		SliceMode sliceMode,
		int currentSliceDepth,
		ref List<DungeonRoom> rooms)
	{
		if (currentSliceDepth == maxSliceDepth)
		{
			rooms.Add(roomToSlice);
			return;
		}

		var (sliceMin, sliceMax) = sliceMode == SliceMode.Vertical
			? (roomToSlice.XMin, roomToSlice.XMax)
			: (roomToSlice.ZMin, roomToSlice.ZMax);

		var minLength = sliceMode == SliceMode.Vertical
			? minRoomSize.x
			: minRoomSize.y;

		sliceMin += minLength;
		sliceMax -= minLength;
		
		if (sliceMax - sliceMin <= minLength)
		{
			rooms.Add(roomToSlice);
			return;
		}

		var slicePosition = _rng.Next(sliceMin, sliceMax);

		var roomA = new DungeonRoom();
		var roomB = new DungeonRoom();
		
		if (sliceMode == SliceMode.Vertical)
		{
			roomA.XMin = roomToSlice.XMin;
			roomA.XMax = slicePosition;
			roomA.ZMin = roomToSlice.ZMin;
			roomA.ZMax = roomToSlice.ZMax;
			
			roomB.XMin = slicePosition;
			roomB.XMax = roomToSlice.XMax;
			roomB.ZMin = roomToSlice.ZMin;
			roomB.ZMax = roomToSlice.ZMax;
		}
		else
		{
			roomA.XMin = roomToSlice.XMin;
			roomA.XMax = roomToSlice.XMax;
			roomA.ZMin = roomToSlice.ZMin;
			roomA.ZMax = slicePosition;

			roomB.XMin = roomToSlice.XMin;
			roomB.XMax = roomToSlice.XMax;
			roomB.ZMin = slicePosition;
			roomB.ZMax = roomToSlice.ZMax;
		}

		var newSliceMode = sliceMode == SliceMode.Vertical
			? SliceMode.Horizontal
			: SliceMode.Vertical;

		GenerateRooms(roomA, newSliceMode, ++currentSliceDepth, ref rooms);
		GenerateRooms(roomB, newSliceMode, ++currentSliceDepth, ref rooms);
	}
	
	private void VisualizeDungeon(List<DungeonRoom> rooms)
	{
		DeleteDungeon();
		
		_dungeonObject = new GameObject("Dungeon");

		for (int i = 0; i < rooms.Count; i++)
		{
			var room = rooms[i];
			var roomParent = new GameObject($"room_{i}");
			roomParent.transform.parent = _dungeonObject.transform;
			
			// TODO(FD): Max -1 for early visualization
			for (int x = room.XMin; x < room.XMax - 1; x++)
			for (int z = room.ZMin; z < room.ZMax - 1; z++)
			{
				var tilePosition = new Vector3(x, 0, z);
				var roomInstance = Instantiate(groundTilePrefab, tilePosition, Quaternion.identity);
				roomInstance.transform.parent = roomParent.transform;
			}

			UnityEngine.Debug.Log($"room_{i}: {room.XMin} {room.XMax} {room.ZMin} {room.ZMax}");
		}
	}
}

public enum SliceMode
{
	Horizontal,
	Vertical,
}
