using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BSPDungeonCreator))]
public class BSPDungeonCreatorInspector : Editor
{
	private BSPDungeonCreator _dungeonCreator;

	private GameObject _prefabFloorTile;
	

	private void OnEnable()
	{
		_dungeonCreator = (BSPDungeonCreator) target;
	}
	
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
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
