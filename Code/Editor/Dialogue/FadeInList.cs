using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Linq;
using UnityEditor.Callbacks;

public class FadeInList : Foldout {
    public DialogueGraphView graphView;

    public FadeInList(DialogueNode dialogueNode, DialogueGraphView graphView) {
        text = "Fade In List";
        tooltip = "List of characters, their animations, and their positions that they'll use to fade into the scene at the beginning of this dialogue line.";
        this.graphView = graphView;

        var addButton = new Button(() => AddItem(dialogueNode)) { text = "+" };
        Add(addButton);

        style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);

        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
        if (nodeData.FadeInList != null && nodeData.FadeInList.Keys.Count > 0) {
            nodeData.FadeInList.Keys.ToList().ForEach(key => AddItem(dialogueNode, key, true));
            value = true;
        } else {
            value = false;
        }
        UpdateListItems();
    }

    private void AddItem(DialogueNode dialogueNode, string character = "", bool loadingData = false) {
        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
        if (loadingData) {
            Add(new FadeInListItem(this, dialogueNode, character, nodeData.FadeInList[character]));
        } else if (!nodeData.FadeInList.ContainsKey(character)) {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoFadeInAdd:" + dialogueNode.GUID);
            FadeInListItemData data = new FadeInListItemData("");
            nodeData.FadeInList.Add(character, data);
            Add(new FadeInListItem(this, dialogueNode, character, data));
            EditorUtility.SetDirty(graphView.containerCache);
        }
    }

    public void RemoveItem(FadeInListItem listItem) {
        Remove(listItem);
        UpdateListItems();
    }

    public void UpdateListItems() {
        List<string> choices = GetCharacterList();
        Children().Where(x => x is FadeInListItem).ToList().ForEach(x => ((FadeInListItem)x).UpdateCharacterChoices(choices));
    }

    public List<string> GetCharacterList() {
        var itemList = Children().Where(x => x is FadeInListItem).Select(x => ((FadeInListItem)x).dropdownCharacter);
        return NovelData.instance.GetCharacterIds().Where(x => !itemList.Contains(x) && x != "").ToList();
    }
}

public class FadeInListItem : VisualElement {
    public string dropdownCharacter = "";
    private DialogueNode dialogueNode;
    private DialogueNodeData nodeData;
    private DropdownField characterDropdown;
    private DropdownField animatorDropdown;
    private FloatField positionField;
    private FadeInList parentContainer;

    public FadeInListItem(FadeInList parentContainer, DialogueNode dialogueNode, string characterId, FadeInListItemData listItemData) {
        this.dialogueNode = dialogueNode;
        this.parentContainer = parentContainer;
        dropdownCharacter = characterId;
        nodeData = dialogueNode.NodeData as DialogueNodeData;

        //Character
        List<string> characters = parentContainer.GetCharacterList().ToList();
        characterDropdown = new DropdownField("Character", characters, 0);
        characterDropdown.labelElement.style.minWidth = 20;
        var deleteButton = new Button(() => RemoveItem()) { text = "X" };
        characterDropdown.contentContainer.Add(deleteButton);
        characterDropdown.RegisterValueChangedCallback(evt => {
            SetCharacterDropdown(evt.newValue, evt.previousValue);
        });
        characterDropdown.SetValueWithoutNotify(characterId);
        Add(characterDropdown);

        //Animation
        animatorDropdown = new DropdownField("Animation", new List<string> { "" }, 0);
        animatorDropdown.labelElement.style.minWidth = 20;
        animatorDropdown.SetValueWithoutNotify(listItemData.animation);
        Add(animatorDropdown);
        UpdateAnimatorDropdownItems(characterId);

        //Position
        positionField = new FloatField("Position");
        positionField.labelElement.style.minWidth = 100;
        positionField.tooltip = "Position is a number that signifies the location on screen that a sprite will be. Position 1 is on screen all the way to the left and Position 5 is on screen to the right. Beyond those numbers is off screen and 3 is in the middle. Feel free to use decimal values as well.";
        positionField.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoFadeInPosition:" + dialogueNode.GUID);
            nodeData.FadeInList[dropdownCharacter].position = evt.newValue;
            EditorUtility.SetDirty(parentContainer.graphView.containerCache);
        });
        positionField.SetValueWithoutNotify(listItemData.position);
        Add(positionField);

        //Flipped
        var flipCheckbox = new Toggle("Start Flipped");
        flipCheckbox.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoFadeInFlipped:" + dialogueNode.GUID);
            nodeData.FadeInList[dropdownCharacter].flipped = evt.newValue;
            EditorUtility.SetDirty(parentContainer.graphView.containerCache);
        });
        Add(flipCheckbox);
        flipCheckbox.SetValueWithoutNotify(listItemData.flipped);
        flipCheckbox.Children().Where(x => !(x is Label)).FirstOrDefault().style.flexDirection = FlexDirection.RowReverse;

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
        if (nodeData.FadeInList.ContainsKey(oldValue)) {
            Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoFadeInRemove:" + dialogueNode.GUID);
            nodeData.FadeInList.Remove(oldValue);
            nodeData.FadeInList.Add(dropdownCharacter, new FadeInListItemData("", positionField.value));
            EditorUtility.SetDirty(parentContainer.graphView.containerCache);
        }

        parentContainer.UpdateListItems();
        UpdateAnimatorDropdownItems(newValue);
    }

    private void UpdateAnimatorDropdownItems(string characterId) {
        CharacterInfoSO charInfo = NovelData.instance.GetCharacterInfoSO(characterId);
        if (charInfo != null) {
            if(charInfo.characterPrefab == null) {
                Debug.LogWarning("The charInfo for: " + characterId + " does not have a characterPrefab set in the CharacterInfoSO.");
                return;
            }
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
        string character = characterDropdown.value;
        Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoFadeInEdit:" + dialogueNode.GUID);
        if (nodeData.FadeInList.ContainsKey(character)) {
            nodeData.FadeInList[character] = new FadeInListItemData(newValue);
        } else {
            nodeData.FadeInList.Add(character, new FadeInListItemData(newValue));
        }
        EditorUtility.SetDirty(parentContainer.graphView.containerCache);
    }

    private void RemoveItem() {
        if (nodeData.FadeInList.ContainsKey(dropdownCharacter)) {
            Undo.RegisterCompleteObjectUndo(parentContainer.graphView.containerCache, "NodeUndoFadeInRemove:" + dialogueNode.GUID);
            nodeData.FadeInList.Remove(dropdownCharacter);
            EditorUtility.SetDirty(parentContainer.graphView.containerCache);
        }

        parentContainer.RemoveItem(this);
    }
}
