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
        public int taskCount;
        int leftTasks;
        public bool editMode = false;
        public bool deleteMode = false;
        const int checkMargin = 10;
        public MainPage()
        {
            this.InitializeComponent();
            taskCount = 0;

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.
            (App.Current as App).loadData();
            foreach (CheckBox checkbox in (App.Current as App).taskData)
            {
                TaskStack.Children.Add(checkbox);
                checkBoxStatusHandler(checkbox);
                attachDeleteEventHandler(checkbox);
                if(checkbox.IsChecked == false)
                {
                    leftTasks++;
                    displayTaskCounter.Text = Convert.ToString(leftTasks);
                }
                TaskStack.Height += checkbox.Height + checkMargin;
                taskCount++;
            }
            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {  
            var textbox = new TextBox();
            Thickness margin = new Thickness(0, ((taskCount + checkMargin)), 0, 0);
            textbox.Margin = margin;
            textbox.Width = TaskStack.Width;
            textbox.Name = $"textbox{taskCount}";
            TaskStack.Children.Add(textbox);
            textbox.Focus(FocusState.Programmatic);
            textboxHandler(textbox);
            addTaskButton.IsEnabled = false;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
        
        private void checkBoxStatusHandler(CheckBox checkbox)
        {
            checkbox.Checked += (sender, e) =>
            {
                leftTasks--;
                displayTaskCounter.Text = Convert.ToString(leftTasks);
                saveTasks();
            };
            checkbox.Unchecked += (sender, e) =>
            {
                leftTasks++;
                displayTaskCounter.Text = Convert.ToString(leftTasks);
                saveTasks();
            };
        }

        private async void attachDeleteEventHandler(CheckBox checkbox)
        {
            checkbox.Tapped += async (sender, e) =>
            {
                if (deleteMode)
                {
                    int currCheckBoxId = TaskStack.Children.IndexOf(checkbox);
                    var warning = new MessageDialog($"You are about to delete task #{currCheckBoxId}. Are you sure?", "Confirm Action");
                    warning.Commands.Add(new UICommand("Yes", null, 1));
                    warning.Commands.Add(new UICommand("No", null, 0));
                    warning.DefaultCommandIndex = 0;
                    warning.CancelCommandIndex = 1;
                    IUICommand result = await warning.ShowAsync();
                    if((int)result.Id == 1)
                    {
                        if (!((bool)checkbox.IsChecked))
                        {
                            leftTasks--;
                            displayTaskCounter.Text = Convert.ToString(leftTasks);
                        }
                        TaskStack.Children.Remove(checkbox);
                        taskCount--;
                        saveTasks();
                        
                    }
                }
            };
        }

        private void textboxHandler(TextBox textbox)
        {
            textbox.KeyDown += (sender, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter && textbox.Text != "")
                {
                    var checkbox = new CheckBox();
                    TaskStack.Height = TaskStack.Height + checkbox.Height + checkMargin;
                    checkbox.Content = textbox.Text;
                    checkbox.Margin = textbox.Margin;
                    TaskStack.Children.RemoveAt(taskCount);
                    TaskStack.Children.Add(checkbox);
                    addTaskButton.IsEnabled = true;
                    checkBoxStatusHandler(checkbox);
                    attachDeleteEventHandler(checkbox);
                    saveTasks();
                    taskCount++;
                    leftTasks++;
                    displayTaskCounter.Text = Convert.ToString(leftTasks);
                }
            };
        }

        private void editTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (editMode)
            {
                editMode = false;
                editTaskButton.Icon = new SymbolIcon(Symbol.Edit);
                editTaskButton.Label = "Edit Task";
                addTaskButton.IsEnabled = true;
                deleteTaskButton.IsEnabled = true;
                exitEditModeHandler();
            }
            else
            {
                editMode = true;
                editTaskButton.Icon = new SymbolIcon(Symbol.Accept);
                editTaskButton.Label = "Accept changes";
                addTaskButton.IsEnabled = false;
                deleteTaskButton.IsEnabled = false;
                editModeHandler();
            }

        }

        private void editModeHandler()
        {
            foreach (object currElement in TaskStack.Children)
            {
                if (currElement is CheckBox)
                {
                    CheckBox currCheckBox = (CheckBox)currElement;
                    currCheckBox.Visibility = Visibility.Collapsed;
                    var textbox = new TextBox();
                    textbox.Text = (string)currCheckBox.Content;
                    textbox.Margin = currCheckBox.Margin;
                    TaskStack.Children.Add(textbox);
                    textbox.KeyDown += (sender, e) =>
                    {
                        if (e.Key == Windows.System.VirtualKey.Enter)
                        {
                            editTaskButton.Focus(FocusState.Programmatic);
                        }
                    };
                }
            }
        }

        private void exitEditModeHandler()
        {
            var textboxes = TaskStack.Children.OfType<TextBox>().ToList();
            List<string> checkboxesContent = new List<string>();
            foreach(TextBox currTextBox in textboxes)
            {
                checkboxesContent.Add(currTextBox.Text);
                TaskStack.Children.Remove(currTextBox);
            }
            saveTasks();
            int index = 0;
            foreach(CheckBox currCheckBox in (App.Current as App).taskData)
            {
                currCheckBox.Content = checkboxesContent[index];
                currCheckBox.Visibility = Visibility.Visible;
                index++;
            }
        }

        private void deleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (deleteMode)
            {
                deleteMode = false;
                editTaskButton.IsEnabled = true;
                addTaskButton.IsEnabled = true;
            }
            else
            {
                deleteMode = true;
                editTaskButton.IsEnabled = false;
                addTaskButton.IsEnabled = false;
            }
            
        }

        public void saveTasks()
        {
            var checkboxes = TaskStack.Children.OfType<CheckBox>().ToList();
            (App.Current as App).taskData = checkboxes;
            var diskTaskData = ApplicationData.Current.LocalSettings;
            int index = 0;
            foreach(CheckBox checkbox in (App.Current as App).taskData)
            {
                string dataStateValues = checkbox.Content.ToString() + '\n' + checkbox.IsChecked.ToString();
                diskTaskData.Values[Convert.ToString(index)] = dataStateValues;
                index++;
            }
        }
    }
}
