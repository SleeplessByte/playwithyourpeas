using System;
using ExEnSilver;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using Microsoft.Xna.Framework.Content;

namespace PlayWithYourPeas.Silverlight
{
	public class App : ExEnSilverApplication
	{
		protected override void SetupMainPage(MainPage mainPage)
		{
            //FontSource fontSource = new FontSource(Application.GetResourceStream(
            //    new Uri("/<ASSEMBLY NAME>;component/<PATH TO TTF>/<TTF FILENAME>", UriKind.Relative)).Stream);
            //FontFamily fontFamily = new FontFamily("<FONT FACE>");

  
            
			var game = new PeasGame();
			mainPage.Children.Add(game);
			game.Play();


		}
	}
}
