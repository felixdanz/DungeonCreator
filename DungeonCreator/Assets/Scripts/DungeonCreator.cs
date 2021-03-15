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
	[SerializeField] private Vector2Int minRoomSize = new Vector2Int(5, 5);
	[SerializeField] private int maxSliceDepth = 10;

	private System.Random _rng;
	private GameObject _dungeonObject;


	public void GenerateDungeon()
	{
		_rng = new System.Random(seed);

		var dungeon = new Dungeon { Rooms = new List<DungeonRoom>() };
		
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
			ref dungeon.Rooms);
		
		VisualizeDungeon(dungeon);
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
			
			roomB.XMin = slicePosition + 1;
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
			roomB.ZMin = slicePosition + 1;
			roomB.ZMax = roomToSlice.ZMax;
		}
		
		var newSliceMode = sliceMode == SliceMode.Vertical
			? SliceMode.Horizontal
			: SliceMode.Vertical;
		
		GenerateRooms(roomA, newSliceMode, ++currentSliceDepth, ref rooms);
		GenerateRooms(roomB, newSliceMode, ++currentSliceDepth, ref rooms);
	}

	private void VisualizeDungeon(Dungeon dungeon)
	{
		DeleteDungeon();
		
		_dungeonObject = new GameObject("Dungeon");

		foreach (var room in dungeon.Rooms)
		{
			var roomParent = new GameObject($"room");
			roomParent.transform.parent = _dungeonObject.transform;

			var cornerPositions = new Vector3[]
			{
				new Vector3(room.XMin, 0, room.ZMin),
				new Vector3(room.XMin, 0, room.ZMax),
				new Vector3(room.XMax, 0, room.ZMax),
				new Vector3(room.XMax, 0, room.ZMin),
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
			
			for (var x = room.XMin + 1; x <= room.XMax - 1; x++)
			{
				var tileWallInstanceA = Instantiate(
					tileWallPrefab, 
					new Vector3(x, 0, room.ZMin),
					Quaternion.Euler(rotations[0]));
				
				var tileWallInstanceB = Instantiate(
					tileWallPrefab, 
					new Vector3(x, 0, room.ZMax),
					Quaternion.Euler(rotations[2]));
				
				tileWallInstanceA.transform.parent = roomParent.transform;
				tileWallInstanceB.transform.parent = roomParent.transform;
			}
			
			for (var z = room.ZMin + 1; z <= room.ZMax - 1; z++)
			{
				var tileWallInstanceA = Instantiate(
					tileWallPrefab, 
					new Vector3(room.XMin, 0, z),
					Quaternion.Euler(rotations[1]));
				
				var tileWallInstanceB = Instantiate(
					tileWallPrefab, 
					new Vector3(room.XMax, 0, z),
					Quaternion.Euler(rotations[3]));
				
				tileWallInstanceA.transform.parent = roomParent.transform;
				tileWallInstanceB.transform.parent = roomParent.transform;
			}

			for (var z = room.ZMin + 1; z <= room.ZMax - 1; z++)
			for (var x = room.XMin + 1; x <= room.XMax - 1; x++)
			{
				var tilePosition = new Vector3(x, 0, z);
				var tileFloorInstance = Instantiate(tileFloorPrefab, tilePosition, Quaternion.identity);
				tileFloorInstance.transform.parent = roomParent.transform;
			}
		}
	}
}

public enum SliceMode
{
	Horizontal,
	Vertical,
}
