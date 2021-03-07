using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonCreator))]
public class DungeonCreatorInspector : Editor
{
	private DungeonCreator _dungeonCreator;

	private GameObject _prefabFloorTile;
	

	private void OnEnable()
	{
		_dungeonCreator = (DungeonCreator) target;
	}
	
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		EditorGUILayout.Separator();

		var buttonGenerate = GUILayout.Button("Generate");
		if (buttonGenerate)
		{
			_dungeonCreator.GenerateDungeon();
		}
		
		var buttonDelete = GUILayout.Button("Delete");
		if (buttonDelete)
		{
			_dungeonCreator.DeleteDungeon();
		}
	}
}
