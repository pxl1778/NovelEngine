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
public class CommentNode : BaseNode {
    private CommentNodeData nodeData;
    public override BaseNodeData NodeData {
        get { return nodeData; }
        set { nodeData = value is CommentNodeData ? (CommentNodeData)value : null; }
    }

    public static CommentNode CreateNode(DialogueGraphView graphView, Vector2 position, CommentNodeData nodeData = null) {
        var commentNode = new CommentNode {
            title = "Comment",
            NodeData = nodeData == null ? new CommentNodeData() : nodeData,
            GUID = Guid.NewGuid().ToString()
        };
        CommentNodeData commentNodeData = commentNode.NodeData as CommentNodeData;
        if (nodeData == null) {
            graphView.containerCache.CommentNodeDatas.Add(commentNodeData);
            commentNode.NodeData.Guid = commentNode.GUID;
            commentNode.NodeData.Position = position;
        } else {
            commentNode.GUID = nodeData.Guid;
        }
        commentNode.styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/Node"));

        //Comment Field
        var commentField = new TextField(string.Empty);
        commentField.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoComment:" + commentNode.GUID);
            commentNodeData.Comment = evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        commentField.style.maxHeight = 200;
        commentField.style.maxWidth = 190;
        commentField.style.whiteSpace = WhiteSpace.Normal;
        commentField.style.flexWrap = Wrap.Wrap;
        commentField.Children().FirstOrDefault().style.flexDirection = FlexDirection.Column;
        commentField.SetValueWithoutNotify(commentNodeData.Comment);
        commentNode.mainContainer.Add(commentField);
        commentField.MarkDirtyRepaint();

        commentNode.mainContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
        commentNode.style.minWidth = 200;
        commentNode.titleContainer.style.backgroundColor = new Color(0.4f, 0.4f, 0.6f, 1);
        commentNode.RefreshExpandedState();
        commentNode.SetPosition(new Rect(position, defaultNodeSize));

        return commentNode;
    }
}
