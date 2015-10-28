using Script;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ScriptDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var engine = new ScriptEngine();
            engine.Exception(err =>
            {
                error.Text = err.Message;
            });
            engine.AddAction<string>("log", ActionMessage);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            output.Text = engine.Evaluate<string>(code.Text);
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            time.Text = String.Format("({0:00}.{1:00})",
                ts.Seconds,
                ts.Milliseconds / 10);
        }

        static async void ActionMessage(string message)
        {
            var dialog = new MessageDialog(message);
            await dialog.ShowAsync();
        }

        private void trigger_Click(object sender, RoutedEventArgs e)
        {
            var engine = new ScriptEngine();
            engine.Exception(err =>
            {
                error.Text = err.Message;
            });
            engine.AddAction<string>("log", ActionMessage);
            engine.Process(code.Text);
            output.Text = "";
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            engine.Trigger("name", 10);
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            time.Text = String.Format("({0:00}.{1:00})",
                ts.Seconds,
                ts.Milliseconds / 10);
        }
    }
}
