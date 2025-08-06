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
            tree.Add(new SearchTreeEntry(new GUIContent("Image Box")) {
                userData = new ImageBoxProperty(),
                level = 1
            });
        }
        if (_node.nodeData.PropertiesList.Where((property) => property is FlipSpriteProperty).FirstOrDefault() == null) {
            tree.Add(new SearchTreeEntry(new GUIContent("Flip Sprite")) {
                userData = new FlipSpriteProperty(),
                level = 1
            });
        }
        if (_node.nodeData.PropertiesList.Where((property) => property is MoveSpriteProperty).FirstOrDefault() == null) {
            tree.Add(new SearchTreeEntry(new GUIContent("Move Sprite")) {
                userData = new MoveSpriteProperty(),
                level = 1
            });
        }
        if (_node.nodeData.PropertiesList.Where((property) => property is OrderSpriteProperty).FirstOrDefault() == null) {
            tree.Add(new SearchTreeEntry(new GUIContent("Order Sprite")) {
                userData = new OrderSpriteProperty(),
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
        switch (SearchTreeEntry.userData) {
            case ImageBoxProperty:
                Undo.RegisterCompleteObjectUndo(_graphView.containerCache, "NodeUndoAddNewImageBox:" + _node.GUID);
                _node.AddProperty(new ImageBoxFoldout(_node, _graphView));
                EditorUtility.SetDirty(_graphView.containerCache);
                return true;
            case FlipSpriteProperty:
                Undo.RegisterCompleteObjectUndo(_graphView.containerCache, "NodeUndoAddNewFlipSpriteProperty:" + _node.GUID);
                _node.AddProperty(new FlipFoldout(_node, _graphView));
                EditorUtility.SetDirty(_graphView.containerCache);
                return true;
            case MoveSpriteProperty:
                Undo.RegisterCompleteObjectUndo(_graphView.containerCache, "NodeUndoAddNewMoveSpriteProperty:" + _node.GUID);
                _node.AddProperty(new MoveFoldout(_node, _graphView));
                EditorUtility.SetDirty(_graphView.containerCache);
                return true;
            case OrderSpriteProperty:
                Undo.RegisterCompleteObjectUndo(_graphView.containerCache, "NodeUndoAddNewOrderSpriteProperty:" + _node.GUID);
                _node.AddProperty(new OrderFoldout(_node, _graphView));
                EditorUtility.SetDirty(_graphView.containerCache);
                return true;
            default:
                return false;
        }
    }
}
