using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Simple_Todo_for_WP8
{

    public sealed partial class App : Application
    {
        private TransitionCollection transitions;
        public List<CheckBox> taskData { get; set; }
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }
        
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.CacheSize = 1;
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            Window.Current.Activate();
        }
        
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
        
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }

        public void loadData()
        {
            var saveFile = ApplicationData.Current.LocalSettings;
            taskData = new List<CheckBox>();
            foreach (KeyValuePair<string, object> saveValue in saveFile.Values)
            {
                CheckBox checkbox = new CheckBox();
                string literal = (string)saveValue.Value;
                int newlineCharIndex = literal.IndexOf('\n');
                string content = literal.Substring(0, newlineCharIndex);
                bool state = Convert.ToBoolean(literal.Substring(newlineCharIndex + 1));
                checkbox.Content = content;
                checkbox.IsChecked = state;
                taskData.Add(checkbox);
            }
        }
    }
}