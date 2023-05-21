using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace NG__Editor
{
    /// <summary>
    /// Interaction logic for Properties.xaml
    /// </summary>
    public partial class Properties : Window
    {
        List<string> propertyNames;
        List<string> subPropertyNames;
        object[]? data;
        JToken? token;
        string selected = "";
        int subSelected = -1;
        public Properties()
        {
            InitializeComponent();
            propertyNames = new List<string>();
            subPropertyNames = new List<string>();
        }

        public void Start()
        {
            data = DataContext as object[];
            if (Header != null && data != null)
            {
                Header.Content = data[0];
                if(PropertiesList != null)
                {
                    PropertiesList.Items.Clear();
                    token = data[1] as JToken;
                    if (token != null)
                    {
                        foreach(JProperty property in token)
                        {
                            PropertiesList.Items.Add(property.Name);
                            propertyNames.Add(property.Name);
                        }
                    }
                }
            }
        }

        private void PropertiesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(data != null)
            {
                if(token != null && propertyNames[PropertiesList.SelectedIndex] != "alertStatus")
                {
                    selected = propertyNames[PropertiesList.SelectedIndex];
                    if(token[selected].Type != JTokenType.Array) 
                    {
                        subSelected = -1;
                        if (token[selected].Type != JTokenType.Boolean)
                        {
                            Dropdown.Visibility = Visibility.Hidden;
                            Value.Text = token[selected].ToString();
                            Value.Visibility = Visibility.Visible;
                            SubPropertiesList.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            Value.Visibility = Visibility.Hidden;
                            selected = propertyNames[PropertiesList.SelectedIndex];
                            List<string> options = new List<string>
                            {
                                "True",
                                "False"
                            };
                            Dropdown.ItemsSource = options;
                            Dropdown.Visibility = Visibility.Visible;
                            //convert alertStatus int into string
                            Dropdown.SelectedIndex = bool.Parse(token[selected].ToString()) ? 0 : 1;
                        }
                    }
                    else
                    {
                        Dropdown.Visibility = Visibility.Hidden;
                        bool isWeirdList = true;
                        SubPropertiesList.Items.Clear();
                        foreach (JToken tok in token[selected].Children())
                        {
                            if(tok.Type != JTokenType.Object)
                            {
                                isWeirdList = false;
                                SubPropertiesList.Visibility = Visibility.Hidden;
                                break;
                            }
                            foreach (JProperty property in tok.Children<JProperty>())
                            {
                                SubPropertiesList.Items.Add(property.Name);
                                subPropertyNames.Add(property.Name);
                            }
                        }
                        if (!isWeirdList)
                        {
                            subSelected = -1;
                            if (token[selected].Type != JTokenType.Boolean)
                            {
                                Value.Text = token[selected].ToString();
                                Value.Visibility = Visibility.Visible;
                                SubPropertiesList.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                Value.Visibility = Visibility.Hidden;
                                selected = propertyNames[PropertiesList.SelectedIndex];
                                //clear itemssource
                                List<string> options = new List<string>();
                                options.Add("True");
                                options.Add("False");
                                Dropdown.ItemsSource = options;
                                Dropdown.Visibility = Visibility.Visible;
                                //convert alertStatus int into string
                                Dropdown.SelectedIndex = bool.Parse(token[selected].ToString()) ? 0 : 1;
                            }
                            return;
                        }
                        SubPropertiesList.Visibility = Visibility.Visible;
                    }
                }
                else if(token != null)
                {
                    Value.Visibility = Visibility.Hidden;
                    selected = propertyNames[PropertiesList.SelectedIndex];
                    //clear itemssource
                    List<string> options = new List<string>();
                    options.Add("Unaware");
                    options.Add("Hunting");
                    options.Add("Combat");
                    Dropdown.ItemsSource = options;
                    Dropdown.Visibility = Visibility.Visible;
                    //convert alertStatus int into string
                    Dropdown.SelectedIndex = (int)token[selected];
                }
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if(data != null)
            {
                JProperty? category = data[2] as JProperty;
                if(category != null)
                {
                    if(token != null && category.Name != "Items")
                    {
                        category.Value[data[0]] = token;
                        JObject? jsonData = data[3] as JObject;
                        if (jsonData != null)
                        {
                            jsonData[data[6] as string][category.Name] = category.Value;
                            string json = jsonData.ToString();
                            string? path = data[4] as string;
                            System.IO.File.WriteAllText(path, json);
                            //update main windows data
                            MainWindow? mainWindow = data[5] as MainWindow;
                            if(mainWindow != null)
                            {
                                mainWindow.json_data = jsonData;
                                JProperty? firstObject = jsonData.Properties().FirstOrDefault();
                                JObject map = (JObject)firstObject.Value;
                                mainWindow.mapObjects.Clear();
                                foreach (JProperty property in map.Properties())
                                {
                                    if (property.Name == "Cutscenes")
                                    {
                                        continue;
                                    }
                                    mainWindow.mapObjects.Add(property);
                                }
                                this.Close();
                            }
                        }
                    }
                    else if(token != null)
                    {
                        JObject? jsonData = data[3] as JObject;
                        if (jsonData != null)
                        {
                            jsonData[data[6] as string][category.Name] = category.Value;

                            string json = jsonData.ToString();
                            string? path = data[4] as string;
                            if (path != null)
                            {
                                System.IO.File.WriteAllText(path, json);
                            }

                            MainWindow? mainWindow = data[5] as MainWindow;
                            if (mainWindow != null)
                            {
                                mainWindow.json_data = jsonData;

                                JProperty? firstProperty = jsonData.Properties().FirstOrDefault();
                                if (firstProperty != null)
                                {
                                    JObject map = (JObject)firstProperty.Value;
                                    mainWindow.mapObjects.Clear();
                                    foreach (JProperty property in map.Properties())
                                    {
                                        if(property.Name == "Cutscenes")
                                        {
                                            continue;
                                        }
                                        mainWindow.mapObjects.Add(property);
                                    }
                                }
                                this.Close();
                            }
                        }

                    }
                }
            }
        }

        private void Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (token != null && selected != null)
            {
                JTokenType type = token[selected].Type;
                //token[selected] = Value.Text;
                //check type and convert Value.Text to that type
                if(subSelected == -1)
                {
                    switch (type)
                    {
                        case JTokenType.Integer:
                            //is Value.Text an int?
                            if (Value.Text.All(char.IsDigit) && Value.Text != "")
                            {
                                token[selected] = int.Parse(Value.Text);
                            }
                            break;
                        case JTokenType.Float:
                            if (Value.Text.All(char.IsDigit))
                            {
                                token[selected] = float.Parse(Value.Text);
                            }
                            break;
                        case JTokenType.String:
                            token[selected] = Value.Text;
                            break;
                        case JTokenType.Boolean:
                            if (Value.Text.ToLower() == "true" && Value.Text.ToLower() == "false" && Value.Text != "")
                            {
                                token[selected] = bool.Parse(Value.Text);
                            }
                            break;
                        case JTokenType.Object:
                            try
                            {
                                token[selected] = JObject.Parse(Value.Text);
                            }
                            catch (Exception)
                            {
                                //oof
                            }
                            break;
                        case JTokenType.Array:
                            try
                            {
                                token[selected] = JArray.Parse(Value.Text);
                            }
                            catch (Exception)
                            {
                                //oof
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    JToken purposes = token[selected][0];
                    string value = "";
                    foreach (JProperty property in purposes.Children<JProperty>())
                    {
                        if (property.Name == SubPropertiesList.SelectedItem as string)
                        {
                            value = property.Name.ToString();
                        }
                    }
                    type = token[selected][0][value].Type;
                    switch (type)
                    {
                        case JTokenType.Integer:
                            //is Value.Text an int?
                            if (Value.Text.All(char.IsDigit) && Value.Text != "")
                            {
                                token[selected][0][value] = int.Parse(Value.Text);
                            }
                            break;
                        case JTokenType.Float:
                            if (Value.Text.All(char.IsDigit))
                            {
                                token[selected][0][value] = float.Parse(Value.Text);
                            }
                            break;
                        case JTokenType.String:
                            token[selected][0][value] = Value.Text;
                            break;
                        case JTokenType.Boolean:
                            if (Value.Text.ToLower() == "true" && Value.Text.ToLower() == "false" && Value.Text != "")
                            {
                                token[selected][0][value] = bool.Parse(Value.Text);
                            }
                            break;
                        case JTokenType.Object:
                            try
                            {
                                token[selected][0][value] = JObject.Parse(Value.Text);
                            }
                            catch (Exception)
                            {
                                //oof
                            }
                            break;
                        case JTokenType.Array:
                            try
                            {
                                token[selected][0][value] = JArray.Parse(Value.Text);
                            }
                            catch (Exception)
                            {
                                //oof
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void Dropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (token != null && subSelected == -1)
            {
                if (token[selected].Type != JTokenType.Boolean)
                {
                    token[selected] = Dropdown.SelectedIndex;
                }
                else
                {
                    token[selected] = Dropdown.SelectedIndex == 1 ? false : true;
                }
            }
            else if(token != null)
            {
                JToken purposes = token[selected][0];
                string value = "";
                foreach (JProperty property in purposes.Children<JProperty>())
                {
                    if (property.Name == SubPropertiesList.SelectedItem as string)
                    {
                        value = property.Name.ToString();
                    }
                }
                if (token[selected][0][value].Type != JTokenType.Boolean)
                {
                    token[selected][0][value] = Dropdown.SelectedIndex;
                }
                else
                {
                    token[selected][0][value] = Dropdown.SelectedIndex == 1 ? false : true;
                }
            }
        }

        private void SubPropertiesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (data != null)
            {
                if (token != null)
                {
                    subSelected = SubPropertiesList.SelectedIndex;
                    JToken purposes = token[selected][0];
                    string name = "";
                    try
                    {
                        foreach (JProperty property in purposes.Children<JProperty>())
                        {
                            if (property.Name == SubPropertiesList.SelectedItem as string)
                            {
                                Value.Text = property.Value.ToString();
                                name = property.Name.ToString();
                            }
                        }
                    }
                    catch(Exception) 
                    {
                        //oof
                    }
                    if (token[selected][0][name].Type != JTokenType.Boolean)
                    {
                        Value.Visibility = Visibility.Visible;
                        Dropdown.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        Value.Visibility = Visibility.Hidden;
                        List<string> options = new List<string>();
                        options.Add("True");
                        options.Add("False");
                        Dropdown.ItemsSource = options;
                        Dropdown.Visibility = Visibility.Visible;
                        //convert alertStatus int into string
                        Dropdown.SelectedIndex = bool.Parse(token[selected][0][name].ToString()) ? 0 : 1;
                    }
                }
            }
        }
    }
}
