namespace DungeonCreator
{
	public class CellularAutomata
	{
		public readonly int[] Map;
		
		private readonly int _width;
		private readonly int _height;
		
		
		public CellularAutomata(
			int[] map, 
			int width, 
			int height, 
			int cycles,
			int lowerSurvivalBound)
		{
			Map = map;
			_width = width;
			_height = height;
			
			for (var i = 0; i < cycles; i++)
			{
				RunCycle(lowerSurvivalBound);
			}
		}

		private void RunCycle(int lowerSurvivalBound)
		{
			for (var i = 0; i < Map.Length; i++)
			{
				var liveNeighbourSum = GetLiveNeighbourSum(i);

				if (liveNeighbourSum > lowerSurvivalBound) { Map[i] = 1; }
				if (liveNeighbourSum < lowerSurvivalBound) { Map[i] = 0; }
			}
		}

		private int GetLiveNeighbourSum(int index)
		{
			var neighboursIndices = new []
			{
				index - 1,				// left
				index + _width - 1,		// up left
				index + _width,			// up
				index + _width + 1,		// up right
				index + 1,				// right
				index - _width + 1,		// down right
				index - _width,			// down
				index - _width - 1,		// down left
			};

			var result = 0;
			
			foreach (var neighbourIndex in neighboursIndices)
			{
				if (!IndexIsValid(neighbourIndex))
					continue;

				result += Map[neighbourIndex];
			}

			return result;
		}

		private bool IndexIsValid(int index)
		{
			return index >= 0 && index < Map.Length;
		}
	}
}