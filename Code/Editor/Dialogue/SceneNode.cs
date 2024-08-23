using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using System;
using System.Linq;
using UnityEditor.Callbacks;

[Serializable]
public class SceneNode : BaseNode {
    private SceneNodeData nodeData;
    public override BaseNodeData NodeData {
        get { return nodeData; }
        set { nodeData = value is SceneNodeData ? (SceneNodeData)value : null; }
    }

    public static SceneNode CreateNode(DialogueGraphView graphView, Vector2 position, SceneNodeData nodeData = null) {
        var sceneNode = new SceneNode {
            title = "Change Scenes",
            NodeData = nodeData == null ? new SceneNodeData() : nodeData,
            GUID = Guid.NewGuid().ToString()
        };
        SceneNodeData sceneNodeData = sceneNode.NodeData as SceneNodeData;
        if (nodeData == null) {
            graphView.containerCache.SceneNodeDatas.Add(sceneNodeData);
            sceneNode.NodeData.Guid = sceneNode.GUID;
            sceneNode.NodeData.Position = position;
        } else {
            sceneNode.GUID = nodeData.Guid;
        }
        sceneNode.styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/Node"));

        //Input Port
        var inputPort = sceneNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        inputPort.portName = "Input";
        sceneNode.inputContainer.Add(inputPort);

        //Scene Field
        var sceneLabel = new Label("Next Scene");
        sceneNode.mainContainer.Add(sceneLabel);
        var sceneField = new ObjectField();
        sceneField.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
        sceneField.objectType = typeof(DialogueContainer);
        sceneField.labelElement.style.minWidth = 70;
        sceneField.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoScene:" + sceneNode.GUID);
            sceneNodeData.NextScene = (DialogueContainer)evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        if (sceneNodeData.NextScene != null) {
            sceneField.SetValueWithoutNotify(sceneNodeData.NextScene);
        }
        sceneNode.mainContainer.Add(sceneField);

        sceneNode.mainContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
        sceneNode.style.minWidth = 100;
        sceneNode.RefreshExpandedState();
        sceneNode.RefreshPorts();
        sceneNode.SetPosition(new Rect(position, defaultNodeSize));

        return sceneNode;
    }
}
