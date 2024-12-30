using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexGridMeshGenerator))]

public class HexGridGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HexGridMeshGenerator hexGridMeshGenerator = (HexGridMeshGenerator)target;

        if (GUILayout.Button("Generate Hex Grid"))
        {
            hexGridMeshGenerator.CreateHexMesh();
        }
        if (GUILayout.Button("Clear Hex Grid"))
        {
            hexGridMeshGenerator.ClearHexGridMesh();
        }

    }
}
