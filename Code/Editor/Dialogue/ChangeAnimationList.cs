using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Linq;
using UnityEditor.Callbacks;

public class ChangeAnimationList : Foldout {
    public DialogueGraphView graphView;

    public ChangeAnimationList(DialogueNode dialogueNode, DialogueGraphView graphView) {
        text = "Change Animation List";
        tooltip = "List of characters and their animations that they'll change to when this message begins.";
        this.graphView = graphView;

        var addButton = new Button(() => AddItem(dialogueNode)) { text = "+" };
        Add(addButton);

        style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);

        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
        if (nodeData.ChangeAnimationList != null && nodeData.ChangeAnimationList.Keys.Count > 0) {
            nodeData.ChangeAnimationList.Keys.ToList().ForEach(key => AddItem(dialogueNode, key, true));
            value = true;
        } else {
            value = false;
        }
        UpdateListItems();
    }

    private void AddItem(DialogueNode dialogueNode, string characterId = "", bool loadingData = false) {
        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
        if (loadingData) {
            Add(new ChangeAnimationListItem(this, dialogueNode, characterId, nodeData.ChangeAnimationList[characterId]));
        } else if(!nodeData.ChangeAnimationList.ContainsKey(characterId)) {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoSpecialAdd:" + dialogueNode.GUID);
            nodeData.ChangeAnimationList.Add(characterId, "");
            Add(new ChangeAnimationListItem(this, dialogueNode, characterId, ""));
            EditorUtility.SetDirty(graphView.containerCache);
        }
    }

    public void RemoveItem(ChangeAnimationListItem listItem) {
        Remove(listItem);
        UpdateListItems();
    }

    public void UpdateListItems() {
        List<string> choices = GetCharacterList();
        Children().Where(x => x is FadeInListItem).ToList().ForEach(x => ((ChangeAnimationListItem)x).UpdateCharacterChoices(choices));
    }

    public List<string> GetCharacterList() {
        var itemList = Children().Where(x => x is ChangeAnimationListItem).Select(x => ((ChangeAnimationListItem)x).dropdownCharacter.ToString());
        return NovelData.instance.GetCharacterIds().Where(x => !itemList.Contains(x) && x != "").ToList();
    }
}

public class ChangeAnimationListItem : VisualElement {
    public string dropdownCharacter = "";
    private DialogueNode dialogueNode;
    private DialogueNodeData nodeData;
    private DropdownField characterDropdown;
    private DropdownField animatorDropdown;
    private ChangeAnimationList parentContainer;

    public ChangeAnimationListItem(ChangeAnimationList parentContainer, DialogueNode dialogueNode, string characterId = "", string animation = "") {
        this.dialogueNode = dialogueNode;
        this.parentContainer = parentContainer;
        dropdownCharacter = characterId;
        nodeData = dialogueNode.NodeData as DialogueNodeData;

        List<string> characters = parentContainer.GetCharacterList().ToList();
        characterDropdown = new DropdownField("Character", characters, 0);
        characterDropdown.labelElement.style.minWidth = 20;
        var deleteButton = new Button(() => RemoveItem()) { text = "X" };
        characterDropdown.contentContainer.Add(deleteButton);
        characterDropdown.RegisterValueChangedCallback(evt => {
            SetCharacterDropdown(evt.newValue, evt.previousValue);
        });
        characterDropdown.SetValueWithoutNotify(characterId.ToString());
        Add(characterDropdown);

        animatorDropdown = new DropdownField("Animation", new List<string> { "" }, 0);
        animatorDropdown.labelElement.style.minWidth = 20;
        animatorDropdown.SetValueWithoutNotify(animation);
        Add(animatorDropdown);
        UpdateAnimationDropdownItems(characterId);

        style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1);
        style.borderBottomColor = new Color(0, 0, 0, 1);
        style.borderBottomLeftRadius = 6;
        style.borderBottomWidth = 2;
    }

    public void UpdateCharacterChoices(List<string> characters) {
        characterDropdown.choices = characters;
    }

    private void SetCharacterDropdown(string newValue, string oldValue) {
        //Reset dropdown
        animatorDropdown.value = "";
        animatorDropdown.choices = new List<string> { "" };
        dropdownCharacter = newValue;
        if (nodeData.ChangeAnimationList.ContainsKey(oldValue)) {
            Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoChangeAnimationRemove:" + dialogueNode.GUID);
            nodeData.ChangeAnimationList.Remove(oldValue);
            nodeData.ChangeAnimationList.Add(dropdownCharacter, "");
            EditorUtility.SetDirty(parentContainer.graphView.containerCache);
        }
        parentContainer.UpdateListItems();
        UpdateAnimationDropdownItems(newValue);
    }

    private void UpdateAnimationDropdownItems(string characterId) {
        CharacterInfoSO charInfo = NovelData.instance.GetCharacterInfoSO(characterId);
        if (charInfo != null) {
            Animator animator = charInfo.characterPrefab.GetComponentInChildren<Animator>();
            if (animator.runtimeAnimatorController) {
                var controller = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(UnityEditor.AssetDatabase.GetAssetPath(animator.runtimeAnimatorController));
                if (controller != null) {
                    List<string> parameters = controller.parameters.Where(x => x.name != "Talking").Select(x => x.name).ToList();
                    animatorDropdown.choices = parameters;
                    animatorDropdown.RegisterValueChangedCallback(evt => {
                        SetAnimationDropdown(evt.newValue);
                    });
                }
            }
        }
    }

    private void SetAnimationDropdown(string newValue) {
        Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoChangeAnimationAdd:" + dialogueNode.GUID);
        if (nodeData.ChangeAnimationList.ContainsKey(characterDropdown.value)) {
            nodeData.ChangeAnimationList[characterDropdown.value] = newValue;
        } else {
            nodeData.ChangeAnimationList.Add(characterDropdown.value, newValue);
        }
        EditorUtility.SetDirty(parentContainer.graphView.containerCache);
    }

    private void RemoveItem() {
        if (nodeData.ChangeAnimationList.ContainsKey(dropdownCharacter)) {
            Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoChangeAnimationRemove:" + dialogueNode.GUID);
            nodeData.ChangeAnimationList.Remove(dropdownCharacter);
            EditorUtility.SetDirty(parentContainer.graphView.containerCache);
        }

        parentContainer.RemoveItem(this);
    }
}
