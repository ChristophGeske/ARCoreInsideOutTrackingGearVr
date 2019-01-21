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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace DaydreamElements.Common {

  [CustomPropertyDrawer(typeof(AssetTree), true)]
  public class AssetTreePropertyDrawer : PropertyDrawer {
    public class DragData {
      public int originalIndex;
      public SerializedProperty node;
      public int originalParentIndex;
      public SerializedProperty parent;
    }

    private SerializedProperty nodesProp;

    private float lineHeight;
    float totalHeight;
    private Dictionary<int, int> indexToNextIndexMap = new Dictionary<int, int>();
    private Dictionary<int, float> indexToNodeHeightMap = new Dictionary<int, float>();

    private const string DRAG_DATA_IDENTIFIER = "TreePropertyDrawerDragData";
    private const float INSERT_PADDING = 6.0f;
    private const float INDENT_AMOUNT = 15.0f;

    public AssetTreePropertyDrawer() {
      Undo.undoRedoPerformed += OnUndoRedo;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      EditorGUI.BeginProperty(position, label, property);

      position.height = lineHeight;

      SerializedProperty open = property.FindPropertyRelative(AssetTree.EXPANDED_PROP);
      open.boolValue = EditorGUI.Foldout(position, open.boolValue, label);
      if (!open.boolValue) {
        return;
      }
      position.y += lineHeight;

      EditorGUI.indentLevel++;

      Rect box = position;
      box.height = totalHeight - lineHeight;
      GUI.Box(box, "");

      if (property.serializedObject.isEditingMultipleObjects) {
        EditorGUI.LabelField(position, "Editing Multiple Objects is not supported.");
      } else {
        nodesProp = property.FindPropertyRelative(AssetTree.ROOT_PROP);
        OnNodeGUI(position, null, -1, 0);
      }

      EditorGUI.indentLevel--;

      EditorGUI.EndProperty();
    }

    private void OnNodeGUI(Rect position, SerializedProperty parentProp, int parentIndex, int index) {
      if (index >= nodesProp.arraySize) {
        Rect initialDropArea = position;
        initialDropArea.height = lineHeight;
        DragAndDropChildren(initialDropArea, null, -1, null, 0);
        EditorGUI.LabelField(initialDropArea, new GUIContent("Drag Assets Here."));
        return;
      }

      SerializedProperty nodeProp = nodesProp.GetArrayElementAtIndex(index);
      SerializedProperty valueProp = nodeProp.FindPropertyRelative(AssetTree.SerializedNode.VALUE_PROP);
      SerializedProperty childrenCountProp = nodeProp.FindPropertyRelative(AssetTree.SerializedNode.CHILDREN_COUNT_PROP);
      int childrenCount = childrenCountProp.intValue;

      string name = "NULL";
      if (valueProp.objectReferenceValue != null) {
        name = valueProp.objectReferenceValue.name;
      }

      EditorGUI.LabelField(position, new GUIContent(name));

      Rect buttonPos = position;
      buttonPos.width = 20.0f;
      buttonPos.height = 15.0f;
      buttonPos.y += ((lineHeight - (INSERT_PADDING * 0.5f)) * 0.5f) - (buttonPos.height * 0.5f);
      buttonPos.x = position.xMax - (buttonPos.width * 4.0f);
      if (GUI.Button(buttonPos, "x")) {
        RemoveNode(parentProp, index);
        return;
      }

      buttonPos.width = 40.0f;
      buttonPos.x -= buttonPos.width + 5.0f;
      if (GUI.Button(buttonPos, "View")) {
        UnityEngine.Object obj =
          nodeProp.FindPropertyRelative(AssetTree.SerializedNode.VALUE_PROP).objectReferenceValue;
        Selection.activeObject = obj;
        EditorGUIUtility.PingObject(obj);
      }

      float outerIndentWidth = INDENT_AMOUNT * EditorGUI.indentLevel;
      bool hasChildren = childrenCount != 0;

      bool isNodeExpanded = false;
      if (!hasChildren) {
        isNodeExpanded = true;
        SetNodeExpanded(index, nodeProp, isNodeExpanded);
      } else {
        Rect foldoutPosition = position;
        foldoutPosition.width = outerIndentWidth;
        isNodeExpanded = EditorGUI.Foldout(foldoutPosition, IsNodeExpanded(nodeProp), "");
        SetNodeExpanded(index, nodeProp, isNodeExpanded);
      }

      float halfInsertPadding = 0.5f * INSERT_PADDING;
      float lastChildHeight = lineHeight;

      if (isNodeExpanded) {
        EditorGUI.indentLevel++;
        float innerIndentWidth = INDENT_AMOUNT * EditorGUI.indentLevel;


        if (hasChildren) {
          Rect box = position;
          box.x += outerIndentWidth;
          box.width -= outerIndentWidth;
          box.y += lineHeight;

          Rect aboveDropArea = position;
          aboveDropArea.y += lineHeight;
          aboveDropArea.height = INSERT_PADDING;
          aboveDropArea.x += outerIndentWidth;
          aboveDropArea.width -= outerIndentWidth;
          DragAndDropChildren(aboveDropArea, nodeProp, index, null, index + 1);
          //EditorGUI.DrawRect(aboveDropArea, Color.green * 0.5f);

          float totalChildrenHeight = 0.0f;
          int nextIndex = index + 1;
          for (int i = 0; i < childrenCount; i++) {
            if (nextIndex >= nodesProp.arraySize || nextIndex < 0) {
              break;
            }

            if (i == 0) {
              position.y += INSERT_PADDING;
              totalChildrenHeight += INSERT_PADDING;
            }

            position.y += lastChildHeight;
            int newNextIndex;
            lastChildHeight = GetNodeHeight(nextIndex, out newNextIndex);
            totalChildrenHeight += lastChildHeight;

            position.height = lastChildHeight;
            OnNodeGUI(position, nodeProp, index, nextIndex);

            if (nextIndex >= nodesProp.arraySize || nextIndex < 0) {
              break;
            }

            Rect replaceDropArea = position;
            replaceDropArea.y += halfInsertPadding;
            replaceDropArea.height = lineHeight - INSERT_PADDING;
            replaceDropArea.x += innerIndentWidth;
            replaceDropArea.width -= innerIndentWidth;
            SerializedProperty node = nodesProp.GetArrayElementAtIndex(nextIndex);
            DragAndDropChildren(replaceDropArea, nodeProp, index, node, nextIndex);

            nextIndex = newNextIndex;
          }

          box.height = totalChildrenHeight;
          GUI.Box(box, "");
        } else {
          Rect insertDropArea = position;
          insertDropArea.y += lineHeight - halfInsertPadding;
          insertDropArea.height = INSERT_PADDING;
          insertDropArea.x += outerIndentWidth;
          insertDropArea.width -= outerIndentWidth;
          DragAndDropChildren(insertDropArea, nodeProp, index, null, index + 1);
          //EditorGUI.DrawRect(insertDropArea, Color.blue * 0.5f);
        }
        EditorGUI.indentLevel--;
      }

      if (parentProp != null) {
        Rect belowDropArea = position;
        belowDropArea.y += lastChildHeight + INSERT_PADDING - halfInsertPadding;
        belowDropArea.height = INSERT_PADDING;
        belowDropArea.x += outerIndentWidth - INDENT_AMOUNT;
        belowDropArea.width -= outerIndentWidth - INDENT_AMOUNT;
        DragAndDropChildren(belowDropArea, parentProp, parentIndex, null, GetNextIndex(index));
        //EditorGUI.DrawRect(belowDropArea, Color.red * 0.5f);
      }
    }

    private bool IsNodeExpanded(SerializedProperty node) {
      SerializedProperty expandedProp = node.FindPropertyRelative(AssetTree.SerializedNode.EXPANDED_PROP);
      if (expandedProp != null) {
        return expandedProp.boolValue;
      }

      return false;
    }

    private void SetNodeExpanded(int index, SerializedProperty node, bool value) {
      SerializedProperty expandedProp = node.FindPropertyRelative(AssetTree.SerializedNode.EXPANDED_PROP);
      if (expandedProp != null && expandedProp.boolValue != value) {
        expandedProp.boolValue = value;
        indexToNodeHeightMap.Clear();
      }
    }

    private void DragAndDropChildren(Rect dropArea, SerializedProperty parent, int parentIndex, SerializedProperty node, int index) {
      Event currentEvent = Event.current;
      EventType currentEventType = currentEvent.type;

      if (currentEventType == EventType.DragExited) {
        DragAndDrop.PrepareStartDrag();
        DragAndDrop.SetGenericData(DRAG_DATA_IDENTIFIER, null);
        return;
      }

      bool inDropArea = dropArea.Contains(currentEvent.mousePosition);
      if (!inDropArea) {
        return;
      }

      switch (currentEventType) {
        case EventType.MouseDown:
          if (node == null) {
            break;
          }
          SerializedProperty childValueProp = node.FindPropertyRelative(AssetTree.SerializedNode.VALUE_PROP);
          if (childValueProp == null) {
            break;
          }

          DragData dragData = new DragData();
          dragData.originalIndex = index;
          dragData.node = node;
          dragData.originalParentIndex = parentIndex;
          dragData.parent = parent;

          DragAndDrop.PrepareStartDrag();
          DragAndDrop.SetGenericData(DRAG_DATA_IDENTIFIER, dragData);
          Object[] objectReferences = new Object[1]{ childValueProp.objectReferenceValue };
          DragAndDrop.objectReferences = objectReferences;
          DragAndDrop.StartDrag(childValueProp.objectReferenceValue.ToString());

          currentEvent.Use();

          break;
        case EventType.MouseDrag:
          DragData existingDragData = DragAndDrop.GetGenericData(DRAG_DATA_IDENTIFIER) as DragData;
          if (existingDragData != null) {
            currentEvent.Use();
          }

          break;
        case EventType.DragUpdated:
          if (IsDragTargetValid(parent, parentIndex, node, index)) {
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
          } else {
            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
          }

          currentEvent.Use();
          break;
        case EventType.Repaint:
          if (IsDragTargetValid(parent, parentIndex, node, index)) {
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
          } else {
            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
          }

          if (DragAndDrop.visualMode == DragAndDropVisualMode.None ||
              DragAndDrop.visualMode == DragAndDropVisualMode.Rejected)
            break;

          EditorGUI.DrawRect(dropArea, Color.grey);
          break;
        case EventType.DragPerform:
          DragAndDrop.AcceptDrag();

          DragData receivedDragData = DragAndDrop.GetGenericData(DRAG_DATA_IDENTIFIER) as DragData;

          if (receivedDragData != null) {
            if (node != null) {
              break;
            }
            MoveNode(receivedDragData.originalIndex, receivedDragData.originalParentIndex, index, parentIndex);
          } else if (node != null) {
            SetNode(node);
          } else {
            AddNode(parent, index);
          }

          DragAndDrop.SetGenericData(DRAG_DATA_IDENTIFIER, null);
          currentEvent.Use();
          break;
        case EventType.MouseUp:
          DragAndDrop.PrepareStartDrag();
          DragAndDrop.SetGenericData(DRAG_DATA_IDENTIFIER, null);
          break;
      }

    }

    private bool IsDragTargetValid(SerializedProperty parent, int parentIndex, SerializedProperty node, int index) {
      if (DragAndDrop.objectReferences.Length != 1) {
        return false;
      }

      if (!AssetDatabase.Contains(DragAndDrop.objectReferences[0])) {
        return false;
      }

      DragData dragData = DragAndDrop.GetGenericData(DRAG_DATA_IDENTIFIER) as DragData;
      if (dragData != null) {
        if (dragData.parent == null) {
          return false;
        }

        int nextIndex = GetNextIndex(dragData.originalIndex);
        bool replacingNode = node != null;
        if (replacingNode) {
          return false;
        }

        if (nextIndex == -1) {
          nextIndex = int.MaxValue;
        }

        if (index > dragData.originalIndex && index <= nextIndex
            && parentIndex >= dragData.originalParentIndex) {
          return false;
        }
      }

      return true;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      InitializeHeights(property);
      return totalHeight;
    }

    private void InitializeHeights(SerializedProperty property) {
      lineHeight = base.GetPropertyHeight(property, null);
      totalHeight = lineHeight;

      SerializedProperty open = property.FindPropertyRelative(AssetTree.EXPANDED_PROP);
      if (!open.boolValue) {
        return;
      }

      nodesProp = property.FindPropertyRelative(AssetTree.ROOT_PROP);
      if (nodesProp.arraySize == 0 || property.serializedObject.isEditingMultipleObjects) {
        totalHeight += lineHeight;
      } else {
        int nextIndex;
        totalHeight += GetNodeHeight(0, out nextIndex);
      }
    }

    private bool DoesPropertyExist(SerializedObject obj, string propertyPath) {
      return obj.FindProperty(propertyPath) != null;
    }

    private int GetNextIndex(int index) {
      if (index >= nodesProp.arraySize) {
        return -1;
      }

      int resultIndex;

      if (indexToNextIndexMap.TryGetValue(index, out resultIndex)) {
        return resultIndex;
      }

      SerializedProperty node = nodesProp.GetArrayElementAtIndex(index);
      SerializedProperty childrenCount = node.FindPropertyRelative(AssetTree.SerializedNode.CHILDREN_COUNT_PROP);
      resultIndex = index + 1;
      for (int i = 0; i < childrenCount.intValue; i++) {
        resultIndex = GetNextIndex(resultIndex);
      }

      indexToNextIndexMap[index] = resultIndex;

      return resultIndex;
    }

    private float GetNodeHeight(int index, out int nextIndex) {
      if (index >= nodesProp.arraySize) {
        nextIndex = index;
        return 0.0f;
      }

      float result;

      if (indexToNodeHeightMap.TryGetValue(index, out result)) {
        nextIndex = GetNextIndex(index);
        return result;
      }

      SerializedProperty node = nodesProp.GetArrayElementAtIndex(index);
      SerializedProperty childrenCount = node.FindPropertyRelative(AssetTree.SerializedNode.CHILDREN_COUNT_PROP);
      result = lineHeight + INSERT_PADDING + (INSERT_PADDING * 0.5f);


      if (IsNodeExpanded(node)) {
        nextIndex = index + 1;
        if (childrenCount.intValue > 0) {
          result += INSERT_PADDING;
        }
        for (int i = 0; i < childrenCount.intValue; i++) {
          result += GetNodeHeight(nextIndex, out nextIndex);
        }
      } else {
        nextIndex = GetNextIndex(index);
      }

      indexToNodeHeightMap[index] = result;

      return result;
    }

    private void AddNode(SerializedProperty parent, int index) {
      if (parent != null) {
        SerializedProperty parentChildrenCount = parent.FindPropertyRelative(AssetTree.SerializedNode.CHILDREN_COUNT_PROP);
        parentChildrenCount.intValue++;
      }

      nodesProp.InsertArrayElementAtIndex(index);
      SerializedProperty newNode = nodesProp.GetArrayElementAtIndex(index);
      SetNode(newNode);
      newNode.FindPropertyRelative(AssetTree.SerializedNode.CHILDREN_COUNT_PROP).intValue = 0;

      indexToNextIndexMap.Clear();
      indexToNodeHeightMap.Clear();
    }

    private void RemoveNode(SerializedProperty parent, int index) {
      if (parent != null) {
        SerializedProperty parentChildrenCount = parent.FindPropertyRelative(AssetTree.SerializedNode.CHILDREN_COUNT_PROP);
        parentChildrenCount.intValue--;
      }

      int lastChildIndex = GetNextIndex(index) - 1;
      for (int i = lastChildIndex; i >= index; i--) {
        nodesProp.DeleteArrayElementAtIndex(i);
      }

      indexToNextIndexMap.Clear();
      indexToNodeHeightMap.Clear();
    }

    private void SetNode(SerializedProperty child) {
      child.FindPropertyRelative(AssetTree.SerializedNode.VALUE_PROP).objectReferenceValue = DragAndDrop.objectReferences[0];
    }

    private void MoveNode(int sourceIndex, int sourceParentIndex, int targetIndex, int targetParentIndex) {
      // Copy the source node to the target
      //Debug.Log(targetParentIndex);
      SerializedProperty targetParent = nodesProp.GetArrayElementAtIndex(targetParentIndex);
      SerializedProperty targetParentChildCount = targetParent.FindPropertyRelative(AssetTree.SerializedNode.CHILDREN_COUNT_PROP);
      targetParentChildCount.intValue++;

      nodesProp.InsertArrayElementAtIndex(targetIndex);
      SerializedProperty newNode = nodesProp.GetArrayElementAtIndex(targetIndex);
      SerializedProperty newValue = newNode.FindPropertyRelative(AssetTree.SerializedNode.VALUE_PROP);
      SerializedProperty newChildCount = newNode.FindPropertyRelative(AssetTree.SerializedNode.CHILDREN_COUNT_PROP);
      //Debug.Log(sourceIndex + " : " + targetIndex);
      if (sourceIndex >= targetIndex) {
        sourceIndex++;
      }
      if (sourceParentIndex >= targetIndex) {
        sourceParentIndex++;
      }

      SerializedProperty sourceNode = nodesProp.GetArrayElementAtIndex(sourceIndex);
      SerializedProperty sourceValue = sourceNode.FindPropertyRelative(AssetTree.SerializedNode.VALUE_PROP);
      SerializedProperty sourceChildCount = sourceNode.FindPropertyRelative(AssetTree.SerializedNode.CHILDREN_COUNT_PROP);

      newValue.objectReferenceValue = sourceValue.objectReferenceValue;
      newChildCount.intValue = sourceChildCount.intValue;
      SetNodeExpanded(targetIndex, newNode, IsNodeExpanded(sourceNode));

      // Copy all of the source nodes children to the target.
      int lastChildIndex = GetNextIndex(sourceIndex) - 1;
      int counter = 0;
      for (int i = sourceIndex + 1; i <= lastChildIndex; i++) {
        counter++;
        int childIndex = targetIndex + counter;

        nodesProp.InsertArrayElementAtIndex(childIndex);
        newNode = nodesProp.GetArrayElementAtIndex(childIndex);
        newValue = newNode.FindPropertyRelative(AssetTree.SerializedNode.VALUE_PROP);
        newChildCount = newNode.FindPropertyRelative(AssetTree.SerializedNode.CHILDREN_COUNT_PROP);

        if (i >= childIndex) {
          i++;
        }

        if (lastChildIndex >= childIndex) {
          lastChildIndex++;
        }

        if (sourceIndex >= childIndex) {
          sourceIndex++;
        }

        if (sourceParentIndex >= childIndex) {
          sourceParentIndex++;
        }

        SerializedProperty sourceNextNode = nodesProp.GetArrayElementAtIndex(i);
        SerializedProperty sourceNextValue = sourceNextNode.FindPropertyRelative(AssetTree.SerializedNode.VALUE_PROP);
        SerializedProperty sourceNextChildCount = sourceNextNode.FindPropertyRelative(AssetTree.SerializedNode.CHILDREN_COUNT_PROP);
        newValue.objectReferenceValue = sourceNextValue.objectReferenceValue;
        newChildCount.intValue = sourceNextChildCount.intValue;
        SetNodeExpanded(childIndex, newNode, IsNodeExpanded(sourceNextNode));
      }

      // Now we remove the source node.
      SerializedProperty sourceParent = nodesProp.GetArrayElementAtIndex(sourceParentIndex);
      SerializedProperty sourceParentChildCount = sourceParent.FindPropertyRelative(AssetTree.SerializedNode.CHILDREN_COUNT_PROP);
      sourceParentChildCount.intValue--;

      for (int i = lastChildIndex; i >= sourceIndex; i--) {
        nodesProp.DeleteArrayElementAtIndex(i);
      }

      indexToNextIndexMap.Clear();
      indexToNodeHeightMap.Clear();
    }

    private void OnUndoRedo() {
      indexToNodeHeightMap.Clear();
      indexToNextIndexMap.Clear();
    }
  }
}
