using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Linq;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;

public class ImageBoxFoldout : Foldout {
    public DialogueGraphView graphView;

    public ImageBoxFoldout(DialogueNode dialogueNode, DialogueGraphView graphView, ImageBoxProperty property = null) {
        this.graphView = graphView;
        text = "Image Box";

        style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
        style.borderTopWidth = 1;
        style.borderTopColor = new Color(0, 0, 0, 1);

        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;

        if(property == null) {
            property = new ImageBoxProperty();
            nodeData.PropertiesList.Add(property);
        }

        var imageField = new ObjectField("Image");
        imageField.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
        imageField.objectType = typeof(Sprite);
        imageField.labelElement.style.minWidth = 100;
        imageField.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoImageBoxImageField:" + dialogueNode.GUID);
            property.ImageBoxSprite = (Sprite)evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });

        var anchorDropdown = new DropdownField("Anchor", Enum.GetNames(typeof(ImageBoxAnchor)).ToList(), 0);
        anchorDropdown.labelElement.style.minWidth = 20;
        anchorDropdown.RegisterValueChangedCallback(evt => {
            //Reset dropdown
            object actionType;
            if (System.Enum.TryParse(typeof(ImageBoxAnchor), evt.newValue, out actionType)) {
                Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoImageBoxAnchor:" + dialogueNode.GUID);
                property.anchor = (ImageBoxAnchor)actionType;
                EditorUtility.SetDirty(graphView.containerCache);
            }
        });
        anchorDropdown.SetValueWithoutNotify(property.anchor.ToString());

        var radioGroup = new RadioButtonGroup("Show Box", new List<string> { "Show", "Hide" });
        radioGroup.RegisterValueChangedCallback(evt => {
            if (evt.newValue < 0) { return; }
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoImageBoxShowToggle:" + dialogueNode.GUID);
            property.showHideToggle = evt.newValue == 0;
            EditorUtility.SetDirty(graphView.containerCache);
        });

        if (property != null) {
            imageField.SetValueWithoutNotify(property.ImageBoxSprite);
            radioGroup.SetValueWithoutNotify(property.showHideToggle == true ? 0 : 1);
        }

        imageField.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1);
        radioGroup.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1);
        anchorDropdown.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1);
        Add(radioGroup);
        Add(imageField);
        Add(anchorDropdown);

        var deleteButton = new Button(() => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoImageBoxRemove:" + dialogueNode.GUID);
            nodeData.PropertiesList.Remove(property);
            EditorUtility.SetDirty(graphView.containerCache);
            dialogueNode.mainContainer.Remove(this);
        }) { text = "X" };
        deleteButton.style.width = 20;
        radioGroup.Add(deleteButton);

        style.borderBottomWidth = 1;
        style.borderBottomColor = new Color(0, 0, 0, 1);
    }
}
