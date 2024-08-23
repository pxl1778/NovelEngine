using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Linq;
using UnityEditor.Callbacks;

public class SpecialActionsList : Foldout {
    public DialogueGraphView graphView;

    public SpecialActionsList(DialogueNode dialogueNode, DialogueGraphView graphView) {
        this.graphView = graphView;
        text = "Special Actions";

        var addButton = new Button(() => AddItem(dialogueNode)) { text = "+" };
        Add(addButton);

        style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);

        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
        if (nodeData.SpecialActionList != null && nodeData.SpecialActionList.Count > 0) {
            for (int i = 0; i < nodeData.SpecialActionList.Count; i++) {
                AddItem(dialogueNode, i);
            }
            value = true;
        } else {
            value = false;
        }
    }

    private void AddItem(DialogueNode dialogueNode, int index = -1) {
        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
        if (index != -1) {
            Add(new ActionListItem(this, dialogueNode, nodeData.SpecialActionList[index]));
        } else {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoSpecialAdd:" + dialogueNode.GUID);
            ActionListItemData data = new ActionListItemData(SpecialAction.Flip);
            nodeData.SpecialActionList.Add(data);
            Add(new ActionListItem(this, dialogueNode, data));
            EditorUtility.SetDirty(graphView.containerCache);
        }
    }

    public void RemoveItem(ActionListItem listItem) {
        Remove(listItem);
    }
}

public class ActionListItem : VisualElement {

    private string actionTypeString;
    private DialogueNode dialogueNode;
    private DialogueNodeData nodeData;
    private DropdownField actionDropdown;
    private DropdownField characterDropdown;
    private FloatField parameterTwoField;
    private SpecialActionsList parentContainer;

    public ActionListItem(SpecialActionsList parentContainer, DialogueNode dialogueNode, ActionListItemData listItemData) {
        this.dialogueNode = dialogueNode;
        this.parentContainer = parentContainer;
        nodeData = dialogueNode.NodeData as DialogueNodeData;

        actionDropdown = new DropdownField("Action", Enum.GetNames(typeof(SpecialAction)).ToList(), 0);
        actionDropdown.labelElement.style.minWidth = 20;
        actionDropdown.RegisterValueChangedCallback(evt => {
            //Reset dropdown
            object actionType;
            if (System.Enum.TryParse(typeof(SpecialAction), evt.newValue, out actionType)) {
                Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoSpecialAction:" + dialogueNode.GUID);
                characterDropdown.value = "";
                parameterTwoField.value = 3;
                actionTypeString = evt.newValue;
                listItemData.actionType = (SpecialAction)actionType;
                listItemData.parameterOne = "";
                listItemData.parameterTwo = 3;
                EditorUtility.SetDirty(parentContainer.graphView.containerCache);
            }
        });
        actionDropdown.SetValueWithoutNotify("");
        Add(actionDropdown);
        var deleteButton = new Button(() => RemoveItem(listItemData)) { text = "X" };
        actionDropdown.contentContainer.Add(deleteButton);

        List<string> characters = NovelData.instance.GetCharacterIds().Where(x => x != "Null").ToList();
        characterDropdown = new DropdownField("Character", characters, 0);
        characterDropdown.labelElement.style.minWidth = 20;
        characterDropdown.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoSpecialAction:" + dialogueNode.GUID);
            listItemData.parameterOne = evt.newValue;
            EditorUtility.SetDirty(parentContainer.graphView.containerCache);
        });
        characterDropdown.SetValueWithoutNotify("");
        Add(characterDropdown);

        parameterTwoField = new FloatField("Position");
        parameterTwoField.labelElement.style.minWidth = 100;
        parameterTwoField.tooltip = "Position is a number that signifies the location on screen that a sprite will be. Position 1 is on screen all the way to the left and Position 5 is on screen to the right. Beyond those numbers is off screen and 3 is in the middle. Feel free to use decimal values as well.";
        parameterTwoField.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoSpecialAction:" + dialogueNode.GUID);
            listItemData.parameterTwo = evt.newValue;
            EditorUtility.SetDirty(parentContainer.graphView.containerCache);
        });
        parameterTwoField.SetValueWithoutNotify(3);
        Add(parameterTwoField);

        actionDropdown.SetValueWithoutNotify(listItemData.actionType.ToString());
        characterDropdown.SetValueWithoutNotify(listItemData.parameterOne);
        parameterTwoField.SetValueWithoutNotify(listItemData.parameterTwo);

        style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1);
        style.borderBottomColor = new Color(0, 0, 0, 1);
        style.borderBottomLeftRadius = 6;
        style.borderBottomWidth = 2;
    }

    public void UpdateCharacterChoices(List<string> characters) {
        characterDropdown.choices = characters;
    }

    private void RemoveItem(ActionListItemData actionData) {
        Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoSpecialRemove:" + dialogueNode.GUID);
        nodeData.SpecialActionList.Remove(actionData);
        EditorUtility.SetDirty(parentContainer.graphView.containerCache);
        parentContainer.RemoveItem(this);
    }
}

