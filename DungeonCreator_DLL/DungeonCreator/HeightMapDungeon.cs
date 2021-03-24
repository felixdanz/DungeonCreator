namespace DungeonCreator
{
	public class HeightMapDungeon
	{
		private readonly int _width;
		private readonly int _height;

		private readonly float _requiredHeightForFloor;

		public readonly float[] HeightMap;
		
		
		public HeightMapDungeon(
			int width, 
			int height, 
			int seed, 
			float scale,
			int octaves,
			float persistence,
			float lacunarity,
			(int x, int y) offset,
			float requiredHeightForFloor)
		{
			_width = width;
			_height = height;
			
			_requiredHeightForFloor = requiredHeightForFloor;

			HeightMap = PerlinNoise.GenerateNoiseMap(
				_width,
				_height,
				seed,
				scale,
				octaves,
				persistence,
				lacunarity,
				offset);
		}

		public int[] ToIntArray()
		{
			var intArray = new int[_width * _height];
			
			for (var z = 0; z < _height; z++)
			for (var x = 0; x < _width; x++)
			{
				var index = z * _width + x;
				var heightValue = HeightMap[index];

				intArray[index] = heightValue >= _requiredHeightForFloor
					? 1
					: 0;
			}

			return intArray;
		}
	}
}