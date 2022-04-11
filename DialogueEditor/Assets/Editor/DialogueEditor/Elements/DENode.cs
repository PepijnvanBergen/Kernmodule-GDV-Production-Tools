using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace DE.Elements
{
    using Enumerations;
    using Utilities;
    using Windows;
    using Data.Save;

    public class DENode : Node
    {
        public string id { get; set; }
        public string nodeName { get; set; }
        public List<DEChoiceSaveData> choices { get; set; }
        public string text { get; set; }
        public DEDialogueType dialogueType { get; set; }
        public DEGroup group { get; set; }
        protected DEGraph graph;
        private Color defaultBackgroundColor; //Make sure this is the same color as the background color in the stylesheets!

        public virtual void Initialize(string _nodeName, Vector2 _position, DEGraph _graph)
        {
            id = Guid.NewGuid().ToString();
            nodeName = _nodeName;
            choices = new List<DEChoiceSaveData>();
            text = "Dialogue text";

            SetPosition(new Rect(_position, Vector2.zero));
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);
            graph = _graph;

            mainContainer.AddToClassList("de-node__main-container");
            extensionContainer.AddToClassList("de-node__extension-container");
        }
        public virtual void Draw()
        {
            //TitleContainer
            TextField dialogueTitleTextField = DEElementUtility.CreateTextField(nodeName, null, callBack =>
            {
                TextField target = (TextField) callBack.target;
                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(nodeName))
                    {
                        ++graph.nameErrorsAmountP;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(nodeName))
                    {
                        --graph.nameErrorsAmountP;
                    }
                }
                if(group == null)
                {
                    graph.RemoveUngroupedNode(this);
                    nodeName = callBack.newValue;
                    graph.AddUngroupedNode(this);
                    return;
                }
                
                DEGroup currentGroup = group;
                
                graph.RemoveGroupedNode(this, group);
                nodeName = callBack.newValue;
                graph.AddGroupedNode(this, currentGroup);
            });

            dialogueTitleTextField.AddCLasses("de-node__textfield", "de-node__textfield__hidden", "de-node__filename-textfield");
            titleContainer.Insert(0, dialogueTitleTextField);

            //Input Container

            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);

            //Extensions Container

            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("de-node__custom-data-container");

            Foldout textFoldout = DEElementUtility.CreateFoldout("Dialogue text");

            TextField textTextField = DEElementUtility.CreateTextArea(text, null, callback =>
            {
                text = callback.newValue;
            });
            textTextField.AddCLasses("de-node__textfield", "de-node__qoute-textfield");

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
        }
        public void DisconnectAllPorts()
        {
            DisconnectPorts(inputContainer);
            DisconnectPorts(outputContainer);
        }
        private void DisconnectPorts(VisualElement _container)
        {
            foreach(Port port in _container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }
                graph.DeleteElements(port.connections);
            }
        }
        public bool IsStartingNode()
        {
            Port inputPort = (Port) inputContainer.Children().First();
            return !inputPort.connected;
        }
        public void SetErrorColor(Color _color)
        {
            mainContainer.style.backgroundColor = _color;
        }
        public void ResetColor()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
    }
}