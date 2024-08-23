using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Linq;
using UnityEditor.Callbacks;

public class CharacterList : Foldout {
    private DialogueNode dialogueNode;
    private DialogueGraphView graphView;

    public CharacterList(DialogueNode dialogueNode, DialogueGraphView graphView, List<string> characters = null) {
        this.graphView = graphView;
        this.dialogueNode = dialogueNode;
        text = "Fade Out List";
        tooltip = "List of characters that will fade out at the end of this dialogue line.";

        var addButton = new Button(() => AddItem("", true)) { text = "+" };
        Add(addButton);

        style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);

        if (characters != null && characters.Count > 0) {
            for(int i = 0; i < characters.Count; i++) {
                AddItem(characters[i], false);
            }
            value = true;
        } else {
            value = false;
        }
        UpdateListItems();
    }

    public void UpdateListItems() {
        List<string> choices = GetCharacterList();
        Children().Where(x => x is DropdownField).ToList().ForEach(x => ((DropdownField)x).choices = choices);
    }

    private void AddItem(string character, bool shouldUpdate) {
        List<string> characters = GetCharacterList();
        var characterDropdown = new DropdownField("Character", characters, 0);
        characterDropdown.labelElement.style.minWidth = 20;

        var deleteButton = new Button(() => RemoveItem(characterDropdown)) { text = "X" };
        characterDropdown.contentContainer.Add(deleteButton);

        characterDropdown.RegisterValueChangedCallback(evt => {
            UpdateList();
            UpdateListItems();
        });
        characterDropdown.SetValueWithoutNotify(character.ToString());
        Add(characterDropdown);

        style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1);
        style.borderBottomColor = new Color(0, 0, 0, 1);
        style.borderBottomLeftRadius = 6;
        style.borderBottomWidth = 2;
        UpdateListItems();
        if (shouldUpdate) {
            UpdateList();
        }
    }

    private void UpdateList() {
        Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoReactionPrefab:" + dialogueNode.GUID);
        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
        nodeData.FadeOutList = new List<string>();
        Children().Where(x => x is DropdownField).ToList().ForEach(dropdown => {
            string character = ((DropdownField)dropdown).value;
            nodeData.FadeOutList.Add(character);
        });
        EditorUtility.SetDirty(graphView.containerCache);
    }

    public void RemoveItem(DropdownField listItem) {
        string characterToRemove = listItem.value;
        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
        Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoReactionRemove:" + dialogueNode.GUID);
        nodeData.FadeOutList.Remove(characterToRemove);
        EditorUtility.SetDirty(graphView.containerCache);
        Remove(listItem);
        UpdateList();
    }

    public List<string> GetCharacterList() {
        var itemList = Children().Where(x => x is DropdownField).Select(x => ((DropdownField)x).value);
        return NovelData.instance.GetCharacterIds().Where(x => !itemList.Contains(x) && x != "").ToList();
    }
}