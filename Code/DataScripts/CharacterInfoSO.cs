using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "CharacterInfoSO", menuName = "ScriptableObjects/CharacterInfoSO")]
public class CharacterInfoSO : ScriptableObject {

    [field: SerializeField] public string Id { get; private set; }
    [field: SerializeField] public string LoadPath { get; private set; }
    public string characterName;
    public AnimatedSprite characterPrefab;
    public Color color = new Color(0, 0, 0, 1.0f);
    public Sprite customCursor;
    public Sprite customCursorFrame;

    private void OnValidate() {
#if UNITY_EDITOR
        Id = this.name;
        string[] assetPaths = AssetDatabase.FindAssets(this.name);
        string fullPath = AssetDatabase.GUIDToAssetPath(assetPaths[0]);
        LoadPath = fullPath.Replace("Assets/Resources/", "").Replace(".asset", "");
        EditorUtility.SetDirty(this);
#endif
    }
}

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and prefab overrides.
//[CustomEditor(typeof(CharacterInfoSO))]
//[CanEditMultipleObjects]
//public class NovelDataEditor : Editor {

//    SerializedProperty characterProperty;
//    string[] _choices = new[] { "Null" };
//    int _choiceIndex = 0;

//    void OnEnable() {
//        _choices = NovelDataSO.instance.CharacterList.Keys.ToArray();
//        // Setup the SerializedProperties.
//        characterProperty = serializedObject.FindProperty("character");
//        // Set the choice index to the previously selected index
//        _choiceIndex = Array.IndexOf(_choices, characterProperty.stringValue);

//    }

//    public override void OnInspectorGUI() {
//        DrawDefaultInspector();
//        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
//        serializedObject.Update();

//        //doing the orientation thing
//        _choiceIndex = EditorGUILayout.Popup("Character", _choiceIndex, _choices);
//        if (_choiceIndex < 0) {
//            _choiceIndex = 0;
//        }
//        if(_choices.Length > 0) {
//            characterProperty.stringValue = _choices[_choiceIndex];
//        }



//        // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
//        serializedObject.ApplyModifiedProperties();
//    }
//}
