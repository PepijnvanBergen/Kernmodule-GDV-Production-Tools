using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using UnityEditor.UIElements; //als ik deze gebruik dan is er kans dat Unity vast loopt.

namespace DE.Windows
{
    using Utilities;
    public class DEWindow : EditorWindow
    {
        private DEGraph graph;
        private readonly string defaultFileName = "DialogueFileName";
        private Button saveButton;
        private static TextField fileNameTextField;

        [MenuItem("DialogueEditor/Graph")]
        public static void Open()
        {
            GetWindow<DEWindow>("Dialogue Graph");
        }
        private void OnEnable()
        {
            AddGraphView();
            AddToolBar();
            AddStyles();
        }

        private void AddToolBar()
        {
            Toolbar toolbar = new Toolbar();

            fileNameTextField = DEElementUtility.CreateTextField(defaultFileName, "File Name:", callback =>
            {
                fileNameTextField.value = callback.newValue;//RemoveWhiteSpace?
            });

            saveButton = DEElementUtility.CreateButton("Save", () => Save());
            Button clearButton = DEElementUtility.CreateButton("Clear", () => Clear());
            Button loadButton = DEElementUtility.CreateButton("Load", () => Load());
            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);

            rootVisualElement.Add(toolbar);
        }
        private void Save()
        {
            if (string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog("Invalid File Name", "Ensure that the file name you have entered is valid", "Oke!");
                return;
            }
            DEIOUUtility.Initialize(graph, fileNameTextField.value);
            DEIOUUtility.Save();
        }
        private void Load()
        {
            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", "Assets/Editor/DialogueEditor/Graphs", "asset");
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            Clear();
            DEIOUUtility.Initialize(graph, Path.GetFileNameWithoutExtension(filePath));
            DEIOUUtility.Load();
        }
        private void Clear()
        {
            graph.ClearGraph();
        }
        private void AddGraphView()
        {
            graph = new DEGraph(this);
            graph.StretchToParentSize();
            rootVisualElement.Add(graph);
        }
        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogueEditor/DEVariables.uss");
        }
        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }
        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }
    }
}