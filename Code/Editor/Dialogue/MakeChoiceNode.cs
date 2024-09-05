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

public class MakeChoiceNode : BaseNode
{
    private MakeChoiceNodeData nodeData;
    public override BaseNodeData NodeData {
        get { return nodeData; }
        set { nodeData = value is MakeChoiceNodeData ? (MakeChoiceNodeData)value : null; }
    }

    public static MakeChoiceNode CreateNode(DialogueGraphView graphView, Vector2 position, MakeChoiceNodeData nodeData = null, Edge outputEdge = null) {
        var makeChoiceNode = new MakeChoiceNode {
            title = "Make Choice",
            NodeData = nodeData == null ? new MakeChoiceNodeData() : nodeData,
            GUID = Guid.NewGuid().ToString()
        };
        MakeChoiceNodeData makeChoiceNodeData = makeChoiceNode.NodeData as MakeChoiceNodeData;
        if (nodeData == null) {
            graphView.containerCache.MakeChoiceNodeDatas.Add(makeChoiceNodeData);
            makeChoiceNode.NodeData.Guid = makeChoiceNode.GUID;
            makeChoiceNode.NodeData.Position = position;
        } else {
            makeChoiceNode.GUID = nodeData.Guid;
        }
        makeChoiceNode.styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/Node"));

        //Input Port
        var inputPort = makeChoiceNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        inputPort.portName = "Input";
        makeChoiceNode.inputContainer.Add(inputPort);

        //Output Port
        var outputPort = makeChoiceNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        outputPort.Children().ToList().ForEach(x => x.pickingMode = PickingMode.Position);
        var oldLabel = outputPort.contentContainer.Q<Label>("type");
        outputPort.contentContainer.Remove(oldLabel);
        if (outputEdge != null) {
            NodeLinkData linkData = graphView.containerCache.NodeLinks.Find(link => link.Guid == outputEdge.viewDataKey);
            outputEdge.output = outputPort;
            outputPort.Connect(outputEdge);
        }
        makeChoiceNode.outputContainer.Add(outputPort);

        //Choice Dropdown
        var choiceDropdown = new DropdownField("Choice Key", NovelData.instance.GetChoiceKeys().Where(x => x != "").ToList(), 0);
        choiceDropdown.labelElement.style.minWidth = 20;
        choiceDropdown.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoChoiceDropdown:" + makeChoiceNode.GUID);
            makeChoiceNodeData.ChoiceKey = evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        choiceDropdown.SetValueWithoutNotify(makeChoiceNodeData.ChoiceKey);
        makeChoiceNode.mainContainer.Add(choiceDropdown);

        //Active radio buttons
        var radioGroup = new RadioButtonGroup("State", new List<string> { "Active", "Inactive" });
        radioGroup.RegisterValueChangedCallback(evt => {
            if(evt.newValue < 0) { return; }
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoChoiceActive:" + makeChoiceNode.GUID);
            makeChoiceNodeData.Activate = evt.newValue == 0;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        radioGroup.SetValueWithoutNotify(makeChoiceNodeData.Activate == true ? 0 : 1);
        makeChoiceNode.mainContainer.Add(radioGroup);

        makeChoiceNode.mainContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
        makeChoiceNode.style.minWidth = 100;
        makeChoiceNode.RefreshExpandedState();
        makeChoiceNode.RefreshPorts();
        makeChoiceNode.SetPosition(new Rect(position, defaultNodeSize));

        return makeChoiceNode;
    }
}
