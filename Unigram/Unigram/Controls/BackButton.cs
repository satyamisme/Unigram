﻿using Unigram.Navigation;

namespace Unigram.Controls
{
    public class BackButton : GlyphButton
    {
        public BackButton()
        {
            DefaultStyleKey = typeof(BackButton);
            Click += OnClick;
        }

        private void OnClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            BootStrapper.Current.RaiseBackRequested();
        }
    }
}
