using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;

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
            Thickness margin = new Thickness(0, ((taskCount + 10)), 0, 0);
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
            };
            checkbox.Unchecked += (sender, e) =>
            {
                leftTasks++;
                displayTaskCounter.Text = Convert.ToString(leftTasks);
            };
        }

        private void attachDeleteEventHandler(CheckBox checkbox)
        {

        }

        private void deleteModeHandler()
        {

        }

        private void textboxHandler(TextBox textbox)
        {
            textbox.KeyDown += (sender, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter && textbox.Text != "")
                {
                    TaskStack.Height = TaskStack.Height + 75;
                    var checkbox = new CheckBox();
                    checkbox.Content = textbox.Text;
                    checkbox.Margin = textbox.Margin;
                    TaskStack.Children.RemoveAt(taskCount);
                    TaskStack.Children.Add(checkbox);
                    addTaskButton.IsEnabled = true;
                    checkBoxStatusHandler(checkbox);
                    attachDeleteEventHandler(checkbox);
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
                exitEditModeHandler();
            }
            else
            {
                editMode = true;
                editTaskButton.Icon = new SymbolIcon(Symbol.Accept);
                editTaskButton.Label = "Accept changes";
                addTaskButton.IsEnabled = false;
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

        private void Textbox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            throw new NotImplementedException();
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
            var checkboxes = TaskStack.Children.OfType<CheckBox>().ToList();
            int index = 0;
            foreach(CheckBox currCheckBox in checkboxes)
            {
                currCheckBox.Content = checkboxesContent[index];
                currCheckBox.Visibility = Visibility.Visible;
                index++;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void deleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (deleteMode)
            {
                deleteMode = false;
                exitEditModeHandler();
            }
            else
            {
                deleteMode = true;
                deleteModeHandler();
            }
        }
    }
}
