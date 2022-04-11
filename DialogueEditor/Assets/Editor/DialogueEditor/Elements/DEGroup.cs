using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace DE.Elements
{
    public class DEGroup : Group
    {
        public string id { get; set; }
        public string oldTitle;
        private Color defaultColor;
        private float defaultBorderWidth;

        public DEGroup(string _groupTitle, Vector2 _position)
        {
            id = Guid.NewGuid().ToString();
            title = _groupTitle;
            oldTitle = _groupTitle;
            SetPosition(new Rect(_position, Vector2.zero));

            defaultColor = contentContainer.style.borderBottomColor.value;
            defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
        }
        public void SetErrorStyle(Color _color)
        {
            contentContainer.style.borderBottomColor = _color;
            contentContainer.style.borderBottomWidth = 3f;
        }
        public void ResetStyle()
        {
            contentContainer.style.borderBottomColor = defaultColor;
            contentContainer.style.borderBottomWidth = defaultBorderWidth;
        }
    }
}