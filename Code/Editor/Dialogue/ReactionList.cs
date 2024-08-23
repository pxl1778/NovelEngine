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

public class ReactionList : Foldout {
    public DialogueGraphView graphView;

    public ReactionList(DialogueNode dialogueNode, DialogueGraphView graphView) {
        this.graphView = graphView;
        text = "Reactions List";
        tooltip = "List of particle effects to play on characters when this dialogue line appears.";

        var addButton = new Button(() => AddItem(dialogueNode)) { text = "+" };
        Add(addButton);

        style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);

        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
        if (nodeData.ReactionList != null && nodeData.ReactionList.Count > 0) {
            for(int i = 0; i < nodeData.ReactionList.Count; i++) {
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
            Add(new ReactionListItem(this, dialogueNode, nodeData.ReactionList[index]));
        } else {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoReactionAdd:" + dialogueNode.GUID);
            ReactionListItemData data = new ReactionListItemData("", null);
            nodeData.ReactionList.Add(data);
            Add(new ReactionListItem(this, dialogueNode, data));
            EditorUtility.SetDirty(graphView.containerCache);
        }
    }

    public void RemoveItem(ReactionListItem listItem) {
        Remove(listItem);
    }
}

public class ReactionListItem : VisualElement {

    private DialogueNode dialogueNode;
    private DialogueNodeData nodeData;
    private DropdownField characterDropdown;
    private ObjectField reactionField;
    private ReactionList parentContainer;

    public ReactionListItem(ReactionList parentContainer, DialogueNode dialogueNode, ReactionListItemData reactionData) {
        this.dialogueNode = dialogueNode;
        this.parentContainer = parentContainer;
        nodeData = dialogueNode.NodeData as DialogueNodeData;

        List<string> characters = NovelData.instance.GetCharacterIds().Where(x => x != "Null").ToList();
        characterDropdown = new DropdownField("Character", characters, 0);
        characterDropdown.labelElement.style.minWidth = 20;
        var deleteButton = new Button(() => RemoveItem(reactionData)) { text = "X" };
        characterDropdown.contentContainer.Add(deleteButton);
        characterDropdown.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoReactionCharacter:" + dialogueNode.GUID);
            reactionData.characterId = evt.newValue;
            EditorUtility.SetDirty(parentContainer.graphView.containerCache);
        });
        characterDropdown.SetValueWithoutNotify(reactionData.characterId.ToString());
        Add(characterDropdown);

        reactionField = new ObjectField("Reaction");
        reactionField.objectType = typeof(GameObject);
        reactionField.tooltip = "The gameobject placed here will be instantiated on the character at the beginning of the dialogue line. This object should have a particle system or an animator that will play on wake.";
        reactionField.labelElement.style.minWidth = 20;
        reactionField.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoReactionPrefab:" + dialogueNode.GUID);
            reactionData.reaction = (GameObject)evt.newValue;
            EditorUtility.SetDirty(parentContainer.graphView.containerCache);
        });
        if(reactionData.reaction != null) {
            reactionField.SetValueWithoutNotify(reactionData.reaction);
        }
        Add(reactionField);

        style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1);
        style.borderBottomColor = new Color(0, 0, 0, 1);
        style.borderBottomLeftRadius = 6;
        style.borderBottomWidth = 2;
    }

    private void RemoveItem(ReactionListItemData reactionData) {
        Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoReactionDelete:" + dialogueNode.GUID);
        nodeData.ReactionList.Remove(reactionData);
        parentContainer.RemoveItem(this);
        EditorUtility.SetDirty(parentContainer.graphView.containerCache);
    }
}
