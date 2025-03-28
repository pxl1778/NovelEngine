using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider {
    private DialogueGraphView _graphView;
    private EditorWindow _window;

    public void Init(EditorWindow window, DialogueGraphView graphView) {
        _graphView = graphView;
        _window = window;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
        var tree = new List<SearchTreeEntry> {
            new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0),
            new SearchTreeEntry(new GUIContent("Dialogue Node")){
                userData = new DialogueNode(),
                level = 1
            },
            new SearchTreeEntry(new GUIContent("Scene Node")){
                userData = new SceneNode(),
                level = 1
            },
            new SearchTreeEntry(new GUIContent("Make Choice Node")){
                userData = new MakeChoiceNode(),
                level = 1
            },
            new SearchTreeEntry(new GUIContent("Choice Branch Node")){
                userData = new ChoiceBranchNode(),
                level = 1
            },
            new SearchTreeEntry(new GUIContent("Spawn Point Node")){
                userData = new SpawnPointNode(),
                level = 1
            },
            new SearchTreeEntry(new GUIContent("Comment Node")){
                userData = new CommentNode(),
                level = 1
            },
        };
        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context) {
        if (_graphView.containerCache == null) {
            var saveUtility = GraphSaveUtility.GetInstance(_graphView);
            saveUtility.SaveGraph();
            return false;
        }
        var worldMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, context.screenMousePosition - _window.position.position);
        var localMousePosition = _graphView.contentViewContainer.WorldToLocal(worldMousePosition);
        switch (SearchTreeEntry.userData) {
            case DialogueNode:
                Undo.RegisterCompleteObjectUndo(_graphView.containerCache, "NodeUndoAddNew");
                _graphView.CreateDialogueNode("Dialogue Node", localMousePosition);
                EditorUtility.SetDirty(_graphView.containerCache);
                return true;
            case SceneNode:
                Undo.RegisterCompleteObjectUndo(_graphView.containerCache, "NodeUndoAddNew");
                _graphView.CreateSceneNode(localMousePosition);
                EditorUtility.SetDirty(_graphView.containerCache);
                return true;
            case MakeChoiceNode:
                Undo.RegisterCompleteObjectUndo(_graphView.containerCache, "NodeUndoAddNew");
                _graphView.CreateMakeChoiceNode(localMousePosition);
                EditorUtility.SetDirty(_graphView.containerCache);
                return true;
            case ChoiceBranchNode:
                Undo.RegisterCompleteObjectUndo(_graphView.containerCache, "NodeUndoAddNew");
                _graphView.CreateChoiceBranchNode(localMousePosition);
                EditorUtility.SetDirty(_graphView.containerCache);
                return true;
            case SpawnPointNode:
                Undo.RegisterCompleteObjectUndo(_graphView.containerCache, "NodeUndoAddNew");
                _graphView.CreateSpawnPointNode(localMousePosition);
                EditorUtility.SetDirty(_graphView.containerCache);
                return true;
            case CommentNode:
                Undo.RegisterCompleteObjectUndo(_graphView.containerCache, "NodeUndoAddNew");
                _graphView.CreateCommentNode(localMousePosition);
                EditorUtility.SetDirty(_graphView.containerCache);
                return true;
            default:
                return false;
        }
    }
}
