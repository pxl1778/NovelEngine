using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;

public class PropertySearchWindow : ScriptableObject, ISearchWindowProvider {
    private DialogueGraphView _graphView;
    private EditorWindow _window;
    private DialogueNode _node;

    public void Init(EditorWindow window, DialogueGraphView graphView) {
        _graphView = graphView;
        _window = window;
    }

    public void SetNode(DialogueNode node) {
        _node = node;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
        var tree = new List<SearchTreeEntry> {
            new SearchTreeGroupEntry(new GUIContent("Add Property"), 0)
        };
        if(_node.nodeData.PropertiesList.Where((property) => property is ImageBoxProperty).FirstOrDefault() == null) {
            tree.Add(new SearchTreeEntry(new GUIContent("ImageBox")) {
                userData = new ImageBoxProperty(),
                level = 1
            });
        }
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
            case ImageBoxProperty:
                Undo.RegisterCompleteObjectUndo(_graphView.containerCache, "NodeUndoAddNewImageBox:" + _node.GUID);
                _node.AddProperty(new ImageBoxFoldout(_node, _graphView));
                EditorUtility.SetDirty(_graphView.containerCache);
                return true;
            default:
                return false;
        }
    }
}
