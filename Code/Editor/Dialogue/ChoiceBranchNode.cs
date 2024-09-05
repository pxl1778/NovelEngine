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


public class ChoiceBranchNode : BaseNode {
    private ChoiceBranchNodeData nodeData;
    public override BaseNodeData NodeData {
        get { return nodeData; }
        set { nodeData = value is ChoiceBranchNodeData ? (ChoiceBranchNodeData)value : null; }
    }

    public static ChoiceBranchNode CreateNode(DialogueGraphView graphView, Vector2 position, ChoiceBranchNodeData nodeData = null, Edge activeEdge = null, Edge inactiveEdge = null) {
        var choiceBranchNode = new ChoiceBranchNode {
            title = "Choice Branch",
            NodeData = nodeData == null ? new ChoiceBranchNodeData() : nodeData,
            GUID = Guid.NewGuid().ToString()
        };
        ChoiceBranchNodeData choiceBranchNodeData = choiceBranchNode.NodeData as ChoiceBranchNodeData;
        if (nodeData == null) {
            graphView.containerCache.ChoiceBranchNodeDatas.Add(choiceBranchNodeData);
            choiceBranchNode.NodeData.Guid = choiceBranchNode.GUID;
            choiceBranchNode.NodeData.Position = position;
        } else {
            choiceBranchNode.GUID = nodeData.Guid;
        }
        choiceBranchNode.styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/Node"));

        //Input Port
        var inputPort = choiceBranchNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        inputPort.portName = "Input";
        choiceBranchNode.inputContainer.Add(inputPort);

        //Output Port
        var activePort = choiceBranchNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        activePort.Children().ToList().ForEach(x => x.pickingMode = PickingMode.Position);
        activePort.portName = "Active";
        if (activeEdge != null) {
            NodeLinkData linkData = graphView.containerCache.NodeLinks.Find(link => link.Guid == activeEdge.viewDataKey);
            activeEdge.output = activePort;
            activePort.Connect(activeEdge);
        }
        choiceBranchNode.outputContainer.Add(activePort);

        var inactivePort = choiceBranchNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        inactivePort.Children().ToList().ForEach(x => x.pickingMode = PickingMode.Position);
        inactivePort.portName = "Inactive";
        if (inactiveEdge != null) {
            NodeLinkData linkData = graphView.containerCache.NodeLinks.Find(link => link.Guid == inactiveEdge.viewDataKey);
            inactiveEdge.output = inactivePort;
            inactivePort.Connect(inactiveEdge);
        }
        choiceBranchNode.outputContainer.Add(inactivePort);

        //Choice Dropdown
        var choiceDropdown = new DropdownField("Choice Key", NovelData.instance.GetChoiceKeys().Where(x => x != "").ToList(), 0);
        choiceDropdown.labelElement.style.minWidth = 20;
        choiceDropdown.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoChoiceDropdown:" + choiceBranchNode.GUID);
            choiceBranchNodeData.ChoiceKey = evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        choiceDropdown.SetValueWithoutNotify(choiceBranchNodeData.ChoiceKey);
        choiceBranchNode.mainContainer.Add(choiceDropdown);

        choiceBranchNode.mainContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
        choiceBranchNode.style.minWidth = 100;
        choiceBranchNode.RefreshExpandedState();
        choiceBranchNode.RefreshPorts();
        choiceBranchNode.SetPosition(new Rect(position, defaultNodeSize));

        return choiceBranchNode;
    }
}
