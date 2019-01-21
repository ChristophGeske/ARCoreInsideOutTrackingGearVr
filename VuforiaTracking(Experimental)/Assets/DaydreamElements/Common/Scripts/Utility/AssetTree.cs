// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DaydreamElements.Common {

  [Serializable]
  public class AssetTree {

    public class Node {
      public UnityEngine.Object value;
      public List<AssetTree.Node> children = new List<Node>();
      public Node parent;
    }

    private Node root;

    public Node Root {
      get {
        InitializeRoot();
        return root;
      }
    }

    /// Flattens an AssetTree.Node into the list serializedRoot which can then be serialized.
    public void SaveForSerialization() {
      Node top = Root;
      serializedRoot.Clear();
      SaveForSerialization(top);
    }

#region Serialization

    public const string ROOT_PROP = "serializedRoot";
#if UNITY_EDITOR
    public const string EXPANDED_PROP = "expanded";
#endif

    [Serializable]
    public class SerializedNode {
      public const string VALUE_PROP = "value";
      public const string CHILDREN_COUNT_PROP = "childrenCount";
#if UNITY_EDITOR
      public const string EXPANDED_PROP = "expanded";
#endif

      public UnityEngine.Object value;
      public int childrenCount;
#if UNITY_EDITOR
      public bool expanded;
#endif
    }

    [SerializeField]
    private List<SerializedNode> serializedRoot;

#if UNITY_EDITOR
    [SerializeField]
    private bool expanded;
#endif

    public void InitializeRoot() {
      if (root != null || serializedRoot.Count == 0) {
        return;
      }

      SerializedNode serializedNode = serializedRoot[0];
      root = new Node();
      root.value = serializedNode.value;
      AddChildren(root, 0);
    }

    private int AddChildren(Node node, int serializedNodeIndex) {
      if (serializedNodeIndex >= serializedRoot.Count) {
        return serializedNodeIndex;
      }

      SerializedNode serializedNode = serializedRoot[serializedNodeIndex];

      if (serializedNode.childrenCount == 0) {
        return serializedNodeIndex + 1;
      }

      int nextIndex = serializedNodeIndex + 1;
      for (int i = 0; i < serializedNode.childrenCount; i++) {
        SerializedNode nextSerializedNode = serializedRoot[nextIndex];
        Node childNode = new Node();
        childNode.parent = node;
        childNode.value = nextSerializedNode.value;
        node.children.Add(childNode);
        nextIndex = AddChildren(childNode, nextIndex);
      }

      return nextIndex;
    }

    private void SaveForSerialization(Node node) {
      if (node != null) {
        SerializedNode sNode = new SerializedNode();
        sNode.childrenCount = node.children.Count;
        sNode.value = node.value;
        serializedRoot.Add(sNode);

        foreach (var child in node.children) {
          SaveForSerialization(child);
        }
      }
      return;
    }
#endregion
  }
}
