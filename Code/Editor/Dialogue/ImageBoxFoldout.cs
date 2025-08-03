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

    public ImageBoxFoldout(DialogueNode dialogueNode, DialogueGraphView graphView) {
        this.graphView = graphView;
        text = "Image Box";

        style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
        style.borderTopWidth = 1;
        style.borderTopColor = new Color(0, 0, 0, 1);

        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;

        ImageBoxProperty imageBoxProperty = (ImageBoxProperty)nodeData.PropertiesList.Where((property) => property is ImageBoxProperty).FirstOrDefault();
        if(imageBoxProperty == null) {
            nodeData.PropertiesList.Add(new ImageBoxProperty());
            imageBoxProperty = (ImageBoxProperty)nodeData.PropertiesList[nodeData.PropertiesList.Count - 1];
        }

        var imageField = new ObjectField("Image");
        imageField.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
        imageField.objectType = typeof(Sprite);
        imageField.labelElement.style.minWidth = 100;
        imageField.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoImageBoxImageField:" + dialogueNode.GUID);
            imageBoxProperty.ImageBoxSprite = (Sprite)evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });

        var radioGroup = new RadioButtonGroup("Show Box", new List<string> { "Show", "Hide" });
        radioGroup.RegisterValueChangedCallback(evt => {
            if (evt.newValue < 0) { return; }
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoImageBoxShowToggle:" + dialogueNode.GUID);
            imageBoxProperty.showHideToggle = evt.newValue == 0;
            EditorUtility.SetDirty(graphView.containerCache);
        });

        if (imageBoxProperty != null) {
            imageField.SetValueWithoutNotify(imageBoxProperty.ImageBoxSprite);
            radioGroup.SetValueWithoutNotify(imageBoxProperty.showHideToggle == true ? 0 : 1);
        }

        imageField.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1);
        radioGroup.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1);
        Add(imageField);
        Add(radioGroup);

        var deleteButton = new Button(() => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoImageBoxRemove:" + dialogueNode.GUID);
            nodeData.PropertiesList.Remove(imageBoxProperty);
            EditorUtility.SetDirty(graphView.containerCache);
            dialogueNode.mainContainer.Remove(this);
        }) { text = "X" };
        Add(deleteButton);

        style.borderBottomWidth = 1;
        style.borderBottomColor = new Color(0, 0, 0, 1);
    }
}
