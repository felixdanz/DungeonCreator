using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CaveDungeonCreator))]
public class CaveDungeonCreatorInspector : Editor
{
	private CaveDungeonCreator _dungeonCreator;

	private GameObject _prefabFloorTile;
	

	private void OnEnable()
	{
		_dungeonCreator = (CaveDungeonCreator) target;
	}
	
	public override void OnInspectorGUI()
	{
		if (DrawDefaultInspector() && _dungeonCreator.updateOnChange)
		{
			_dungeonCreator.CreateDungeon();
		}
		
		EditorGUILayout.Separator();

		var buttonGenerate = GUILayout.Button("Generate");
		if (buttonGenerate)
		{
			_dungeonCreator.CreateDungeon();
		}
		
		var buttonDelete = GUILayout.Button("Delete");
		if (buttonDelete)
		{
			_dungeonCreator.DeleteDungeon();
		}
	}
}
