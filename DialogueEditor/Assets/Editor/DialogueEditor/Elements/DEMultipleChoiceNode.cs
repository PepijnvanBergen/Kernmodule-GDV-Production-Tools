using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;


namespace DE.Elements
{
    using Data.Save;
    using Enumerations;
    using Utilities;
    using Windows;
    public class DEMultipleChoiceNode : DENode
    {
        public override void Initialize(string _nodeName, Vector2 _position, DEGraph _graph)
        {
            base.Initialize(_nodeName, _position, _graph);
            dialogueType = DEDialogueType.MultipleChoice;

            DEChoiceSaveData choiceData = new DEChoiceSaveData()
            {
                text = "Next Dialogue"
            };

            choices.Add(choiceData);
        }
        public override void Draw()
        {
            base.Draw();

            //Main Container

            Button addChoiceButton = DEElementUtility.CreateButton("Add Choice", () =>
            {
                DEChoiceSaveData choiceData = new DEChoiceSaveData()
                {
                    text = "Next Dialogue"
                };
                choices.Add(choiceData);
                Port choicePort = CreateChoicePort(choiceData);
                outputContainer.Add(choicePort);
            });
            addChoiceButton.AddToClassList("de-node__button");
            mainContainer.Insert(1, addChoiceButton);

            //Output container

            foreach (DEChoiceSaveData choice in choices)
            {
                Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }
            RefreshExpandedState();
        }
        private Port CreateChoicePort(object _userData)
        {
            Port choicePort = this.CreatePort();
            choicePort.userData = _userData;

            DEChoiceSaveData choiceData = (DEChoiceSaveData) _userData;

            Button deleteChoiceButton = DEElementUtility.CreateButton("Delete", () =>
            {
                if (choices.Count == 1)
                {
                    return;
                }
                if (choicePort.connected)
                {
                    graph.DeleteElements(choicePort.connections);
                }
                choices.Remove(choiceData);
                graph.RemoveElement(choicePort);
            });
            
            deleteChoiceButton.AddToClassList("de-node__button");

            TextField choiceTextField = DEElementUtility.CreateTextField(choiceData.text, null, callback =>
            {
                choiceData.text = callback.newValue;
            });
            choiceTextField.AddCLasses("de-node__textfield", "de-node__choice-textfield", "de-node__textfield__hidden");

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            //outputContainer.Add(choicePort);
            return choicePort;
        }

    }
}