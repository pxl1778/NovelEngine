using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using System;

public class SpawnPointNode : BaseNode {
    private SpawnPointNodeData nodeData;
    public override BaseNodeData NodeData {
        get { return nodeData; }
        set { nodeData = value is SpawnPointNodeData ? (SpawnPointNodeData)value : null; }
    }

    public static SpawnPointNode CreateNode(DialogueGraphView graphView, Vector2 position, SpawnPointNodeData nodeData = null) {
        var spawnPointNode = new SpawnPointNode {
            title = "Change Scenes",
            NodeData = nodeData == null ? new SpawnPointNodeData() : nodeData,
            GUID = Guid.NewGuid().ToString()
        };
        SpawnPointNodeData spawnPointNodeData = spawnPointNode.NodeData as SpawnPointNodeData;
        if (nodeData == null) {
            graphView.containerCache.SpawnPointNodeDatas.Add(spawnPointNodeData);
            spawnPointNode.NodeData.Guid = spawnPointNode.GUID;
            spawnPointNode.NodeData.Position = position;
        } else {
            spawnPointNode.GUID = nodeData.Guid;
        }
        spawnPointNode.styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/Node"));

        //Input Port
        var inputPort = spawnPointNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        inputPort.portName = "Input";
        spawnPointNode.inputContainer.Add(inputPort);

        //Scene Field
        var sceneLabel = new Label("Next Scene");
        spawnPointNode.mainContainer.Add(sceneLabel);
        var sceneField = new ObjectField();
        sceneField.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
        sceneField.objectType = typeof(SpawnPointSO);
        sceneField.labelElement.style.minWidth = 70;
        sceneField.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoSpawnPoint:" + spawnPointNode.GUID);
            spawnPointNodeData.SpawnPoint = (SpawnPointSO)evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        if (spawnPointNodeData.SpawnPoint != null) {
            sceneField.SetValueWithoutNotify(spawnPointNodeData.SpawnPoint);
        }
        spawnPointNode.mainContainer.Add(sceneField);

        spawnPointNode.mainContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
        spawnPointNode.style.minWidth = 100;
        spawnPointNode.RefreshExpandedState();
        spawnPointNode.RefreshPorts();
        spawnPointNode.SetPosition(new Rect(position, defaultNodeSize));

        return spawnPointNode;
    }
}
