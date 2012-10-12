using System.Windows.Controls;
using System.Windows.Graphics;
using System.Windows;
using Microsoft.Xna.Framework.Input;
using System;
using PlayWithYourPeas.Logic;
using System.Net;

namespace PlayWithYourPeas.SilverLight
{
    public partial class MainPage
    {
        PeasGame game;


        public MainPage(String token = null)
        {
            InitializeComponent();

            // Capture controlls
            Keyboard.RootControl = this;
            Mouse.RootControl = this;

            // Social connect
            SocialController.Setup();

            if (!String.IsNullOrWhiteSpace(token))
                SocialController.Connect(token);

            // Create game
            game = new PeasGame(App.Current.IsRunningOutOfBrowser, 1280, 720);
            game.Attach(LayoutRoot);

            // Makes sure to redraw
            game.IsFixedTimeStep = false;

            // Make resizable
            this.SizeChanged += new SizeChangedEventHandler(MainPage_SizeChanged);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            game.Resize(e.NewSize.Width, e.NewSize.Height);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Check if GPU is on
            if (GraphicsDeviceManager.Current.RenderMode != RenderMode.Hardware)
                MessageBox.Show("Enable GPU access by going to the silverlight plugin page > permission tab", "Warning", MessageBoxButton.OK);

            // Starts the game
            game.Run();

            // In Browser. Perhaps canvas.
            if (!App.Current.IsRunningOutOfBrowser)
                game.Resize(720, 720); // Resize for browser
        }

        
    }
}
