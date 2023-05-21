using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NG__Editor
{
    /// <summary>
    /// Interaction logic for StepEditor.xaml
    /// </summary>
    public partial class StepEditor : Window
    {
        public StepEditor()
        {
            InitializeComponent();
            properties = new List<JToken>();
        }

        object[]? data;
        List<JToken> properties;
        public void Start()
        {
            data = DataContext as object[];
            if (data != null)
            {
                foreach (var property in data[0] as JToken)
                {
                    //loop through the value and get it's children's names
                    //add the name to the list
                    Step.Items.Add(property.Path.Split(".").Last());
                    properties.Add(property);
                }
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (data != null)
            {
                //get our cutscene data
                var cutsceneData = data[0] as JToken;

                //get data[2] (all json data)
                var allData = data[2] as JToken;
                //save it to the main json's cutscene data
                allData["Cutscenes"][data[1] as string] = cutsceneData;
                string json = allData.ToString();
                string? path = data[3] as string;
                File.WriteAllText(path, json);
            }
        }
        private void Step_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            JProperty token = (JProperty)properties[Step.SelectedIndex];
            //set Property.text to the value of the property
            Property.Text = token.Value.ToString();
            Property.Visibility = Visibility.Visible;
        }

        private void Property_TextChanged(object sender, TextChangedEventArgs e)
        {
            //get the selected step
            var selectedStep = Step.SelectedItem as string;
            if (selectedStep != null)
            {
                //get the step data
                var stepData = data?[0] as JToken;
                var SD = stepData[selectedStep];
                if (SD != null)
                {
                    //set the value of the step to the value of the textbox
                    switch (SD.Type)
                    {
                        case JTokenType.Integer:
                            //is Value.Text an int?
                            if (Property.Text.All(char.IsDigit) && Property.Text != "")
                            {
                                SD.Replace(int.Parse(Property.Text));
                            }
                            break;
                        case JTokenType.Float:
                            if (Property.Text.All(char.IsDigit))
                            {
                                SD.Replace(float.Parse(Property.Text));
                            }
                            break;
                        case JTokenType.String:
                            SD.Replace(Property.Text);
                            break;
                        case JTokenType.Boolean:
                            if (Property.Text.ToLower() == "true" && Property.Text.ToLower() == "false" && Property.Text != "")
                            {
                                SD.Replace(bool.Parse(Property.Text));
                            }
                            break;
                        default:
                            break;
                    }
                    stepData[selectedStep] = SD;
                    data[0] = stepData;
                }
                //get the property name you are editing
                JToken property = properties[Step.SelectedIndex];
                //set the value of the property to the value of the textbox
                switch (property.Type)
                {
                    case JTokenType.Integer:
                        //is Value.Text an int?
                        if (Property.Text.All(char.IsDigit) && Property.Text != "")
                        {
                            property.Replace(int.Parse(Property.Text));
                        }
                        break;
                    case JTokenType.Float:
                        if (Property.Text.All(char.IsDigit))
                        {
                            property.Replace(float.Parse(Property.Text));
                        }
                        break;
                    case JTokenType.String:
                        property.Replace(Property.Text);
                        break;
                    case JTokenType.Boolean:
                        if (Property.Text.ToLower() == "true" && Property.Text.ToLower() == "false" && Property.Text != "")
                        {
                            property.Replace(bool.Parse(Property.Text));
                        }
                        break;
                    default:
                        break;
                }
                properties[Step.SelectedIndex] = property;
            }
        }
    }
}
