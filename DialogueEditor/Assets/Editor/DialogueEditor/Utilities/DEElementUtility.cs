using System;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace DE.Utilities
{
    using Elements;
    public static class DEElementUtility
    {
        public static Button CreateButton(string _text, Action _onClick = null)
        {
            Button button = new Button(_onClick)
            {
                text = _text,
            };
            return button;
        }
        public static Foldout CreateFoldout(string _title, bool _collapse = false)
        {
            Foldout foldout = new Foldout()
            {
                text = _title,
                value = !_collapse
            };
            return foldout;
        }
        public static Port CreatePort(this DENode _node, string _portName =  "", Orientation _orientation = Orientation.Horizontal, Direction _direction = Direction.Output, Port.Capacity _capacity = Port.Capacity.Single)
        {
            Port port = _node.InstantiatePort(_orientation, _direction, _capacity, typeof(bool));
            port.portName = _portName;
            return port;
        }
        public static TextField CreateTextField(string _value = null, string _label = null, EventCallback<ChangeEvent<string>> _onValueChanged = null)
        {
            TextField textField = new TextField()
            {
                value = _value,
                label = _label
            };
            if(_onValueChanged != null)
            {
                textField.RegisterValueChangedCallback(_onValueChanged);
            }

            return textField;
        }
        public static TextField CreateTextArea(string _value = null, string _label = null, EventCallback<ChangeEvent<string>> _onValueChanged = null)
        {
            TextField textArea = CreateTextField(_value, _label, _onValueChanged);

            textArea.multiline = true;

            return textArea;
        }
    }
}