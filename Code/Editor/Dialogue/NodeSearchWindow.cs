using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
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
            new SearchTreeEntry(new GUIContent("Dialogue Nodes...")){
                userData = "MultiNode",
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
            case "MultiNode":
                CreateMultipleNodesWindow.ShowWindow(this, localMousePosition);
                return true;
            default:
                return false;
        }
    }

    public void CreateMultipleNodes(int amount, Vector2 localMousePosition) {
        Undo.RegisterCompleteObjectUndo(_graphView.containerCache, "NodeUndoAddNewMulti");
        DialogueNode prevNode = null;
        for(int i=0; i<amount; i++) {
            var position = localMousePosition + new Vector2(i * 280, 0);
            DialogueNode node = _graphView.CreateDialogueNode("Dialogue Node", position);
            if(prevNode != null) {
                // get first output port from previous node and input port from current node
                Port outputPort = prevNode.outputContainer.childCount > 0 ? prevNode.outputContainer[0] as Port : null;
                Port inputPort = node.inputContainer.childCount > 0 ? node.inputContainer[0] as Port : null;

                if (outputPort != null && inputPort != null) {
                    // create the edge and connect ports
                    var edge = new Edge {
                        output = outputPort,
                        input = inputPort
                    };
                    outputPort.Connect(edge);
                    inputPort.Connect(edge);

                    // give the edge a unique id and add it to the graph view
                    edge.viewDataKey = Guid.NewGuid().ToString();
                    _graphView.AddElement(edge);

                    _graphView.containerCache.NodeLinks.Add(new NodeLinkData {
                        BaseNodeGuid = ((BaseNode)outputPort.node).GUID,
                        PortName = outputPort.portName,
                        Guid = edge.viewDataKey,
                        TargetNodeGuid = ((BaseNode)inputPort.node).GUID
                    });
                }
            }
            prevNode = node;
        }
        EditorUtility.SetDirty(_graphView.containerCache);
    }
}

public class CreateMultipleNodesWindow : EditorWindow {
    int amount = 3;
    NodeSearchWindow owner;
    UnityEngine.Vector2 localMousePosition;

    public static void ShowWindow(NodeSearchWindow owner, Vector2 localMousePosition) {
        var w = CreateInstance<CreateMultipleNodesWindow>();
        w.titleContent = new GUIContent("Create Multiple Nodes");
        w.owner = owner;
        w.localMousePosition = localMousePosition;
        w.position = new Rect(Screen.width / 2f, Screen.height / 2f, 220, 80);
        w.ShowUtility();
    }

    void OnGUI() {
        EditorGUILayout.LabelField("Number of nodes to create:", EditorStyles.boldLabel);
        amount = EditorGUILayout.IntField(amount);
        if (amount < 1) amount = 1;

        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope()) {
            if (GUILayout.Button("Create")) {
                owner?.CreateMultipleNodes(amount, localMousePosition);
                Close();
            }
            if (GUILayout.Button("Cancel")) {
                Close();
            }
        }
    }
}
