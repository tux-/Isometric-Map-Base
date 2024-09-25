using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IsoTiles))]
public class IsoTilesEditor : Editor
{
	int toolbarInt = 0;
	string[] toolbarStrings = {"None", "Raise", "Lower", "Flatten"};

	IsoTiles terrain;

	public override void OnInspectorGUI ()
	{
		EditorGUILayout.LabelField("Level", "Terrain");

		DrawDefaultInspector();

		IsoTiles isoTiles = (IsoTiles) target;

		EditorGUILayout.HelpBox("Always regenerate after size change, or there will be bugs!", MessageType.Info);

		if (GUILayout.Button("Generate Terrain")) {
			isoTiles.GenerateTerrain(isoTiles.seed, isoTiles.hillyness);
		}

		toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings);
	}

	private void OnEnable ()
	{
		terrain = (IsoTiles) target;
		Tools.hidden = true;
	}

	private void OnDisable ()
	{
		Tools.hidden = false;
	}

	private void OnSceneGUI ()
	{

		if (toolbarInt > 0) {

			Event guiEvent = Event.current;
			Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);

			int nearest_idx = -1;
			float nearest_dist = float.PositiveInfinity;
			float d1 = 0, d2 = 0;
			for (int i = 0; i < terrain.vertices.Length; ++i) {
				d1 = Vector3.Distance(terrain.vertices[i], mouseRay.origin) / 2f;
				d2 = Vector3.Cross(mouseRay.direction, terrain.vertices[i] - mouseRay.origin).magnitude;

				float di = d1 + d2;

				if (di < nearest_dist) {
					nearest_idx = i;
					nearest_dist = di;
				}
			}

			Handles.SphereHandleCap(0, terrain.vertices[nearest_idx], Quaternion.LookRotation(Vector3.right), 0.15f, EventType.Repaint);

			if ((guiEvent.type == EventType.MouseDown) && (guiEvent.button == 0)) {
				if (toolbarInt == 1) {
					terrain.Raise(nearest_idx);
				}
				else if (toolbarInt == 2) {
					terrain.Lower(nearest_idx);
				}
			}

			if (guiEvent.type == EventType.Layout) {
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			}
		}




		EditorGUI.BeginChangeCheck();

		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(terrain, "Terrain update");


			terrain.UpdadeVisuals();

		}
	}
}
