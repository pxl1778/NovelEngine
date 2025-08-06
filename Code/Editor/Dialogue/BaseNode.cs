using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

[Serializable]
public class BaseNode : Node {
    public string GUID;
    public bool EntryPoint;
    public virtual BaseNodeData NodeData { get; set; }

    public static readonly Vector2 defaultNodeSize = new Vector2(260, 150);
}
