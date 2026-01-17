using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;
using Windows.Storage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Simple_Todo_for_WP8
{
    /// <summary>
    /// The main and only page of this app (might add functionality for multiple pages)
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int taskCount = 0;
        private int leftTasks = 0;
        private const int stackMargin = 10;
        ApplicationDataContainer saveFile = ApplicationData.Current.LocalSettings;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            (App.Current as App).loadData();
            foreach (CheckBox checkbox in (App.Current as App).taskData)
            {
                initializeCheckBox(checkbox, true);
            }
        }

        private void initializeCheckBox(CheckBox checkbox, bool fromSaveFile)
        {
            TaskStack.Children.Add(checkbox);
            checkBoxStatusHandler(checkbox);
            attachHoldingEvent(checkbox);
            if (checkbox.IsChecked == false)
            {
                leftTasks++;
                displayTaskCounter.Text = Convert.ToString(leftTasks);
            }
            taskCount++;
            if (!fromSaveFile)
            {
                int dataIndex = TaskStack.Children.IndexOf(checkbox);
                (App.Current as App).taskData.Insert(dataIndex, checkbox);
                string dataStateValues = checkbox.Content.ToString() + '\n' + checkbox.IsChecked.ToString();
                string dictionaryDataIndex = Convert.ToString(dataIndex);
                if (saveFile.Values.Keys.Contains(dictionaryDataIndex))
                {
                    saveFile.Values.Remove(dictionaryDataIndex);
                }
                saveFile.Values.Add(dictionaryDataIndex, dataStateValues);
            }
            TaskStack.Height += checkbox.Height + stackMargin;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {  
            var textbox = new TextBox();
            Thickness margin = new Thickness(0, stackMargin, 0, 0);
            textbox.Margin = margin;
            textbox.Width = TaskStack.Width;
            textbox.Name = $"textbox{taskCount}";
            TaskStack.Children.Add(textbox);
            textbox.Focus(FocusState.Programmatic);
            addCheckBox(textbox);
            addTaskButton.IsEnabled = false;
        }

        private void addCheckBox(TextBox textbox)
        {
            textbox.KeyDown += (sender, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter && textbox.Text != "")
                {
                    var checkbox = new CheckBox();
                    checkbox.Content = textbox.Text;
                    checkbox.Margin = textbox.Margin;
                    TaskStack.Children.Remove(textbox);
                    initializeCheckBox(checkbox, false);
                    addTaskButton.IsEnabled = true;
                }
            };
        }

        private void attachHoldingEvent(CheckBox checkbox)
        {
            bool popupMenuIsShowing = false;
            checkbox.Holding += async (sender, e) =>
            {
                if (popupMenuIsShowing == false) //Prevents a double-invoke bug
                {
                    popupMenuIsShowing = true;
                    var menu = new PopupMenu();
                    menu.Commands.Add(new UICommand("Edit", null, 0));
                    menu.Commands.Add(new UICommand("Delete", null, 1));
                    Point holdPoint = new Point();
                    holdPoint.X = e.GetPosition(null).X;
                    holdPoint.Y = e.GetPosition(null).Y;
                    var chosenCommand = await menu.ShowAsync(holdPoint);
                    if (chosenCommand == null)
                    {
                        popupMenuIsShowing = false;
                    }
                    else if ((int?)chosenCommand.Id == 0)
                    {
                        editCheckBox(checkbox);
                        popupMenuIsShowing = false;
                    }
                    else if ((int?)chosenCommand.Id == 1)
                    {
                        deleteCheckBox(checkbox);
                        popupMenuIsShowing = false;
                    }
                }
                
            };
        }

        private void editCheckBox(CheckBox checkbox)
        {
            checkbox.Visibility = Visibility.Collapsed;
            var textbox = new TextBox();
            textbox.Margin = checkbox.Margin;
            textbox.Text = (string)checkbox.Content;
            textbox.Width = checkbox.Width;
            TaskStack.Children.Insert(TaskStack.Children.IndexOf(checkbox),textbox);
            textbox.Focus(FocusState.Programmatic);
            textbox.KeyDown += (sender, e) =>
            {
                if(e.Key == Windows.System.VirtualKey.Enter)
                {
                    checkbox.Content = textbox.Text;
                    TaskStack.Children.Remove(textbox);
                    checkbox.Visibility = Visibility.Visible;
                    saveTask(checkbox);
                }
            };
            textbox.LostFocus += (sender, e) => 
            {
                TaskStack.Children.Remove(textbox);
                checkbox.Visibility = Visibility.Visible;
            };
        }

        private async void deleteCheckBox(CheckBox checkbox)
        {
            int currCheckBoxId = TaskStack.Children.IndexOf(checkbox);
            var warning = new MessageDialog($"You are about to delete task #{currCheckBoxId + 1}. Are you sure?", "Confirm Action");
            warning.Commands.Add(new UICommand("Yes", null, 1));
            warning.Commands.Add(new UICommand("No", null, 0));
            warning.DefaultCommandIndex = 0;
            warning.CancelCommandIndex = 1;
            IUICommand result = await warning.ShowAsync();
            if ((int)result.Id == 1)
            {
                if (!((bool)checkbox.IsChecked))
                {
                    leftTasks--;
                    displayTaskCounter.Text = Convert.ToString(leftTasks);
                }
                saveFile.Values.Clear(); //Cleaning the list. Not a good solution, i know
                (App.Current as App).taskData.Clear();
                TaskStack.Children.Remove(checkbox);
                int index = 0; 
                foreach (CheckBox currCheckbox in TaskStack.Children)
                { //Reassigning the list and save file with updated indexes from TaskStack(prevents duplicate assignments)
                    (App.Current as App).taskData.Insert(index, currCheckbox);
                    string dataStateValues = currCheckbox.Content.ToString() + '\n' + currCheckbox.IsChecked.ToString();
                    saveFile.Values.Add(Convert.ToString(index), dataStateValues);
                    index++;
                } 
                taskCount--;
            }
        }
        
        private void checkBoxStatusHandler(CheckBox checkbox)
        {
            checkbox.Checked += (sender, e) =>
            {
                leftTasks--;
                displayTaskCounter.Text = Convert.ToString(leftTasks);
                saveTask(checkbox);
            };
            checkbox.Unchecked += (sender, e) =>
            {
                leftTasks++;
                displayTaskCounter.Text = Convert.ToString(leftTasks);
                saveTask(checkbox);
            };
        }

        public void saveTask(CheckBox checkbox)
        {
            int index = (App.Current as App).taskData.IndexOf(checkbox);
            if((App.Current as App).taskData.Contains(checkbox))
            {
                (App.Current as App).taskData.Remove(checkbox);
            }
            (App.Current as App).taskData.Insert(index, checkbox);
            string dataStateValues = checkbox.Content.ToString() + '\n' + checkbox.IsChecked.ToString();
            saveFile.Values[Convert.ToString(index)] = dataStateValues;
        }
    }
}
