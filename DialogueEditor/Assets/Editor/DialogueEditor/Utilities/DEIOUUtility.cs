using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DE.Utilities
{
    using Data;
    using Data.Save;
    using Elements;
    using ScriptableObjects;
    using Windows;

    public static class DEIOUUtility
    {
        private static DEGraph graph;

        private static string graphFileName;
        private static string containerFolderPath;

        private static List<DENode> nodes;
        private static List<DEGroup> groups;

        private static Dictionary<string, DEDialogueGroupSO> createdDialogueGroups;
        private static Dictionary<string, DEDialogueSO> createdDialogues;

        private static Dictionary<string, DEGroup> loadedGroups;
        private static Dictionary<string, DENode> loadedNodes;

        public static void Initialize(DEGraph _graph, string _graphName)
        {
            graph = _graph;

            graphFileName = _graphName;
            containerFolderPath = $"Assets/DialogueEditor/Dialogues/{_graphName}";

            nodes = new List<DENode>();
            groups = new List<DEGroup>();

            createdDialogueGroups = new Dictionary<string, DEDialogueGroupSO>();
            createdDialogues = new Dictionary<string, DEDialogueSO>();

            loadedGroups = new Dictionary<string, DEGroup>();
            loadedNodes = new Dictionary<string, DENode>();
        }
        public static void Save()
        {
            CreateDefaultFolders();

            GetElementsFromGraph();

            DEGraphSaveDataSO graphData = CreateAsset<DEGraphSaveDataSO>("Assets/Editor/DialogueEditor/Graphs", $"{graphFileName}Graph");

            graphData.Initialize(graphFileName);

            DEDialogueContainerSO dialogueContainer = CreateAsset<DEDialogueContainerSO>(containerFolderPath, graphFileName);

            dialogueContainer.Initialize(graphFileName);

            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);

            SaveAsset(graphData);
            SaveAsset(dialogueContainer);

            //SavetoJson
            SaveToJson<DENodeSaveData>(graphData.nodes, graphFileName + "nodes");
            SaveToJson<DEGroupSaveData>(graphData.groups, graphFileName + "groups");
        }

        private static void SaveToJson<T>(List<T> _toSave, string _filename)
        {
            string content = JsonHelper.ToJson<T>(_toSave.ToArray());
            WriteJsonFile(GetPath(_filename), content);
            Debug.Log(GetPath(_filename));
        }

        private static void WriteJsonFile(string _path, string content)
        {
            FileStream fileStream = new FileStream(_path, FileMode.Create);
            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                writer.Write(content);
            }
        }
        private static string GetPath(string _fileName)
        {
            return Application.persistentDataPath + "/" + _fileName;
        }
        private static void SaveGroups(DEGraphSaveDataSO _graphData, DEDialogueContainerSO _dialogueContainer)
        {
            List<string> groupNames = new List<string>();

            foreach (DEGroup group in groups)
            {
                SaveGroupToGraph(group, _graphData);
                SaveGroupToScriptableObject(group, _dialogueContainer);

                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, _graphData);
        }
        private static void SaveGroupToGraph(DEGroup _group, DEGraphSaveDataSO _graphData)
        {
            DEGroupSaveData groupData = new DEGroupSaveData()
            {
                id = _group.id,
                name = _group.title,
                position = _group.GetPosition().position
            };

            _graphData.groups.Add(groupData);
        }
        private static void SaveGroupToScriptableObject(DEGroup _group, DEDialogueContainerSO _dialogueContainer)
        {
            string groupName = _group.title;

            CreateFolder($"{containerFolderPath}/Groups", groupName);
            CreateFolder($"{containerFolderPath}/Groups/{groupName}", "Dialogues");

            DEDialogueGroupSO dialogueGroup = CreateAsset<DEDialogueGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);

            dialogueGroup.Initialize(groupName);

            createdDialogueGroups.Add(_group.id, dialogueGroup);

            _dialogueContainer.dialogueGroups.Add(dialogueGroup, new List<DEDialogueSO>());

            SaveAsset(dialogueGroup);
        }
        private static void UpdateOldGroups(List<string> _currentGroupNames, DEGraphSaveDataSO _graphData)
        {
            if (_graphData.oldGroupNames != null && _graphData.oldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = _graphData.oldGroupNames.Except(_currentGroupNames).ToList();

                foreach (string groupToRemove in groupsToRemove)
                {
                    RemoveFolder($"{containerFolderPath}/Groups/{groupToRemove}");
                }
            }

            _graphData.oldGroupNames = new List<string>(_currentGroupNames);
        }
        private static void SaveNodes(DEGraphSaveDataSO _graphData, DEDialogueContainerSO _dialogueContainer)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();

            foreach (DENode node in nodes)
            {
                SaveNodeToGraph(node, _graphData);
                SaveNodeToScriptableObject(node, _dialogueContainer);

                if (node.group != null)
                {
                    groupedNodeNames.AddItem(node.group.title, node.nodeName);

                    continue;
                }

                ungroupedNodeNames.Add(node.nodeName);
            }

            UpdateDialoguesChoicesConnections();

            UpdateOldGroupedNodes(groupedNodeNames, _graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, _graphData);
        }
        private static void SaveNodeToGraph(DENode _node, DEGraphSaveDataSO _graphData)
        {
            List<DEChoiceSaveData> choices = CloneNodeChoices(_node.choices);

            DENodeSaveData nodeData = new DENodeSaveData()
            {
                id = _node.id,
                name = _node.nodeName,
                choices = choices,
                text = _node.text,
                groupID = _node.group?.id,
                dialogueType = _node.dialogueType,
                position = _node.GetPosition().position
            };

            _graphData.nodes.Add(nodeData);
        }
        private static void SaveNodeToScriptableObject(DENode _node, DEDialogueContainerSO _dialogueContainer)
        {
            DEDialogueSO dialogue;

            if (_node.group != null)
            {
                dialogue = CreateAsset<DEDialogueSO>($"{containerFolderPath}/Groups/{_node.group.title}/Dialogues", _node.nodeName);

                _dialogueContainer.dialogueGroups.AddItem(createdDialogueGroups[_node.group.id], dialogue);
            }
            else
            {
                dialogue = CreateAsset<DEDialogueSO>($"{containerFolderPath}/Global/Dialogues", _node.nodeName);

                _dialogueContainer.unGroupedDialogues.Add(dialogue);
            }

            dialogue.Initialize(
                _node.nodeName,
                _node.text,
                ConvertNodeChoicesToDialogueChoices(_node.choices),
                _node.dialogueType,
                _node.IsStartingNode()
            );

            createdDialogues.Add(_node.id, dialogue);

            SaveAsset(dialogue);
        }
        private static List<DEDialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<DEChoiceSaveData> _nodeChoices)
        {
            List<DEDialogueChoiceData> dialogueChoices = new List<DEDialogueChoiceData>();

            foreach (DEChoiceSaveData nodeChoice in _nodeChoices)
            {
                DEDialogueChoiceData choiceData = new DEDialogueChoiceData()
                {
                    text = nodeChoice.text
                };

                dialogueChoices.Add(choiceData);
            }

            return dialogueChoices;
        }
        private static void UpdateDialoguesChoicesConnections()
        {
            foreach (DENode node in nodes)
            {
                DEDialogueSO dialogue = createdDialogues[node.id];

                for (int choiceIndex = 0; choiceIndex < node.choices.Count; ++choiceIndex)
                {
                    DEChoiceSaveData nodeChoice = node.choices[choiceIndex];

                    if (string.IsNullOrEmpty(nodeChoice.nodeID))
                    {
                        continue;
                    }

                    dialogue.choices[choiceIndex].nextDialogue = createdDialogues[nodeChoice.nodeID];

                    SaveAsset(dialogue);
                }
            }
        }
        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> _currentGroupedNodeNames, DEGraphSaveDataSO _graphData)
        {
            if (_graphData.oldGroupedNodeNames != null && _graphData.oldGroupedNodeNames.Count != 0)
            {
                foreach (KeyValuePair<string, List<string>> oldGroupedNode in _graphData.oldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();

                    if (_currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                    {
                        nodesToRemove = oldGroupedNode.Value.Except(_currentGroupedNodeNames[oldGroupedNode.Key]).ToList();
                    }

                    foreach (string nodeToRemove in nodesToRemove)
                    {
                        RemoveAsset($"{containerFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues", nodeToRemove);
                    }
                }
            }

            _graphData.oldGroupedNodeNames = new SerializableDictionary<string, List<string>>(_currentGroupedNodeNames);
        }
        private static void UpdateOldUngroupedNodes(List<string> _currentUngroupedNodeNames, DEGraphSaveDataSO _graphData)
        {
            if (_graphData.oldUngroupedNodeNames != null && _graphData.oldUngroupedNodeNames.Count != 0)
            {
                List<string> nodesToRemove = _graphData.oldUngroupedNodeNames.Except(_currentUngroupedNodeNames).ToList();

                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/Global/Dialogues", nodeToRemove);
                }
            }

            _graphData.oldUngroupedNodeNames = new List<string>(_currentUngroupedNodeNames);
        }
        public static void Load()
        {
            DEGraphSaveDataSO graphData = LoadAsset<DEGraphSaveDataSO>("Assets/Editor/DialogueEditor/Graphs", graphFileName);

            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Could not find the file!",
                    "The file at the following path could not be found:\n\n" +
                    $"\"Assets/Editor/DialogueEditor/Graphs/{graphFileName}\".\n\n" +
                    "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                    "Oke"
                );

                return;
            }
            LoadGroups(graphData.groups);
            LoadNodes(graphData.nodes);
            LoadNodesConnections();
        }
        private static void LoadGroups(List<DEGroupSaveData> _groups)
        {
            foreach (DEGroupSaveData groupData in _groups)
            {
                DEGroup group = graph.CreateGroup(groupData.name, groupData.position);

                group.id = groupData.id;

                loadedGroups.Add(group.id, group);
            }
        }
        private static void LoadNodes(List<DENodeSaveData> _nodes)
        {
            foreach (DENodeSaveData nodeData in _nodes)
            {
                List<DEChoiceSaveData> choices = CloneNodeChoices(nodeData.choices);

                DENode node = graph.CreateNode(nodeData.name, nodeData.dialogueType, nodeData.position, false);

                node.id = nodeData.id;
                node.choices = choices;
                node.text = nodeData.text;

                node.Draw();

                graph.AddElement(node);

                loadedNodes.Add(node.id, node);

                if (string.IsNullOrEmpty(nodeData.groupID))
                {
                    continue;
                }

                DEGroup group = loadedGroups[nodeData.groupID];

                node.group = group;

                group.AddElement(node);
            }
        }
        private static void LoadNodesConnections()
        {
            foreach (KeyValuePair<string, DENode> loadedNode in loadedNodes)
            {
                foreach (Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    DEChoiceSaveData choiceData = (DEChoiceSaveData)choicePort.userData;

                    if (string.IsNullOrEmpty(choiceData.nodeID))
                    {
                        continue;
                    }

                    DENode nextNode = loadedNodes[choiceData.nodeID];

                    Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();

                    Edge edge = choicePort.ConnectTo(nextNodeInputPort);

                    graph.AddElement(edge);

                    loadedNode.Value.RefreshPorts();
                }
            }
        }
        private static void CreateDefaultFolders()
        {
            CreateFolder("Assets/Editor/DialogueEditor", "Graphs");

            CreateFolder("Assets", "DialogueEditor");
            CreateFolder("Assets/DialogueEditor", "Dialogues");

            CreateFolder("Assets/DialogueEditor/Dialogues", graphFileName);
            CreateFolder(containerFolderPath, "Global");
            CreateFolder(containerFolderPath, "Groups");
            CreateFolder($"{containerFolderPath}/Global", "Dialogues");
            
            //Create Json folders
            //CreateFolder("Assets/DialogueEditor/Json", graphFileName);
        }
        private static void GetElementsFromGraph()
        {
            Type groupType = typeof(DEGroup);

            graph.graphElements.ForEach(graphElement =>
            {
                if (graphElement is DENode node)
                {
                    nodes.Add(node);

                    return;
                }

                if (graphElement.GetType() == groupType)
                {
                    DEGroup group = (DEGroup)graphElement;

                    groups.Add(group);

                    return;
                }
            });
        }
        public static void CreateFolder(string _parentFolderPath, string _newFolderName)
        {
            if (AssetDatabase.IsValidFolder($"{_parentFolderPath}/{_newFolderName}"))
            {
                return;
            }

            AssetDatabase.CreateFolder(_parentFolderPath, _newFolderName);
        }
        public static void RemoveFolder(string _path)
        {
            FileUtil.DeleteFileOrDirectory($"{_path}.meta");
            FileUtil.DeleteFileOrDirectory($"{_path}/");
        }
        public static T CreateAsset<T>(string _path, string _assetName) where T : ScriptableObject
        {
            string fullPath = $"{_path}/{_assetName}.asset";

            T asset = LoadAsset<T>(_path, _assetName);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();

                AssetDatabase.CreateAsset(asset, fullPath);
            }

            return asset;
        }
        public static T LoadAsset<T>(string _path, string _assetName) where T : ScriptableObject
        {
            string fullPath = $"{_path}/{_assetName}.asset";

            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }
        public static void SaveAsset(UnityEngine.Object _asset)
        {
            EditorUtility.SetDirty(_asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        public static void RemoveAsset(string _path, string _assetName)
        {
            AssetDatabase.DeleteAsset($"{_path}/{_assetName}.asset");
        }
        private static List<DEChoiceSaveData> CloneNodeChoices(List<DEChoiceSaveData> _nodeChoices)
        {
            List<DEChoiceSaveData> choices = new List<DEChoiceSaveData>();

            foreach (DEChoiceSaveData choice in _nodeChoices)
            {
                DEChoiceSaveData choiceData = new DEChoiceSaveData()
                {
                    text = choice.text,
                    nodeID = choice.nodeID
                };

                choices.Add(choiceData);
            }

            return choices;
        }
    }
}