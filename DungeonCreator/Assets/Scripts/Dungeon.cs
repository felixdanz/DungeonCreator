using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dungeon
{
	private readonly System.Random _rng;
	
	private Vector2Int _startPosition;
	private Vector2Int _endPosition;

	private Vector2Int _minSize;
	
	private Dungeon _leftLeaf;
	private Dungeon _rightLeaf;


	public Dungeon(System.Random rng, Vector2Int startPosition, Vector2Int endPosition, Vector2Int minSize)
	{
		_rng = rng;
		_startPosition = startPosition;
		_endPosition = endPosition;
		_minSize = minSize;

		GenerateLeaves();
	}

	private void GenerateLeaves()
	{
		var splitMode = (SplitMode) _rng.Next(0, 2);

		var (splitStart, splitEnd) = splitMode == SplitMode.Horizontal
			? (_startPosition.y + _minSize.y + 1, _endPosition.y - _minSize.y - 1)
			: (_startPosition.x + _minSize.x + 1, _endPosition.x - _minSize.x - 1);

		if (splitEnd - splitStart <= 1)
			return;

		var splitPosition = _rng.Next(splitStart + 1, splitEnd);
		
		Vector2Int startPositionLeftLeaf;
		Vector2Int startPositionRightLeaf;
		
		Vector2Int endPositionLeftLeaf;
		Vector2Int endPositionRightLeaf;

		if (splitMode == SplitMode.Horizontal)
		{
			startPositionLeftLeaf = new Vector2Int(_startPosition.x, _startPosition.y);
			startPositionRightLeaf = new Vector2Int(_startPosition.x, splitPosition + 1);
			
			endPositionLeftLeaf = new Vector2Int(_endPosition.x, splitPosition - 1);
			endPositionRightLeaf = new Vector2Int(_endPosition.x, _endPosition.y);
		}
		else
		{
			startPositionLeftLeaf = new Vector2Int(_startPosition.x, _startPosition.y);
			startPositionRightLeaf = new Vector2Int(splitPosition + 1, _startPosition.y);
			
			endPositionLeftLeaf = new Vector2Int(splitPosition - 1, _endPosition.y);
			endPositionRightLeaf = new Vector2Int(_endPosition.x, _endPosition.y);
		}
		
		_leftLeaf = new Dungeon(_rng, startPositionLeftLeaf, endPositionLeftLeaf, _minSize);
		_rightLeaf = new Dungeon(_rng, startPositionRightLeaf, endPositionRightLeaf, _minSize);
	}
	
	public IEnumerable<(Vector2Int, Vector2Int)> GetRooms()
	{
		if (_leftLeaf == null || _rightLeaf == null)
			return new List<(Vector2Int, Vector2Int)>() {(_startPosition, _endPosition)};
		
		return _leftLeaf.GetRooms().Concat(_rightLeaf.GetRooms());
	}
}

public enum SplitMode
{
	Horizontal,
	Vertical,
}