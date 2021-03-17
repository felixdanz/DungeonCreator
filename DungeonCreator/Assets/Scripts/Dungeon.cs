using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dungeon
{
	private readonly System.Random _rng;
	
	private Vector2Int _sectorStartPosition;
	private Vector2Int _sectorEndPosition;
	
	private readonly Vector2Int _minSectorSize;
	private readonly Vector2Int _minRoomSize;

	private readonly int _minWidthConnector;
	private readonly int _maxWidthConnector;
	
	private Vector2Int _roomStartPosition;
	private Vector2Int _roomEndPosition;
	
	private (Vector2Int, Vector2Int) _connectorStartPositions;
	private (Vector2Int, Vector2Int) _connectorCenterPositions;
	private (Vector2Int, Vector2Int) _connectorEndPositions;
	
	private Dungeon _leftNode;
	private Dungeon _rightNode;

	private bool IsLeaf => _leftNode == null || _rightNode == null;
	
	
	public Dungeon(
		System.Random rng, 
		Vector2Int sectorStartPosition, 
		Vector2Int sectorEndPosition, 
		Vector2Int minSectorSize,
		Vector2Int minRoomSize,
		int minWidthConnector,
		int maxWidthConnector,
		int maxSplitDepth,
		int currentSplitDepth,
		SplitMode splitMode)
	{
		_rng = rng;
		_sectorStartPosition = sectorStartPosition;
		_sectorEndPosition = sectorEndPosition;
		_minSectorSize = minSectorSize;
		_minRoomSize = minRoomSize;
		_minWidthConnector = minWidthConnector;
		_maxWidthConnector = maxWidthConnector;

		GenerateSectors(maxSplitDepth, currentSplitDepth, splitMode);
	}

	private void GenerateSectors(int maxSplitDepth, int currentSplitDepth, SplitMode splitMode)
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
		
		_leftNode = new Dungeon(
			_rng, 
			startPositionLeftLeaf, 
			endPositionLeftLeaf, 
			_minSectorSize,
			_minRoomSize,
			_minWidthConnector,
			_maxWidthConnector,
			maxSplitDepth, 
			currentSplitDepth, 
			splitMode);
		
		_rightNode = new Dungeon(
			_rng, 
			startPositionRightLeaf, 
			endPositionRightLeaf, 
			_minSectorSize,
			_minRoomSize,
			_minWidthConnector,
			_maxWidthConnector,
			maxSplitDepth, 
			currentSplitDepth, 
			splitMode);
	}

	public void CreateRooms()
	{
		if (_leftNode == null || _rightNode == null)
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
		
		_leftNode.CreateRooms();
		_rightNode.CreateRooms();
	}

	public void CreateConnectors()
	{
		if (_leftNode.IsLeaf && _rightNode.IsLeaf)
		{
			CreateConnectorBetweenLeaves();
			return;
		}
		
		_leftNode.CreateConnectors();
		_rightNode.CreateConnectors();
		
		CreateConnectorBetweenSectors();
	}

	private void CreateConnectorBetweenLeaves()
	{
		var connectorWidth = _rng.Next(_minWidthConnector, _maxWidthConnector + 1);
		var connectorVerticalFirst = Convert.ToBoolean(_rng.Next(0, 2));

		_connectorStartPositions.Item1 = new Vector2Int(
			_rng.Next(_leftNode._roomStartPosition.x + 1, _leftNode._roomEndPosition.x - connectorWidth + 1),
			_rng.Next(_leftNode._roomStartPosition.y + 1, _leftNode._roomEndPosition.y - connectorWidth + 1));
		
		_connectorEndPositions.Item1 = new Vector2Int(
			_rng.Next(_rightNode._roomStartPosition.x + 1, _rightNode._roomEndPosition.x - connectorWidth + 1),
			_rng.Next(_rightNode._roomStartPosition.y + 1, _rightNode._roomEndPosition.y - connectorWidth + 1));

		if (connectorVerticalFirst)
		{
			_connectorCenterPositions.Item1.x = _connectorStartPositions.Item1.x;
			_connectorCenterPositions.Item1.y = _connectorEndPositions.Item1.y;
		}
		else
		{
			_connectorCenterPositions.Item1.y = _connectorStartPositions.Item1.y;
			_connectorCenterPositions.Item1.x = _connectorEndPositions.Item1.x;
		}
	}

	private void CreateConnectorBetweenSectors()
	{
		var connectorWidth = _rng.Next(_minWidthConnector, _maxWidthConnector + 1);
		var connectorVerticalFirst = Convert.ToBoolean(_rng.Next(0, 2));

		_connectorStartPositions.Item1 = _leftNode._connectorCenterPositions.Item1;
		_connectorEndPositions.Item1 = _rightNode._connectorCenterPositions.Item1;

		if (connectorVerticalFirst)
		{
			_connectorCenterPositions.Item1.x = _connectorStartPositions.Item1.x;
			_connectorCenterPositions.Item1.y = _connectorEndPositions.Item1.y;
		}
		else
		{
			_connectorCenterPositions.Item1.y = _connectorStartPositions.Item1.y;
			_connectorCenterPositions.Item1.x = _connectorEndPositions.Item1.x;
		}
	}

	public IEnumerable<(Vector2Int, Vector2Int)> GetRooms()
	{
		if (_leftNode == null || _rightNode == null)
			return new List<(Vector2Int, Vector2Int)>() {(_roomStartPosition, _roomEndPosition)};
		
		return _leftNode.GetRooms().Concat(_rightNode.GetRooms());
	}

	// currently only used for debug visualization
	public IEnumerable<(Vector2Int, Vector2Int, Vector2Int)> GetConnectors()
	{
		if (IsLeaf)
			return new List<(Vector2Int, Vector2Int, Vector2Int)>();

		var result = new List<(Vector2Int, Vector2Int, Vector2Int)>()
		{
			(_connectorStartPositions.Item1, _connectorCenterPositions.Item1, _connectorEndPositions.Item1)
		};

		if (_leftNode.IsLeaf && _rightNode.IsLeaf)
			return result;

		return result.Concat(_leftNode.GetConnectors().Concat(_rightNode.GetConnectors()));
	}
}

public enum SplitMode
{
	Horizontal,
	Vertical,
}