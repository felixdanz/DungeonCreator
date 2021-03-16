using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dungeon
{
	private readonly System.Random _rng;
	
	private Vector2Int _sectorStartPosition;
	private Vector2Int _sectorEndPosition;
	
	private Vector2Int _minSectorSize;
	private Vector2Int _minRoomSize;
	
	private Vector2Int _roomStartPosition;
	private Vector2Int _roomEndPosition;
	
	private Dungeon _leftLeaf;
	private Dungeon _rightLeaf;
	
	
	public Dungeon(
		System.Random rng, 
		Vector2Int sectorStartPosition, 
		Vector2Int sectorEndPosition, 
		Vector2Int minSectorSize,
		Vector2Int minRoomSize,
		int maxSplitDepth,
		int currentSplitDepth,
		SplitMode splitMode)
	{
		_rng = rng;
		_sectorStartPosition = sectorStartPosition;
		_sectorEndPosition = sectorEndPosition;
		_minSectorSize = minSectorSize;
		_minRoomSize = minRoomSize;

		GenerateLeaves(maxSplitDepth, currentSplitDepth, splitMode);
	}

	private void GenerateLeaves(int maxSplitDepth, int currentSplitDepth, SplitMode splitMode)
	{
		if (currentSplitDepth >= maxSplitDepth)
			return;
		
		currentSplitDepth++;
		splitMode = splitMode == SplitMode.Horizontal
			? SplitMode.Vertical
			: SplitMode.Horizontal;

		var (splitStart, splitEnd) = splitMode == SplitMode.Horizontal
			? (_sectorStartPosition.y + _minSectorSize.y + 1, _sectorEndPosition.y - _minSectorSize.y - 1)
			: (_sectorStartPosition.x + _minSectorSize.x + 1, _sectorEndPosition.x - _minSectorSize.x - 1);

		if (splitEnd - splitStart <= 1)
			return;
		
		var splitPosition = _rng.Next(splitStart + 1, splitEnd);
		
		Vector2Int startPositionLeftLeaf;
		Vector2Int startPositionRightLeaf;
		
		Vector2Int endPositionLeftLeaf;
		Vector2Int endPositionRightLeaf;
		
		if (splitMode == SplitMode.Horizontal)
		{
			startPositionLeftLeaf = new Vector2Int(_sectorStartPosition.x, _sectorStartPosition.y);
			startPositionRightLeaf = new Vector2Int(_sectorStartPosition.x, splitPosition + 1);
			
			endPositionLeftLeaf = new Vector2Int(_sectorEndPosition.x, splitPosition - 1);
			endPositionRightLeaf = new Vector2Int(_sectorEndPosition.x, _sectorEndPosition.y);
		}
		else
		{
			startPositionLeftLeaf = new Vector2Int(_sectorStartPosition.x, _sectorStartPosition.y);
			startPositionRightLeaf = new Vector2Int(splitPosition + 1, _sectorStartPosition.y);
			
			endPositionLeftLeaf = new Vector2Int(splitPosition - 1, _sectorEndPosition.y);
			endPositionRightLeaf = new Vector2Int(_sectorEndPosition.x, _sectorEndPosition.y);
		}
		
		_leftLeaf = new Dungeon(
			_rng, 
			startPositionLeftLeaf, 
			endPositionLeftLeaf, 
			_minSectorSize,
			_minRoomSize,
			maxSplitDepth, 
			currentSplitDepth, 
			splitMode);
		
		_rightLeaf = new Dungeon(
			_rng, 
			startPositionRightLeaf, 
			endPositionRightLeaf, 
			_minSectorSize,
			_minRoomSize,
			maxSplitDepth, 
			currentSplitDepth, 
			splitMode);
	}

	public void CreateRooms()
	{
		if (_leftLeaf == null || _rightLeaf == null)
		{
			var endReducedByMin = _sectorEndPosition - _minRoomSize;
			
			var newStartX = _rng.Next(_sectorStartPosition.x, endReducedByMin.x);
			var newStartY = _rng.Next(_sectorStartPosition.y, endReducedByMin.y);

			var newXLength = _sectorEndPosition.x - newStartX;
			var newYLength = _sectorEndPosition.y - newStartY;

			_roomStartPosition.x = _rng.Next(_sectorStartPosition.x, _sectorEndPosition.x - newXLength);
			_roomStartPosition.y = _rng.Next(_sectorStartPosition.y, _sectorEndPosition.y - newYLength);

			_roomEndPosition.x = _roomStartPosition.x + newXLength;
			_roomEndPosition.y = _roomStartPosition.y + newYLength;
			
			return;
		}
		
		_leftLeaf.CreateRooms();
		_rightLeaf.CreateRooms();
	}

	public void GenerateConnectors()
	{
		
	}

	public IEnumerable<(Vector2Int, Vector2Int)> GetRooms()
	{
		if (_leftLeaf == null || _rightLeaf == null)
			return new List<(Vector2Int, Vector2Int)>() {(_roomStartPosition, _roomEndPosition)};
		
		return _leftLeaf.GetRooms().Concat(_rightLeaf.GetRooms());
	}
}

public enum SplitMode
{
	Horizontal,
	Vertical,
}