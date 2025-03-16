using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

[Serializable]
public class DialogueNode : BaseNode {
    public DialogueNodeData nodeData;
    public override BaseNodeData NodeData {
        get { return nodeData; }
        set { nodeData = value is DialogueNodeData ? (DialogueNodeData)value : null; }
    }

    public static DialogueNode CreateNode(DialogueGraphView graphView, string nodeName, Vector2 position, string characterId = "", DialogueNodeData nodeData = null, List<Edge> outputEdges = null) {
        var containerCache = graphView.containerCache;
        string decoration = nodeName.Length > 14 ? "..." : "";
        string shortTitle = nodeName.Substring(0, Mathf.Clamp(nodeName.Length, 0, 14)) + decoration;
        var dialogueNode = new DialogueNode {
            title = shortTitle,
            NodeData = nodeData == null ? new DialogueNodeData() : nodeData,
            GUID = Guid.NewGuid().ToString()
        };
        DialogueNodeData dialogueNodeData = dialogueNode.NodeData as DialogueNodeData;
        if (nodeData == null) {
            containerCache.DialogueNodeDatas.Add((DialogueNodeData)dialogueNode.NodeData);
            dialogueNode.NodeData.Guid = dialogueNode.GUID;
            dialogueNode.NodeData.Position = position;
        } else {
            dialogueNode.GUID = nodeData.Guid;
        }
        dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/Node"));

        //Input Port
        var inputPort = dialogueNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        inputPort.portName = "Input";
        dialogueNode.inputContainer.Add(inputPort);

        //Output Ports
        if(outputEdges != null && outputEdges.Count > 0) {
            outputEdges.ForEach(edge => {
                NodeLinkData linkData = graphView.containerCache.NodeLinks.Find(link => link.Guid == edge.viewDataKey);
                Port newPort = AddChoicePort(graphView, dialogueNode, linkData.PortName);
                edge.output = newPort;
                newPort.Connect(edge);
            });
        } else {
            AddChoicePort(graphView, dialogueNode, "Next");
        }

        //Add choice button
        var button = new Button(() => { AddChoicePort(graphView, dialogueNode); });
        button.text = "New Choice";
        dialogueNode.titleContainer.Add(button);

        //Dialogue Text
        var dialogueLabel = new Label("Dialogue");
        dialogueNode.mainContainer.Add(dialogueLabel);
        var dialogueField = new TextField(string.Empty);
        dialogueField.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoDialogue:" + dialogueNode.GUID);
            dialogueNodeData.DialogueText = evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
            string decoration = evt.newValue.Length > 14 ? "..." : "";
            dialogueNode.title = evt.newValue.Substring(0, Mathf.Clamp(evt.newValue.Length, 0, 14)) + decoration;
        });
        dialogueField.style.maxHeight = 200;
        dialogueField.style.maxWidth = 260;
        dialogueField.style.whiteSpace = WhiteSpace.Normal;
        dialogueField.SetValueWithoutNotify(dialogueNodeData.DialogueText);
        dialogueNode.mainContainer.Add(dialogueField);
        dialogueField.MarkDirtyRepaint();

        //Speaking Character
        var characterList = NovelData.instance.GetCharacterIds().Where(x => x != "").ToList();
        characterList.Add(NovelData.NARRATOR);
        var characterDropdown = new DropdownField("Speaking Character", characterList, 0);
        characterDropdown.labelElement.style.minWidth = 20;
        characterDropdown.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoSpeaking:" + dialogueNode.GUID);
            dialogueNodeData.SpeakingCharacterId = evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
            if(evt.newValue != NovelData.NARRATOR) {
                CharacterInfoSO charInfo = NovelData.instance.GetCharacterInfoSO(dialogueNodeData.SpeakingCharacterId);
                if (charInfo != null) {
                    dialogueNode.titleContainer.style.backgroundColor = charInfo.color;
                }
            } else {
                dialogueNode.titleContainer.style.backgroundColor = Color.gray;
            }
        });
        if (dialogueNodeData.SpeakingCharacterId != NovelData.NARRATOR) {
            CharacterInfoSO charInfo = NovelData.instance.GetCharacterInfoSO(dialogueNodeData.SpeakingCharacterId);
            if (charInfo != null) {
                dialogueNode.titleContainer.style.backgroundColor = charInfo.color;
            }
        } else {
            dialogueNode.titleContainer.style.backgroundColor = Color.gray;
        }
        characterDropdown.SetValueWithoutNotify(characterId.ToString());
        dialogueNode.mainContainer.Add(characterDropdown);

        //Nameplate Override
        var nameplateLabel = new Label("Nameplate Override");
        dialogueNode.mainContainer.Add(nameplateLabel);
        nameplateLabel.tooltip = "This will be shown as the character's name instead of their default Character Info name. Leave blank to use the character's default name.";
        var nameplateField = new TextField(string.Empty);
        nameplateField.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoNameplate:" + dialogueNode.GUID);
            dialogueNodeData.NameplateOverride = evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        nameplateField.style.maxHeight = 100;
        nameplateField.style.maxWidth = 260;
        nameplateField.style.whiteSpace = WhiteSpace.Normal;
        nameplateField.SetValueWithoutNotify(dialogueNodeData.NameplateOverride);
        dialogueNode.mainContainer.Add(nameplateField);
        nameplateField.MarkDirtyRepaint();

        //ExclaimTextBox
        var exclaimCheckbox = new Toggle("Shouting Textbox");
        exclaimCheckbox.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoExclaim:" + dialogueNode.GUID);
            dialogueNodeData.ExclaimTextBox = evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        dialogueNode.mainContainer.Add(exclaimCheckbox);
        exclaimCheckbox.SetValueWithoutNotify(dialogueNodeData.ExclaimTextBox);
        exclaimCheckbox.Children().Where(x => !(x is Label)).FirstOrDefault().style.flexDirection = FlexDirection.RowReverse;

        var characterFoldout = new Foldout();
        characterFoldout.text = "Characters";
        //FadeInlist
        var fadeInList = new FadeInList(dialogueNode, graphView);
        characterFoldout.Add(fadeInList);

        //ChangeAnimationList
        var changeAnimationList = new ChangeAnimationList(dialogueNode, graphView);
        characterFoldout.Add(changeAnimationList);

        //Reaction Effect
        var reactionList = new ReactionList(dialogueNode, graphView);
        characterFoldout.Add(reactionList);

        //FadeOutlist
        var fadeOutList = new CharacterList(dialogueNode, graphView, dialogueNodeData.FadeOutList);
        characterFoldout.Add(fadeOutList);

        characterFoldout.value = fadeInList.value || changeAnimationList.value || reactionList.value || fadeOutList.value;
        dialogueNode.mainContainer.Add(characterFoldout);

        var sceneFoldout = new Foldout();
        sceneFoldout.text = "Scene";
        bool openSceneFoldout = false;
        //Background
        var backgroundField = new ObjectField("Background");
        backgroundField.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
        backgroundField.objectType = typeof(Sprite);
        backgroundField.labelElement.style.minWidth = 100;
        backgroundField.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoBackground:" + dialogueNode.GUID);
            dialogueNodeData.Background = (Sprite)evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        if (dialogueNodeData.Background != null) {
            backgroundField.SetValueWithoutNotify(dialogueNodeData.Background);
            openSceneFoldout = true;
        }
        sceneFoldout.Add(backgroundField);

        //Stop Music
        var stopMusicCheckbox = new Toggle("Stop Music");
        stopMusicCheckbox.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
        stopMusicCheckbox.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoStopMusic:" + dialogueNode.GUID);
            dialogueNodeData.StopMusic = evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        sceneFoldout.Add(stopMusicCheckbox);
        stopMusicCheckbox.SetValueWithoutNotify(dialogueNodeData.StopMusic);
        stopMusicCheckbox.Children().Where(x => !(x is Label)).FirstOrDefault().style.flexDirection = FlexDirection.RowReverse;

        //Music
        var musicField = new ObjectField("Music");
        musicField.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
        musicField.objectType = typeof(AudioClip);
        musicField.labelElement.style.minWidth = 100;
        musicField.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoMusic:" + dialogueNode.GUID);
            dialogueNodeData.Music = (AudioClip)evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        if (dialogueNodeData.Music != null) {
            musicField.SetValueWithoutNotify(dialogueNodeData.Music);
            openSceneFoldout = true;
        }
        sceneFoldout.Add(musicField);

        //Sound
        var soundField = new ObjectField("Sound");
        soundField.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
        soundField.objectType = typeof(AudioClip);
        soundField.labelElement.style.minWidth = 100;
        soundField.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoSound:" + dialogueNode.GUID);
            dialogueNodeData.Sound = (AudioClip)evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        if (dialogueNodeData.Sound != null) {
            soundField.SetValueWithoutNotify(dialogueNodeData.Sound);
            openSceneFoldout = true;
        }
        sceneFoldout.Add(soundField);

        //Screen Fade In
        var fadeInCheckbox = new Toggle("Fade From Black");
        fadeInCheckbox.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
        fadeInCheckbox.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoScreenIn:" + dialogueNode.GUID);
            dialogueNodeData.ScreenFadeIn = evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        openSceneFoldout = dialogueNodeData.ScreenFadeIn ? true : openSceneFoldout;
        sceneFoldout.Add(fadeInCheckbox);
        fadeInCheckbox.SetValueWithoutNotify(dialogueNodeData.ScreenFadeIn);
        fadeInCheckbox.Children().Where(x => !(x is Label)).FirstOrDefault().style.flexDirection = FlexDirection.RowReverse;

        //Screen Fade Out
        var fadeOutCheckbox = new Toggle("Fade To Black");
        fadeOutCheckbox.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
        fadeOutCheckbox.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoScreenOut:" + dialogueNode.GUID);
            dialogueNodeData.ScreenFadeOut = evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        openSceneFoldout = dialogueNodeData.ScreenFadeOut ? true : openSceneFoldout;
        sceneFoldout.Add(fadeOutCheckbox);
        fadeOutCheckbox.SetValueWithoutNotify(dialogueNodeData.ScreenFadeOut);
        fadeOutCheckbox.Children().Where(x => !(x is Label)).FirstOrDefault().style.flexDirection = FlexDirection.RowReverse;

        //Camera Shake
        var cameraShakeFoldout = new Foldout();
        cameraShakeFoldout.text = "Camera Shake";
        bool cameraShakeFoldoutOpen = false;

        //Camera Shake Trigger
        var cameraShakeCheckbox = new Toggle("Do Shake");
        cameraShakeCheckbox.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoCameraShake:" + dialogueNode.GUID);
            dialogueNodeData.CameraShake = evt.newValue;
            EditorUtility.SetDirty(graphView.containerCache);
        });
        cameraShakeFoldoutOpen = dialogueNodeData.CameraShake ? true : cameraShakeFoldoutOpen;
        cameraShakeFoldout.Add(cameraShakeCheckbox);
        cameraShakeCheckbox.SetValueWithoutNotify(dialogueNodeData.CameraShake);
        cameraShakeCheckbox.Children().Where(x => !(x is Label)).FirstOrDefault().style.flexDirection = FlexDirection.RowReverse;

        //Shake Amplitude
        var shakeAmplitudeSlider = new Slider("Shake Amplitude", 1, 200, SliderDirection.Horizontal);
        shakeAmplitudeSlider.labelElement.style.minWidth = 100;
        shakeAmplitudeSlider.tooltip = "How large the initial shake will be that then dwindles during the duration of the shake.";
        var shakeAmplitudeText = new Label("1");
        shakeAmplitudeText.style.minWidth = 30;
        shakeAmplitudeSlider.contentContainer.Add(shakeAmplitudeText);
        shakeAmplitudeSlider.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoShakeAmplitude:" + dialogueNode.GUID);
            dialogueNodeData.ShakeAmplitude = evt.newValue;
            shakeAmplitudeText.text = dialogueNodeData.ShakeAmplitude.ToString();
            EditorUtility.SetDirty(graphView.containerCache);
        });
        cameraShakeFoldoutOpen = dialogueNodeData.CameraShake ? true : cameraShakeFoldoutOpen;
        shakeAmplitudeSlider.SetValueWithoutNotify(dialogueNodeData.ShakeAmplitude);
        shakeAmplitudeText.text = dialogueNodeData.ShakeAmplitude.ToString();
        cameraShakeFoldout.Add(shakeAmplitudeSlider);

        //Shake Duration
        var shakeDurationSlider = new Slider("Shake Duration", 0.2f, 5, SliderDirection.Horizontal);
        shakeDurationSlider.labelElement.style.minWidth = 100;
        shakeDurationSlider.tooltip = "How long the shake goes on for.";
        var shakeDurationText = new Label("0.2");
        shakeDurationText.style.minWidth = 30;
        shakeDurationSlider.contentContainer.Add(shakeDurationText);
        shakeDurationSlider.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoFadeShakeDuration:" + dialogueNode.GUID);
            dialogueNodeData.ShakeDuration = evt.newValue;
            shakeDurationText.text = dialogueNodeData.ShakeDuration.ToString();
            EditorUtility.SetDirty(graphView.containerCache);
        });
        cameraShakeFoldoutOpen = dialogueNodeData.CameraShake ? true : cameraShakeFoldoutOpen;
        shakeDurationSlider.SetValueWithoutNotify(dialogueNodeData.ShakeDuration);
        shakeDurationText.text = dialogueNodeData.ShakeDuration.ToString();
        cameraShakeFoldout.Add(shakeDurationSlider);

        cameraShakeFoldout.value = cameraShakeFoldoutOpen;
        sceneFoldout.Add(cameraShakeFoldout);
        sceneFoldout.value = openSceneFoldout || cameraShakeFoldoutOpen;
        dialogueNode.mainContainer.Add(sceneFoldout);

        //Special Actions
        var actionsList = new SpecialActionsList(dialogueNode, graphView);
        dialogueNode.mainContainer.Add(actionsList);

        dialogueNode.mainContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
        dialogueNode.style.minWidth = defaultNodeSize.x;
        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(position, defaultNodeSize));

        var GUIDLabel = new Label(dialogueNode.GUID);
        dialogueNode.mainContainer.Add(GUIDLabel);

        return dialogueNode;
    }

    public static Port AddChoicePort(DialogueGraphView graphView, DialogueNode dialogueNode, string overriddenPortName = "") {
        //limit choices
        if (dialogueNode.outputContainer.childCount >= 3) { return null; }

        var generatedPort = dialogueNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        generatedPort.Children().ToList().ForEach(x => x.pickingMode = PickingMode.Position);
        var oldLabel = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(oldLabel);

        var outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;

        var choicePortName = string.IsNullOrEmpty(overriddenPortName) ? $"Choice {outputPortCount + 1}" : overriddenPortName;

        var textField = new TextField {
            name = string.Empty,
            value = choicePortName
        };
        textField.RegisterValueChangedCallback(evt => {
            generatedPort.portName = evt.newValue;
            Edge portEdge = graphView.edges.ToList().Find(x => x.output == generatedPort);
            if (portEdge != null) {
                NodeLinkData linkData = graphView.containerCache.NodeLinks.Find(x => x.Guid == portEdge.viewDataKey);
                Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoPortRename:" + dialogueNode.GUID);
                linkData.PortName = evt.newValue;
                EditorUtility.SetDirty(graphView.containerCache);
            }
        });
        textField.style.flexDirection = FlexDirection.Column;
        generatedPort.contentContainer.Add(new Label("  "));
        generatedPort.contentContainer.Add(textField);

        //Only add a delete button for extra ports. There should always be one port on a dialogue node
        if (dialogueNode.outputContainer.childCount >= 1) {
            var deleteButton = new Button(() => {
                RemovePort(graphView, dialogueNode, generatedPort);
            }) { text = "X" };
            generatedPort.contentContainer.Add(deleteButton);
        }

        generatedPort.portName = choicePortName;

        dialogueNode.outputContainer.Add(generatedPort);
        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
        return generatedPort;
    }

    public static void RemovePort(DialogueGraphView graphView, DialogueNode dialogueNode, Port generatedPort) {
        var targetEdge = graphView.edges.ToList().Where(x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

        if (targetEdge.Any()) {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoChoiceRemove");
            graphView.containerCache.NodeLinks.Remove(graphView.containerCache.NodeLinks.Find(x => x.Guid == edge.viewDataKey));
            EditorUtility.SetDirty(graphView.containerCache);
            graphView.RemoveElement(targetEdge.First());
        }

        dialogueNode.outputContainer.Remove(generatedPort);
        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();

    }
}
