using UnityEditor;
using UnityEngine.UIElements;


namespace DE.Utilities
{
    public static class DEStyleUtility
    {
        public static VisualElement AddStyleSheets(this VisualElement _visualElement, params string[] _styleSheetNames)
        {
            foreach(string styleSheetName in _styleSheetNames)
            {
                StyleSheet styleSheet = (StyleSheet) EditorGUIUtility.Load(styleSheetName);

                _visualElement.styleSheets.Add(styleSheet);
            }
            return _visualElement;
        }
        public static VisualElement AddCLasses(this VisualElement _visualElement, params string[] _classNames)
        {
            foreach(string className in _classNames)
            {
                _visualElement.AddToClassList(className);
            }
            return _visualElement;
        }
    }
}