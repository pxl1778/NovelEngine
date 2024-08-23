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
using System.IO;

[Serializable]
public class DialogueGraphView : GraphView
{
    private NodeSearchWindow _searchWindow;
    private EditorWindow _editorWindow;
    private BlackboardField shortLineProperty;
    public DialogueContainer containerCache;

    public DialogueGraphView(EditorWindow editorWindow) {
        _editorWindow = editorWindow;
        styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/DialogueGraph"));
        SetupZoom(ContentZoomer.DefaultMinScale, 2.0f);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        GenerateEntryPointNode();
        AddSearchWindow();

        graphViewChanged = OnGraphChange;
    }

    private GraphViewChange OnGraphChange(GraphViewChange change) {
        if (containerCache == null) return change;
        if(change.edgesToCreate != null) {
            change.edgesToCreate.ForEach(edge => {
                var outputNode = edge.output.node as BaseNode;
                var inputNode = edge.input.node as BaseNode;
                Undo.RegisterCompleteObjectUndo(containerCache, "NodeUndoAddEdge");
                containerCache.NodeLinks.Add(new NodeLinkData {
                    BaseNodeGuid = outputNode.GUID,
                    PortName = edge.output.portName,
                    Guid = edge.viewDataKey,
                    TargetNodeGuid = inputNode.GUID
                });
                if (outputNode is DialogueNode && outputNode.EntryPoint) {
                    containerCache.StartingNodeGUID = outputNode.GUID;
                }
                EditorUtility.SetDirty(containerCache);
            });
        }
        if(change.elementsToRemove != null) {
            change.elementsToRemove.ForEach(element => {
                Undo.RegisterCompleteObjectUndo(containerCache, "NodeUndoRemoveStuff");
                if (element is Edge) {
                    containerCache.NodeLinks.RemoveAll(link => link.Guid == element.viewDataKey);
                }
                if (element is DialogueNode) {
                    containerCache.DialogueNodeDatas.RemoveAll(node => node.Guid == ((DialogueNode)element).GUID);
                }
                if (element is SceneNode) {
                    containerCache.SceneNodeDatas.RemoveAll(node => node.Guid == ((SceneNode)element).GUID);
                }
                EditorUtility.SetDirty(containerCache);
            });
        }
        if (change.movedElements != null) {
            string elementGuids = "";
            change.movedElements.ForEach(element => {
                elementGuids += ":" + ((BaseNode)element).GUID;
            });
            change.movedElements.ForEach(element => {
                if(element is BaseNode) {
                    Undo.RegisterCompleteObjectUndo(containerCache, "NodeUndoNodePosition" + elementGuids);
                    ((BaseNode)element).NodeData.Position = element.GetPosition().position;
                    EditorUtility.SetDirty(containerCache);
                }
            });
        }

        return change;
    }

    public void SetShortLineProperty(BlackboardField property) {
        shortLineProperty = property;
    }

    public void SetShortLine(string line) {
        shortLineProperty.text = line;
    }

    public string GetShortLine() {
        return shortLineProperty.text;
    }

    public void SetContainerCache(DialogueContainer containerCache) {
        this.containerCache = containerCache;
    }

    private void AddSearchWindow() {
        _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _searchWindow.Init(_editorWindow, this);
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
    }

    private Port GeneratePort(Node node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single) {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }

    public void GenerateEntryPointNode() {
        var node = new DialogueNode { title = "START", GUID = "EntryPointGUID", EntryPoint = true };

        var generatedPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        generatedPort.portName = "Next";
        node.outputContainer.Add(generatedPort);

        node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

        var GUIDLabel = new Label(node.GUID.ToString());
        node.mainContainer.Add(GUIDLabel);

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100, 200, 100, 150));
        AddElement(node);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
        var compatiblePorts = new List<Port>();
        ports.ForEach((port) => {
            if(startPort != port && startPort.node != port.node) {
                compatiblePorts.Add(port);
            }
        });
        return compatiblePorts;
    }

    public DialogueNode CreateDialogueNode(string nodeName, Vector2 position, string characterId = "", DialogueNodeData nodeData = null, List<Edge> outputEdges = null) {
        var dialogueNode = DialogueNode.CreateNode(this, nodeName, position, characterId, nodeData, outputEdges);
        AddElement(dialogueNode);
        return dialogueNode;
    }

    public SceneNode CreateSceneNode(Vector2 position, SceneNodeData nodeData = null) {
        var sceneNode = SceneNode.CreateNode(this, position, nodeData);
        AddElement(sceneNode);
        return sceneNode;
    }

    public void ReloadNode(string guid) {
        //Remake Node
        BaseNodeData reloadData = containerCache.DialogueNodeDatas.Find(x => x.Guid == guid);
        BaseNode newNode = null;
        BaseNode nodeToDelete = nodes.ToList().Cast<BaseNode>().ToList().Find(x => x.GUID == guid);
        RemoveElement(nodeToDelete);
        if (reloadData != null) {
            DialogueNodeData dialogueData = reloadData as DialogueNodeData;
            var outputEdges = edges.ToList().Where(x => ((BaseNode)x.output.node).GUID == guid).ToList();
            newNode = CreateDialogueNode(dialogueData.DialogueText, dialogueData.Position, dialogueData.SpeakingCharacterId, dialogueData, outputEdges);
        } else {
            reloadData = containerCache.SceneNodeDatas.Find(x => x.Guid == guid);
            SceneNodeData sceneData = reloadData as SceneNodeData;
            newNode = CreateSceneNode(sceneData.Position, sceneData);
        }

        //Reconnect Input Port
        var inputEdges = edges.ToList().Where(x => ((BaseNode)x.input.node).GUID == guid).ToList();
        Port inputPort = (Port)newNode.inputContainer[0];
        inputEdges.ForEach(x => {
            x.input = inputPort;
            inputPort.Connect(x);
        });
    }

#if UNITY_EDITOR
    [OnOpenAssetAttribute]
    public static bool OpenGraphAsset(int instanceID, int line) {
        // This gets called whenever ANY asset is double clicked 
        // So we gotta check if the asset is of the proper type
        UnityEngine.Object asset = EditorUtility.InstanceIDToObject(instanceID);
        if (!(asset is DialogueContainer)) return false;

        bool windowIsOpen = EditorWindow.HasOpenInstances<DialogueGraph>();
        if (!windowIsOpen) EditorWindow.CreateWindow<DialogueGraph>();
        else EditorWindow.FocusWindowIfItsOpen<DialogueGraph>();

        DialogueGraph window = EditorWindow.GetWindow<DialogueGraph>();
        string assetPath = AssetDatabase.GetAssetPath(instanceID);
        string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        string resourcesPath = assetPath.Substring(assetPath.IndexOf("/Resources/") + "/Resources/".Length);
        window.LoadGraphAssetViaFileName(resourcesPath, fileName);
        return true;
    }
#endif
}
