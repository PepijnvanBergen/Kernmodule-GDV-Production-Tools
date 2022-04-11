using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace DE.Elements
{
    using Data.Save;
    using Enumerations;
    using Utilities;
    using Windows;
    public class DESingleChoiceNode : DENode
    {
        public override void Initialize(string _nodeName, Vector2 _position, DEGraph _graph)
        {
            base.Initialize(_nodeName, _position, _graph);
            dialogueType = DEDialogueType.SingleChoice;

            DEChoiceSaveData choiceData = new DEChoiceSaveData()
            {
                text = "Next Dialogue"
            };
            choices.Add(choiceData);
        }
        public override void Draw()
        {
            base.Draw();
            
            foreach(DEChoiceSaveData choice in choices)
            {
                Port choicePort = this.CreatePort(choice.text);

                choicePort.userData = choice;

                outputContainer.Add(choicePort);
            }
            RefreshExpandedState();
        }
    }
}