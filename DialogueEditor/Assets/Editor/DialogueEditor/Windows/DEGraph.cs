using System;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

namespace DE.Windows
{
    using Elements;
    using Enumerations;
    using Utilities;
    using Data.Error;
    using Data.Save;
    public class DEGraph : GraphView
    {
        private DEWindow window;

        private SerializableDictionary<string, DENodeErrorData> ungroupedNodes;
        private SerializableDictionary<string, DEGroupErrorData> groups;
        private SerializableDictionary<Group, SerializableDictionary<string, DENodeErrorData>> groupedNodes;

        private int nameErrorsAmount;

        public int nameErrorsAmountP
        {
            get
            {
                return nameErrorsAmount;
            }

            set
            {
                nameErrorsAmount = value;

                if (nameErrorsAmount == 0)
                {
                    window.EnableSaving();
                }

                if (nameErrorsAmount == 1)
                {
                    window.DisableSaving();
                }
            }
        }
        public DEGraph(DEWindow _window)
        {
            ungroupedNodes = new SerializableDictionary<string, DENodeErrorData>();
            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, DENodeErrorData>>();
            groups = new SerializableDictionary<string, DEGroupErrorData>();
            window = _window;

            AddGridBackGround();
            AddStyles();
            AddManipulators();
            OnElementsDeleted();
            OnGroupRenamed();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGraphViewChanged();
        }

        public override List<Port> GetCompatiblePorts(Port _startPort, NodeAdapter _nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if(_startPort == port)
                {
                    return;
                }
                if(_startPort.node == port.node)
                {
                    return;
                }
                if(_startPort.direction == port.direction)
                {
                    return;
                }
                compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }
        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Add Node (Single Choice)", DEDialogueType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Multiple Choice)", DEDialogueType.MultipleChoice));

            this.AddManipulator(CreateGroupedContextualMenu());
        }
        private IManipulator CreateGroupedContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("DialogueGroup", actionEvent.eventInfo.localMousePosition)))
                );
            return contextualMenuManipulator;
        }
        private IManipulator CreateNodeContextualMenu(string _actionTitle, DEDialogueType _dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(_actionTitle, actionEvent => AddElement(CreateNode("DialogueName",_dialogueType, actionEvent.eventInfo.localMousePosition)))
                );
            return contextualMenuManipulator;
        }
        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DENode))
                    {
                        continue;
                    }
                    DENode node = (DENode)element;

                    RemoveGroupedNode(node, group);
                    AddUngroupedNode(node);
                }
            };
        }
        public DEGroup CreateGroup(string _title,  Vector2 _localMousePosition)
        {
            DEGroup group = new DEGroup(_title, _localMousePosition);
            AddGroup(group);

            return group;
        }
        public DENode CreateNode(string _nodeName, DEDialogueType _dialogueType, Vector2 _position, bool _shouldDraw = true)
        {
            Type nodeType = Type.GetType($"DE.Elements.DE{_dialogueType}Node");
            DENode node = (DENode)Activator.CreateInstance(nodeType);

            node.Initialize(_nodeName, _position, this);

            if (_shouldDraw)
            {
                node.Draw();
            }

            AddElement(node);
            AddUngroupedNode(node);
            return node;
        }
        public void AddUngroupedNode(DENode _node)
        {
            string nodeName = _node.nodeName;

            if (!ungroupedNodes.ContainsKey(nodeName))
            {
                DENodeErrorData nodeErrorData = new DENodeErrorData();
                nodeErrorData.nodes.Add(_node);
                ungroupedNodes.Add(nodeName, nodeErrorData);
                return;
            }
            ungroupedNodes[nodeName].nodes.Add(_node);

            List<DENode> ungroupedNodesList = ungroupedNodes[nodeName].nodes;
            Color errorColor = ungroupedNodes[nodeName].errorData.color;

            _node.SetErrorColor(errorColor);

            if (ungroupedNodesList.Count == 2)
            {
                ++nameErrorsAmountP;

                ungroupedNodesList[0].SetErrorColor(errorColor);
            }
        }
        public void RemoveUngroupedNode(DENode _node)
        {
            string nodeName = _node.nodeName;
            List<DENode> ungroupedNodesList = ungroupedNodes[nodeName].nodes;

            ungroupedNodesList.Remove(_node);
            _node.ResetColor();

            if (ungroupedNodesList.Count == 1)
            {
                --nameErrorsAmountP;
                ungroupedNodesList[0].ResetColor();
                return;
            }
            if (ungroupedNodesList.Count == 0)
            {
                ungroupedNodes.Remove(nodeName);
            }
        }
        public void AddGroupedNode(DENode _node, DEGroup _group)
        {
            string nodeName = _node.nodeName;

            _node.group = _group;

            if (!groupedNodes.ContainsKey(_group))
            {
                groupedNodes.Add(_group, new SerializableDictionary<string, DENodeErrorData>());
            }
            if (!groupedNodes[_group].ContainsKey(nodeName))
            {
                DENodeErrorData nodesErrorData = new DENodeErrorData();
                nodesErrorData.nodes.Add(_node);
                groupedNodes[_group].Add(nodeName, nodesErrorData);
                return;
            }

            List<DENode> groupedNodesList = groupedNodes[_group][nodeName].nodes;
            groupedNodesList.Add(_node);
            Color errorColor = groupedNodes[_group][nodeName].errorData.color;
            _node.SetErrorColor(errorColor);

            if(groupedNodesList.Count == 2)
            {
                ++nameErrorsAmountP;
                groupedNodesList[0].SetErrorColor(errorColor);
            }
        }
        public void RemoveGroupedNode(DENode _node, Group _group)
        {
            string nodeName = _node.nodeName;
            _node.group = null;
            List<DENode> groupedNodesList = groupedNodes[_group][nodeName].nodes;
            groupedNodesList.Remove(_node);
            _node.ResetColor();
            if (groupedNodes.Count == 1)
            {
                --nameErrorsAmountP;
                groupedNodesList[0].ResetColor();
                return;
            }
            if (groupedNodesList.Count == 0)
            {
                groupedNodes[_group].Remove(nodeName);
                if (groupedNodes[_group].Count == 0)
                {
                    groupedNodes.Remove(_group);
                }
            }
        }
        private void AddGroup(DEGroup _group)
        {
            string groupName = _group.title;
            if (!groups.ContainsKey(groupName))
            {
                DEGroupErrorData groupErrorData = new DEGroupErrorData();
                groupErrorData.groups.Add(_group);
                groups.Add(groupName, groupErrorData);
                return;
            }
            List<DEGroup> groupsList = groups[groupName].groups;
            groupsList.Add(_group);

            Color errorColor = groups[groupName].errorData.color;
            _group.SetErrorStyle(errorColor);

            if(groupsList.Count == 2)
            {
                ++nameErrorsAmountP;

                groupsList[0].SetErrorStyle(errorColor);
            }
        }
        private void RemoveGroup(DEGroup _group)
        {
            string oldGroupName = _group.oldTitle;
            List<DEGroup> groupsList = groups[oldGroupName].groups;

            groupsList.Remove(_group);
            _group.ResetStyle();

            if(groupsList.Count == 1)
            {
                --nameErrorsAmountP;

                groupsList[0].ResetStyle();
            }
            if(groupsList.Count == 0)
            {
                groups.Remove(oldGroupName);
            }
        }
        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DENode))
                    {
                        continue;
                    }

                    DEGroup nodeGroup = (DEGroup)group;
                    DENode node = (DENode)element;
                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, nodeGroup);
                }
            };
        }
        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, AskUser) =>
            {
                Type groupType = typeof(DEGroup);
                Type edgeType = typeof(Edge);

                List<Edge> edgesToDelete = new List<Edge>();
                List<DEGroup> groupsToDelete = new List<DEGroup>();
                List<DENode> nodesToDelete = new List<DENode>();

                foreach (GraphElement element in selection)
                {
                    if (element is DENode node)
                    {
                        nodesToDelete.Add(node);
                        continue;
                    }
                    if (element.GetType() == edgeType)
                    {
                        Edge edge = (Edge)element;
                        edgesToDelete.Add(edge);
                        continue;
                    }
                    if (element.GetType() != groupType)
                    {
                        continue;
                    }

                    DEGroup group = (DEGroup)element;

                    groupsToDelete.Add(group);
                    RemoveGroup(group);
                }
                foreach (DEGroup group in groupsToDelete)
                {
                    RemoveElement(group);
                }

                DeleteElements(edgesToDelete);

                foreach (DENode node in nodesToDelete)
                {
                    if (node.group != null)
                    {
                        node.group.RemoveElement(node);
                    }
                    RemoveUngroupedNode(node); //the dictionary can get empty real fast! Or it doesn't remove the node!
                    node.DisconnectAllPorts();
                    RemoveElement(node);
                }
            };
        }
        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DEGroup deGroup = (DEGroup)group;

                deGroup.title = newTitle;
                if (string.IsNullOrEmpty(deGroup.title))
                {
                    if (!string.IsNullOrEmpty(deGroup.oldTitle))
                    {
                        ++nameErrorsAmountP;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(deGroup.oldTitle))
                    {
                        --nameErrorsAmountP;
                    }
                }

                RemoveGroup(deGroup);
                deGroup.oldTitle = newTitle;
                AddGroup(deGroup);
            };
        }
        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if(changes.edgesToCreate != null)
                {
                    foreach(Edge edge in changes.edgesToCreate)
                    {
                        DENode nextNode = (DENode) edge.input.node;

                        DEChoiceSaveData choiceData = (DEChoiceSaveData) edge.output.userData;

                        choiceData.nodeID = nextNode.id;
                    }
                }
                if(changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);
                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if(element.GetType() != edgeType)
                        {
                            continue;
                        }
                        Edge edge = (Edge)element;
                        DEChoiceSaveData choideData = (DEChoiceSaveData) edge.output.userData;
                        choideData.nodeID = "";
                    }
                }
                return changes;
            };
        }
        private void AddGridBackGround()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            
            Insert(0, gridBackground);
        }
        private void AddStyles()
        {
            this.AddStyleSheets("DialogueEditor/DEGraphStyles.uss", "DialogueEditor/DENodeStyles.uss");
        }
        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));

            groups.Clear();
            groupedNodes.Clear();
            ungroupedNodes.Clear();

            nameErrorsAmountP = 0;
        }
    }
}