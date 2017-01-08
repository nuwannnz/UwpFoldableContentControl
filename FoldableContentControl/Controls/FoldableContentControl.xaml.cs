using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace FoldableContentControl.Controls
{
    public sealed partial class FoldableContentControl : UserControl,INotifyPropertyChanged
    {
        private Rect _clip;

        // This is the final content which will be displayed in the foldable area 
        private object _DisplayableContent;

        public FoldableContentControl()
        {
            this.InitializeComponent();
          
            FoldableContentContainer.SizeChanged += FoldableContent_SizeChanged;
            FoldableContainerParent.Visibility = Visibility.Collapsed;
            
        }

        private void FoldableContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {            
            // Apply the clipping 
            _clip= new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);
            FoldableContainerParent.Clip = new RectangleGeometry() { Rect = _clip };           
            
            // set the offset Y of the content
            if(IsFoldered) FoldableContentTranslateTransform.Y = -e.NewSize.Height;

            SetFoldableContent();                 
        }

        #region Properties
        private bool _isFoldered = true;
        public bool IsFoldered
        {
            get
            {
                return _isFoldered;
            }
            set
            {
                _isFoldered = value;
                RaisePropertyChanged();
                if ((bool)value)
                {
                    IconRotationReverseAnimation.Begin();
                    ExecuteCollapseAnimation();
                }
                else
                {
                    IconRotationAnimation.Begin();
                    ExecuteExpandAnimation();
                }
            }
        }

        #endregion


        #region DPs


        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register("HeaderText", typeof(string), typeof(FoldableContentControl), new PropertyMetadata("Header Text"));

        public static readonly DependencyProperty FoldableContentProperty =
            DependencyProperty.Register("FoldableContent", typeof(FrameworkElement), typeof(FoldableContentControl), new PropertyMetadata(null,new PropertyChangedCallback(OnFoldableContentChanged)));

        public static readonly DependencyProperty EmptyContentDisplayProperty =
            DependencyProperty.Register("EmptyContent", typeof(FrameworkElement), typeof(FoldableContentControl), new PropertyMetadata(null,new PropertyChangedCallback(OnEmptyContentChanged)));

        public static readonly DependencyProperty EmptyTextProperty =
            DependencyProperty.Register("EmptyText", typeof(string), typeof(FoldableContentControl), new PropertyMetadata(null,new PropertyChangedCallback(OnEmptyTextChanged)));


        public static readonly DependencyProperty DisplayEmptyProperty =
            DependencyProperty.Register("DisplayEmpty", typeof(bool), typeof(FoldableContentControl), new PropertyMetadata(false,new PropertyChangedCallback(OnDisplayEmptyContentChanged)));

        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }
              
        public FrameworkElement FoldableContent
        {
            get { return (FrameworkElement)GetValue(FoldableContentProperty); }
            set{SetValue(FoldableContentProperty, value);RaisePropertyChanged();}
        }
                
        public FrameworkElement EmptyContent
        {
            get { return (FrameworkElement)GetValue(EmptyContentDisplayProperty); }
            set { SetValue(EmptyContentDisplayProperty, value); }
        }

        public string EmptyText
        {
            get { return (string)GetValue(EmptyTextProperty); }
            set { SetValue(EmptyTextProperty, value); }
        }

        public bool DisplayEmpty
        {
            get { return (bool)GetValue(DisplayEmptyProperty); }
            set { SetValue(DisplayEmptyProperty, value); }
        }

       
        private static void OnDisplayEmptyContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FoldableContentControl;
            if((bool)e.NewValue == true)
            {
                control.SetEmptyContent();
            }
            else
            {
                control.SetFoldableContent();
            }
        }

        private static void OnFoldableContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FoldableContentControl;
            if(e.NewValue != null)
            {
                control.SetFoldableContent();                
            }
        }

        private static void OnEmptyContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FoldableContentControl;
            if(e.NewValue != null)
            {
                if(d.GetValue(FoldableContentProperty) == null)
                {
                    control.SetEmptyContent();
                }
            }
        }

        private static void OnEmptyTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FoldableContentControl;
            if(e.NewValue != null)
            {
                if(control.GetValue(FoldableContentProperty) == null)
                {
                    control.SetEmptyContent();
                }
            }
        }





        #endregion

        /// <summary>
        /// This method will make the empty content/text visible 
        /// </summary>
        private void SetEmptyContent()
        {
            if(EmptyContent !=  null)
            {
                this._DisplayableContent = EmptyContent;
            }
            else
            {
                this._DisplayableContent=  new TextBlock()
                {
                    Text = EmptyText == null? "No items to display" : EmptyText,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20)
                };
            }
            RaisePropertyChanged(nameof(_DisplayableContent));
        }

        /// <summary>
        /// This method will make the FoldableContent visible
        /// </summary>
        private void SetFoldableContent()
        {
            if(FoldableContent != null && !DisplayEmpty)
            {
                this._DisplayableContent = FoldableContent;
            }
            else
            {
                SetEmptyContent();
            }
            RaisePropertyChanged(nameof(_DisplayableContent));
        }


        /// <summary>
        /// Executes when the user taps on the control to Close/Open the control
        /// </summary>       
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {          
            IsFoldered = !IsFoldered;
        }


        private async void ExecuteCollapseAnimation()
        {
            OffsetYAnimation.From = 0;
            OffsetYAnimation.To = -(_clip.Height);            
            OffsetYAnimationStoryboard.Begin();
            await Task.Delay(250);
            FoldableContainerParent.Clip = null;
            FoldableContainerParent.Visibility = Visibility.Collapsed;
        }

        private void ExecuteExpandAnimation()
        {
            OffsetYAnimation.From = -(_clip.Height);
            OffsetYAnimation.To = 0;
            
            FoldableContainerParent.Visibility = Visibility.Visible;            
            FoldableContainerParent.Clip = new RectangleGeometry() { Rect = _clip };
            OffsetYAnimationStoryboard.Begin();

        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string property = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion

        
    }
}
