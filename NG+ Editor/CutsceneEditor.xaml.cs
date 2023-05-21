using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NG__Editor
{
    /// <summary>
    /// Interaction logic for CutsceneEditor.xaml
    /// </summary>
    public partial class CutsceneEditor : Window
    {
        public CutsceneEditor()
        {
            InitializeComponent();
            stepsList = new List<JToken>();
        }

        object[]? data;
        List<JToken> stepsList;
        public void Start()
        {
            data = DataContext as object[];
            if (data != null)
            {
                foreach(var cutscene in data[0] as JToken)
                {
                    //loop through the value and get it's children's names
                    foreach(var cutsceneName in cutscene.Children())
                    {
                        //add the name to the list
                        Cutscenes.Items.Add(cutsceneName.Path.Split(".").Last());
                    }
                }
            }
        }

        private void Cutscenes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //get the selected cutscene
            var selectedCutscene = Cutscenes.SelectedItem as string;
            if (selectedCutscene != null)
            {
                //get the cutscene data
                var cutsceneData = data?[0] as JToken;
                cutsceneData = cutsceneData[selectedCutscene];
                if (cutsceneData != null)
                {
                    //clear the list
                    Steps.Items.Clear();
                    stepsList.Clear();
                    //loop through the cutscene data
                    int i = 0;
                    foreach (var cutscene in cutsceneData)
                    {
                        //add the cutscene data to the list
                        var property = cutscene.Value<JToken>();
                        if (i == 0)
                        {
                            Steps.Items.Add(property.Path.Split(".").Last());
                        }
                        else
                        {
                            int index = 1;
                            foreach(var step in property.Values())
                            {
                                Steps.Items.Add("Step " + index.ToString());
                                stepsList.Add(step);
                                index++;
                            }
                        }
                        i++;
                        
                    }

                    Steps.Visibility = Visibility.Visible;
                }
            }
        }

        private void Steps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedStep = Steps.SelectedIndex;
            if(selectedStep == -1 || selectedStep == 0)
            {
                return;
            }
            var newWindow = new StepEditor();

            newWindow.Title = "Step Editor";

            newWindow.DataContext = new object[] { stepsList[selectedStep - 1], Cutscenes.SelectedItem as string, data[2], data[3] };
            newWindow.Start();
            newWindow.ShowDialog();
        }
    }
}
