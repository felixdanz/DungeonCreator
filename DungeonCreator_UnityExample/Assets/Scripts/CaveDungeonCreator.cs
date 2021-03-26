using DungeonCreator;
using UnityEngine;

public class CaveDungeonCreator : MonoBehaviour
{
	[Header("Visualization")] 
	
	[SerializeField] private GameObject tileFloorRoomPrefab;
	[SerializeField] private GameObject tileFloorConnectorPrefab;
	
	[Header("Configuration")] 
	
	[SerializeField] private int seed = 123456;
	
	[SerializeField] private int width = 100;
	[SerializeField] private int height = 100;
	[SerializeField] private float scale = 0.3f;
	[SerializeField] private int octaves = 4;
	[SerializeField] private float persistence = 0.5f;
	[SerializeField] private float lacunarity = 1.0f;
	[SerializeField] private int offsetX = 1234;
	[SerializeField] private int offsetY = 4321;
	[SerializeField] private float requiredHeightForFloor = 0.5f;
	
	[SerializeField] private int cycles = 4;
	[SerializeField] private int lowerSurvivalBound = 4;

	public bool updateOnChange;
	
	private HeightMap _heightMap;
	private CellularAutomata _cellularAutomata;
	
	private GameObject _visualizedDungeon;


	public void CreateDungeon()
	{
		_heightMap = new HeightMap(
			width,
			height,
			seed,
			scale,
			octaves,
			persistence,
			lacunarity,
			(offsetX, offsetY),
			requiredHeightForFloor);
		
		var dungeon = _heightMap.ToIntArray();
		GenerateBorder(dungeon, 2);
		
		_cellularAutomata = new CellularAutomata(
			dungeon,
			width,
			height,
			cycles,
			lowerSurvivalBound);

		dungeon = InvertMap(_cellularAutomata.Map);

		VisualizeDungeon(
			dungeon,
			width,
			height);
	}
	
    private void GenerateBorder(int[] map, int thickness)
    {
        for (var i = 0; i < map.Length; i++)
        {
	        var x = i % width;
	        var z = i / width;
	        
	        if (x < thickness || x >= width - thickness ||	// left + right border
	            z < thickness || z >= height - thickness)	// bot + top border
            {
                map[i] = 1;
            }
        }
    }

    private int[] InvertMap(int[] map)
    {
	    var result = new int[map.Length];
	    
	    for (var i = 0; i < map.Length; i++)
	    {
		    result[i] = map[i] == 0 ? 1 : 0;
	    }

	    return result;
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
		}
	}

	private void OnValidate()
	{
		if (width < 1)
			width = 1;

		if (height < 1)
			height = 1;

		if (octaves < 1)
			octaves = 1;

		if (persistence < 0.0f)
			persistence = 0.0f;

		if (persistence > 1.0f)
			persistence = 1.0f;

		if (lacunarity < 1.0f)
			lacunarity = 1.0f;
	}
}