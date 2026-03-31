// Copyright (c) 2026 Keshav Prasad Neupane (Kope)
// Licensed under the MIT License. See LICENSE in the repository root for details.


#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(CreateCheckerBoardFlooring))]
public class CreateCheckerBoardFlooringEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		CreateCheckerBoardFlooring script = (CreateCheckerBoardFlooring)target;
		if (GUILayout.Button("Create Checkerboard Flooring")) {
			script.CreateFlooring();
		}
	}
}
#endif