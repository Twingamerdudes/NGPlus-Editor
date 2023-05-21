using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace NG__Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<JProperty> mapObjects;
        public List<JProperty> sceneRooms;
        public JObject? json_data;
        public string filePath;
        public JProperty? currentCategory;
        public MainWindow()
        {
            mapObjects = new List<JProperty>();
            sceneRooms = new List<JProperty>();
            filePath = "";
            InitializeComponent();
        }


        private void Load_Json(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json files (*.json)|*.json";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                filePath = selectedFilePath;

                //parse the file
                string json = System.IO.File.ReadAllText(selectedFilePath);
                JObject jsonObject;
                try
                {
                    jsonObject = JObject.Parse(json);
                    json_data = jsonObject;
                }
                catch(Exception)
                {
                    MessageBox.Show("Invalid Json file");
                    return;
                }

                // Get the first object in the JSON
                JProperty? firstObject = jsonObject.Properties().FirstOrDefault();
                if (firstObject != null)
                {
                    //get the property Characters from the json
                    JObject _ = (JObject)firstObject.Value;
                    if(_.GetValue("Characters") == null)
                    {
                        MessageBox.Show("Invalid NG+ Json file");
                        return;
                    }
                    string objectName = firstObject.Name; // Name of the first object
                    JObject map = (JObject)firstObject.Value; // Value of the first object

                    Label jsonName = new Label();
                    jsonName.Content = objectName;
                    jsonName.FontSize = 20;
                    jsonName.FontWeight = FontWeights.Bold;

                    jsonName.Margin = new Thickness(0, 0, 0, 10);

                    //add it to the window
                    Button button = (Button)sender;
                    button.Visibility = Visibility.Hidden;

                    if (TagLine != null) { 
                        TagLine.Visibility = Visibility.Hidden;
                    }

                    if (Categories != null)
                    {
                        List<string> rooms = new List<string>();
                        foreach (JProperty property in jsonObject.Properties())
                        {
                            rooms.Add(property.Name);
                            sceneRooms.Add(property);
                            //mapObjects.Add(property);
                        }
                        Rooms.Visibility = Visibility.Visible;
                        Rooms.ItemsSource = rooms;
                        if (Header != null)
                        {
                            Header.Visibility = Visibility.Visible;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Invalid NG+ Json file");
                }
            }
        }

        private void Categories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ((ListBox)sender).SelectedIndex;
            if (index == -1)
            {
                return;
            }
            JProperty category = mapObjects[index];
            if(category.Name == "Cutscenes" && json_data != null)
            {
                var newWindow = new CutsceneEditor();

                newWindow.Title = "Cutscene Editor";

                newWindow.DataContext = new object[] {category.Value, "Cutscenes", json_data, filePath};
                newWindow.Start();
                newWindow.ShowDialog();
                return;
            }
            List<string> items = new List<string>();
            currentCategory = category;
            if (Items != null && category != null)
            {
                if(category.Name != "Items")
                {
                    foreach (JProperty property in category.Value)
                    {
                        items.Add(property.Name);
                    }
                    Items.ItemsSource = items;
                    Items.Visibility = Visibility.Visible;
                    Label? Header = FindName("Header2") as Label;
                    if (Header != null)
                    {
                        Header.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    //items is a list and not a object
                    //loop through the objects in the list, get the value myItem, and add it to listbox
                    foreach(JObject myItem in category.Value)
                    {
                        JProperty? item = myItem.Property("myItem");
                        if (item != null)
                        {
                            items.Add(item.Value.ToString());
                        }
                    }
                    Items.ItemsSource = items;
                    Items.Visibility = Visibility.Visible;
                }
            }
        }

        private void Items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ((ListBox)sender).SelectedIndex;
            if (index == -1)
            {
                return;
            }
            string? objName = ((ListBox)sender).SelectedValue as string;
            //get the object
            JToken? obj = null;
            if (objName != null && currentCategory != null && currentCategory.Name != "Items")
            {
                foreach(JProperty item in currentCategory.Value)
                {
                    if(item.Name == objName)
                    {
                        obj = item.Value; break;
                    }
                }
                if (obj != null && json_data != null)
                {
                    var newWindow = new Properties();

                    newWindow.Title = objName;

                    newWindow.DataContext = new object[] { objName, obj, currentCategory, json_data, filePath, this, Rooms.SelectedItem as string };
                    newWindow.Start();
                    newWindow.ShowDialog();
                }
            }
            else if(objName != null && currentCategory != null && currentCategory.Name == "Items")
            {
                foreach (JObject myItem in currentCategory.Value)
                {
                    if (myItem.Property("myItem").Value.ToString() == objName)
                    {
                        obj = myItem; break;
                    }
                }
                if (obj != null && json_data != null)
                {
                    var newWindow = new Properties();

                    newWindow.Title = objName;

                    newWindow.DataContext = new object[] { objName, obj, currentCategory, json_data, filePath, this, Rooms.SelectedItem as string};
                    newWindow.Start();
                    newWindow.ShowDialog();
                }
            }
        }

        private void Rooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Categories.UnselectAll();
            Items.Visibility = Visibility.Hidden;
            mapObjects.Clear();
            List<string> categories = new List<string>();
            var room = sceneRooms[Rooms.SelectedIndex];
            //add each category found within room
            foreach (JProperty property in room.Value)
            {
                if(property.Name == "Cutscenes")
                {
                    continue;
                }
                categories.Add(property.Name);
                mapObjects.Add(property);
            }
            //mapObjects.Add(property);
            Categories.Visibility = Visibility.Visible;
            Categories.ItemsSource = categories;
            if (Header1 != null)
            {
                Header1.Visibility = Visibility.Visible;
            }
        }
    }
}
