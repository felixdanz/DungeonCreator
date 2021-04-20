using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonCreator
{
	public class BSPDungeon
	{
		private readonly Random _rng;
		
		private readonly Vector2Int _sectorStart;
		private readonly Vector2Int _sectorEnd;
		
		private readonly Vector2Int _minSectorSize;
		private readonly Vector2Int _minRoomSize;

		private readonly int _minWidthConnector;
		private readonly int _maxWidthConnector;

		private RoomData _roomData;
		private ConnectorData _connectorData;
		
		private BSPDungeon _leftNode;
		private BSPDungeon _rightNode;

		private bool IsLeaf => _leftNode == null || _rightNode == null;
		
		
		public BSPDungeon(
			int seed,
			(int, int) sectorStart, 
			(int, int) sectorEnd, 
			(int, int) minSectorSize,
			(int, int) minRoomSize,
			int minWidthConnector,
			int maxWidthConnector,
			int maxSplitDepth)
		{
			_rng = new Random(seed);
			_sectorStart = new Vector2Int(sectorStart);
			_sectorEnd = new Vector2Int(sectorEnd);
			_minSectorSize = new Vector2Int(minSectorSize);
			_minRoomSize = new Vector2Int(minRoomSize);
			_minWidthConnector = minWidthConnector;
			_maxWidthConnector = maxWidthConnector;
			
			var splitMode = _rng.Next(0, 2) == 0 
				? SplitMode.Vertical
				: SplitMode.Horizontal;
			
			GenerateSectors(maxSplitDepth, 0, splitMode);
		}
		
		private BSPDungeon(
			Random rng, 
			Vector2Int sectorStart, 
			Vector2Int sectorEnd, 
			Vector2Int minSectorSize,
			Vector2Int minRoomSize,
			int minWidthConnector,
			int maxWidthConnector,
			int maxSplitDepth,
			int currentSplitDepth,
			SplitMode splitMode)
		{
			_rng = rng;
			_sectorStart = sectorStart;
			_sectorEnd = sectorEnd;
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
				? (_sectorStart.Y + _minSectorSize.Y + 1, _sectorEnd.Y - _minSectorSize.Y - 1)
				: (_sectorStart.X + _minSectorSize.X + 1, _sectorEnd.X - _minSectorSize.X - 1);

			if (splitEnd - splitStart <= 1)
				return;
			
			var splitPosition = _rng.Next(splitStart + 1, splitEnd);
			
			Vector2Int startVector2IntLeftLeaf;
			Vector2Int startVector2IntRightLeaf;
			
			Vector2Int endVector2IntLeftLeaf;
			Vector2Int endVector2IntRightLeaf;
			
			if (splitMode == SplitMode.Horizontal)
			{
				startVector2IntLeftLeaf = new Vector2Int(_sectorStart.X, _sectorStart.Y);
				startVector2IntRightLeaf = new Vector2Int(_sectorStart.X, splitPosition + 1);
				
				endVector2IntLeftLeaf = new Vector2Int(_sectorEnd.X, splitPosition - 1);
				endVector2IntRightLeaf = new Vector2Int(_sectorEnd.X, _sectorEnd.Y);
			}
			else
			{
				startVector2IntLeftLeaf = new Vector2Int(_sectorStart.X, _sectorStart.Y);
				startVector2IntRightLeaf = new Vector2Int(splitPosition + 1, _sectorStart.Y);
				
				endVector2IntLeftLeaf = new Vector2Int(splitPosition - 1, _sectorEnd.Y);
				endVector2IntRightLeaf = new Vector2Int(_sectorEnd.X, _sectorEnd.Y);
			}
			
			_leftNode = new BSPDungeon(
				_rng, 
				startVector2IntLeftLeaf, 
				endVector2IntLeftLeaf, 
				_minSectorSize,
				_minRoomSize,
				_minWidthConnector,
				_maxWidthConnector,
				maxSplitDepth, 
				currentSplitDepth, 
				splitMode);
			
			_rightNode = new BSPDungeon(
				_rng, 
				startVector2IntRightLeaf, 
				endVector2IntRightLeaf, 
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
				var endReducedByMin = _sectorEnd - _minRoomSize;
				
				var newStartX = _rng.Next(_sectorStart.X, endReducedByMin.X);
				var newStartY = _rng.Next(_sectorStart.Y, endReducedByMin.Y);
				
				var newXLength = _sectorEnd.X - newStartX;
				var newYLength = _sectorEnd.Y - newStartY;
				
				_roomData.StartVector2Int.X = _rng.Next(_sectorStart.X, _sectorEnd.X - newXLength);
				_roomData.StartVector2Int.Y = _rng.Next(_sectorStart.Y, _sectorEnd.Y - newYLength);
				
				_roomData.EndVector2Int.X = _roomData.StartVector2Int.X + newXLength;
				_roomData.EndVector2Int.Y = _roomData.StartVector2Int.Y + newYLength;
				
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

			_connectorData.StartVector2IntFrom = new Vector2Int()
			{
				X = _rng.Next(
					_leftNode._roomData.StartVector2Int.X + 1,
					_leftNode._roomData.EndVector2Int.X - connectorWidth + 1),
				Y = _rng.Next(
					_leftNode._roomData.StartVector2Int.Y + 1,
					_leftNode._roomData.EndVector2Int.Y - connectorWidth + 1),
			};
			
			_connectorData.EndVector2IntFrom = new Vector2Int()
			{
				X = _rng.Next(
					_rightNode._roomData.StartVector2Int.X + 1,
					_rightNode._roomData.EndVector2Int.X - connectorWidth + 1),
				Y = _rng.Next(
					_rightNode._roomData.StartVector2Int.Y + 1,
					_rightNode._roomData.EndVector2Int.Y - connectorWidth + 1),
			};


			if (connectorVerticalFirst)
			{
				_connectorData.StartVector2IntTo.X = _connectorData.StartVector2IntFrom.X + connectorWidth - 1;
				_connectorData.StartVector2IntTo.Y = _connectorData.StartVector2IntFrom.Y;
				
				_connectorData.EndVector2IntTo.X = _connectorData.EndVector2IntFrom.X;
				_connectorData.EndVector2IntTo.Y = _connectorData.EndVector2IntFrom.Y + connectorWidth - 1;
				
				_connectorData.CenterVector2IntFrom.X = _connectorData.StartVector2IntFrom.X;
				_connectorData.CenterVector2IntFrom.Y = _connectorData.EndVector2IntFrom.Y;

				_connectorData.CenterVector2IntTo.X = _connectorData.StartVector2IntTo.X;
				_connectorData.CenterVector2IntTo.Y = _connectorData.EndVector2IntTo.Y;
			}
			else
			{
				_connectorData.StartVector2IntTo.Y  = _connectorData.StartVector2IntFrom.Y + connectorWidth - 1;
				_connectorData.StartVector2IntTo.X = _connectorData.StartVector2IntFrom.X;
				
				_connectorData.EndVector2IntTo.Y = _connectorData.EndVector2IntFrom.Y;
				_connectorData.EndVector2IntTo.X = _connectorData.EndVector2IntFrom.X + connectorWidth - 1;
				
				_connectorData.CenterVector2IntFrom.Y = _connectorData.StartVector2IntFrom.Y;
				_connectorData.CenterVector2IntFrom.X = _connectorData.EndVector2IntFrom.X;
				
				_connectorData.CenterVector2IntTo.Y = _connectorData.StartVector2IntTo.Y;
				_connectorData.CenterVector2IntTo.X = _connectorData.EndVector2IntTo.X;
			}
		}

		private void CreateConnectorBetweenSectors()
		{
			var connectorVerticalFirst = Convert.ToBoolean(_rng.Next(0, 2));
			
			_connectorData.StartVector2IntFrom = _leftNode._connectorData.CenterVector2IntFrom;
			_connectorData.StartVector2IntTo = _leftNode._connectorData.CenterVector2IntTo;
			
			_connectorData.EndVector2IntFrom = _rightNode._connectorData.CenterVector2IntFrom;
			_connectorData.EndVector2IntTo = _rightNode._connectorData.CenterVector2IntTo;
			
			if (connectorVerticalFirst)
			{
				_connectorData.CenterVector2IntFrom.X = _connectorData.StartVector2IntFrom.X;
				_connectorData.CenterVector2IntFrom.Y = _connectorData.EndVector2IntFrom.Y;
			
				_connectorData.CenterVector2IntTo.X = _connectorData.StartVector2IntTo.X;
				_connectorData.CenterVector2IntTo.Y = _connectorData.EndVector2IntTo.Y;
			}
			else
			{
				_connectorData.CenterVector2IntFrom.Y = _connectorData.StartVector2IntFrom.Y;
				_connectorData.CenterVector2IntFrom.X = _connectorData.EndVector2IntFrom.X;
				
				_connectorData.CenterVector2IntTo.Y = _connectorData.StartVector2IntTo.Y;
				_connectorData.CenterVector2IntTo.X = _connectorData.EndVector2IntTo.X;
			}
		}

		private IEnumerable<RoomData> GetRooms()
		{
			if (_leftNode == null || _rightNode == null)
				return new List<RoomData>() { _roomData };
			
			return _leftNode.GetRooms().Concat(_rightNode.GetRooms());
		}
		
		private IEnumerable<ConnectorData> GetConnectors()
		{
			if (IsLeaf)
				return new List<ConnectorData>();

			var result = new List<ConnectorData>() { _connectorData };

			if (_leftNode.IsLeaf && _rightNode.IsLeaf)
				return result;

			return result.Concat(_leftNode.GetConnectors().Concat(_rightNode.GetConnectors()));
		}

		public int[] ToIntArray()
		{
			var width = _sectorEnd.X;
			var height = _sectorEnd.Y;
			var array = new int[width * height];

			var rooms = GetRooms();
			var connectors = GetConnectors();

			foreach (var roomData in rooms)
			{
				for (var z = roomData.StartVector2Int.Y; z < roomData.EndVector2Int.Y; z++)
				for (var x = roomData.StartVector2Int.X; x < roomData.EndVector2Int.X; x++)
				{
					var index = z * width + x;
					array[index] = 1;
				}
			}

			foreach (var connectorData in connectors)
			{
				var (startIndexZ, targetIndexZ) = 
					connectorData.StartVector2IntFrom.Y <= connectorData.CenterVector2IntTo.Y 
						? (connectorData.StartVector2IntFrom.Y, connectorData.CenterVector2IntTo.Y)
						: (connectorData.CenterVector2IntTo.Y, connectorData.StartVector2IntFrom.Y);
			
				var (startIndexX, targetIndexX) = 
					connectorData.StartVector2IntFrom.X <= connectorData.CenterVector2IntTo.X
						? (connectorData.StartVector2IntFrom.X, connectorData.CenterVector2IntTo.X)
						: (connectorData.CenterVector2IntTo.X, connectorData.StartVector2IntFrom.X);
			
				for (var z = startIndexZ; z <= targetIndexZ; z++)
				for (var x = startIndexX; x <= targetIndexX; x++)
				{
					var index = z * width + x;

					if (array[index] == 0)
					{
						array[index] = 2;
					}
				}
			
				(startIndexZ, targetIndexZ) = 
					connectorData.CenterVector2IntFrom.Y <= connectorData.EndVector2IntTo.Y
						? (connectorData.CenterVector2IntFrom.Y, connectorData.EndVector2IntTo.Y)
						: (connectorData.EndVector2IntTo.Y, connectorData.CenterVector2IntFrom.Y);
			
				(startIndexX, targetIndexX) = 
					connectorData.CenterVector2IntFrom.X <= connectorData.EndVector2IntTo.X
						? (connectorData.CenterVector2IntFrom.X, connectorData.EndVector2IntTo.X)
						: (connectorData.EndVector2IntTo.X, connectorData.CenterVector2IntFrom.X);

				for (var z = startIndexZ; z <= targetIndexZ; z++)
				for (var x = startIndexX; x <= targetIndexX; x++)
				{
					var index = z * width + x;
					
					if (array[index] == 0)
					{
						array[index] = 2;
					}
				}
			}

			return array;
		}
	}
	
	internal struct RoomData
	{
		public Vector2Int StartVector2Int;
		public Vector2Int EndVector2Int;
	}
	
	internal struct ConnectorData
	{
		public Vector2Int StartVector2IntFrom;
		public Vector2Int StartVector2IntTo;
		
		public Vector2Int CenterVector2IntFrom;
		public Vector2Int CenterVector2IntTo;
		
		public Vector2Int EndVector2IntFrom;
		public Vector2Int EndVector2IntTo;
	}
	
	internal struct Vector2Int
	{
		public int X;
		public int Y;
		
		public Vector2Int((int x, int y) _)
		{
			X = _.x;
			Y = _.y;
		}
		
		public Vector2Int(int x, int y)
		{
			X = x;
			Y = y;
		}
		
		public static Vector2Int operator -(Vector2Int a, Vector2Int b)
		{
			return new Vector2Int(a.X - b.X, a.Y - b.Y);
		}
	}

	internal enum SplitMode
	{
		Horizontal,
		Vertical,
	}
}