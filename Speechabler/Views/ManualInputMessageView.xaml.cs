using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace Speechabler.Views
{
    [View]
    public partial class ManualInputMessageView : UserControl
    {
        public ManualInputMessageView()
        {
            InitializeComponent();
            InputRegion.MouseMove += InputRegionMouseMove;
            InputRegion.MouseEnter += (s, e) => ManualInputPreviewCanvas.Visibility = Visibility.Visible;
            InputRegion.MouseLeave += (s, e) => ManualInputPreviewCanvas.Visibility = Visibility.Collapsed;
            Message.TextChanged += (s, e) => UpdatePreviewText();
            Message.SelectionChanged += (s, e) => UpdatePreviewText();
        }

        private void MoveLeftButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            Message.Focus();
            if (Message.SelectionLength > 0)
            {
                Message.CaretIndex = Message.SelectionStart;
                Message.SelectionLength = 0;
            }
            else if (Message.CaretIndex > 0)
                Message.CaretIndex--;
        }

        private void MoveRightButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            Message.Focus();
            if (Message.SelectionLength > 0)
            {
                Message.CaretIndex = Message.SelectionStart + Message.SelectionLength;
                Message.SelectionLength = 0;
            }
            else if (Message.CaretIndex < Message.Text.Length)
                Message.CaretIndex++;
        }

        private void InputRegionMouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(InputRegion);
            Canvas.SetLeft(ManualInputPreview, position.X);
            Canvas.SetTop(ManualInputPreview, position.Y);
        }

        private void UpdatePreviewText()
        {
            var message = Message.Text;

            var selectionLength = Message.SelectionLength;
            if (selectionLength > 0)
            {
                var selectionStart = Message.SelectionStart;
                PrevText.Text = message.Substring(0, selectionStart);
                MidText.Text = Message.SelectedText;
                PostText.Text = message.Substring(selectionStart + selectionLength);

                PreviewCaretBox.Visibility = Visibility.Visible;
            }
            else
            {
                var caretIndex = Message.CaretIndex;
                PrevText.Text = caretIndex > 0 ? message.Substring(0, caretIndex - 1) : string.Empty;
                MidText.Text = caretIndex > 0 ? message.Substring(caretIndex - 1, 1) : string.Empty;
                PostText.Text = message.Substring(caretIndex);

                PreviewCaretBox.Visibility = !string.IsNullOrWhiteSpace(MidText.Text) && VagabondK.Korean.Hangul.HangulCharacter.IsHangul(MidText.Text[0]) ? Visibility.Visible : Visibility.Hidden;
            }

        }
    }

    class PreviewCanvasLeftConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return -values[0].To<double>() + values[1].To<double>() * -0.5;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public class NumericScaleConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// 값을 변환합니다.
        /// </summary>
        /// <param name="value">바인딩 소스에서 생성한 값입니다.</param>
        /// <param name="targetType">바인딩 대상 속성의 형식입니다.</param>
        /// <param name="parameter">사용할 변환기 매개 변수입니다.</param>
        /// <param name="culture">변환기에서 사용할 문화권입니다.</param>
        /// <returns>변환된 값입니다.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue || value == Binding.DoNothing) return value;
            if (value == DBNull.Value) return null;

            var typeCode = System.Convert.GetTypeCode(value);
            return System.Convert.ChangeType(value.To<decimal>() * parameter.To<decimal>(), typeCode);
        }

        /// <summary>
        /// 값을 변환합니다.
        /// </summary>
        /// <param name="value">바인딩 대상에서 생성한 값입니다.</param>
        /// <param name="targetType">변환할 대상 형식입니다.</param>
        /// <param name="parameter">사용할 변환기 매개 변수입니다.</param>
        /// <param name="culture">변환기에서 사용할 문화권입니다.</param>
        /// <returns>변환된 값입니다.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 태그 확장의 대상 속성 값으로 제공된 개체를 반환합니다.
        /// </summary>
        /// <param name="serviceProvider">태그 확장명 서비스를 제공할 수 있는 서비스 공급자 도우미입니다.</param>
        /// <returns>확장이 적용되는 속성에 설정할 개체 값입니다.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

}
