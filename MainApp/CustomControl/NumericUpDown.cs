using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.CustomControl
{
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_ButtonUp", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonDown", Type = typeof(ButtonBase))]
    public class NumericUpDown : Control
    {
        private TextBox PART_TextBox = new TextBox();

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TextBox textBox = GetTemplateChild("PART_TextBox") as TextBox;
            if (textBox != null)
            {
                PART_TextBox = textBox;
                PART_TextBox.PreviewKeyDown += textBox_PreviewKeyDown;
                PART_TextBox.TextChanged += textBox_TextChanged;
                PART_TextBox.Text = Value.ToString();
            }

            ButtonBase PART_ButtonUp = GetTemplateChild("PART_ButtonUp") as ButtonBase;
            if (PART_ButtonUp != null)
            {
                PART_ButtonUp.Click += buttonUp_Click;
            }

            ButtonBase PART_ButtonDown = GetTemplateChild("PART_ButtonDown") as ButtonBase;
            if (PART_ButtonDown != null)
            {
                PART_ButtonDown.Click += buttonDown_Click;
            }
        }

        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
        }

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            "ValueChanged", RoutingStrategy.Direct,
            typeof(ValueChangedEventHandler), typeof(NumericUpDown));
        public event ValueChangedEventHandler ValueChanged
        {
            add
            {
                base.AddHandler(NumericUpDown.ValueChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(NumericUpDown.ValueChangedEvent, value);
            }
        }

        public int Places
        {
            get => (int)GetValue(PlacesProperty);
            set => SetValue(PlacesProperty, value);
        }

        public static readonly DependencyProperty PlacesProperty = DependencyProperty.Register("Places", typeof(int), typeof(NumericUpDown));

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(100D, maxValueChangedCallback, coerceMaxValueCallback));
        private static object coerceMaxValueCallback(DependencyObject d, object value)
        {
            double minValue = ((NumericUpDown)d).MinValue;
            if ((double)value < minValue)
                return minValue;

            return value;
        }
        private static void maxValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown numericUpDown = ((NumericUpDown)d);
            numericUpDown.CoerceValue(MinValueProperty);
            numericUpDown.CoerceValue(ValueProperty);
        }

        public double MinValue
        {
            get => (double)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(0D, minValueChangedCallback, coerceMinValueCallback));

        private static object coerceMinValueCallback(DependencyObject d, object value)
        {
            double maxValue = ((NumericUpDown)d).MaxValue;
            if ((double)value > maxValue)
                return maxValue;

            return value;
        }

        private static void minValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown numericUpDown = ((NumericUpDown)d);
            numericUpDown.CoerceValue(NumericUpDown.MaxValueProperty);
            numericUpDown.CoerceValue(NumericUpDown.ValueProperty);
        }

        public double Increment
        {
            get => (double)GetValue(IncrementProperty);
            set => SetValue(IncrementProperty, value);
        }
        
        public static readonly DependencyProperty IncrementProperty =
            DependencyProperty.Register(nameof(Increment), typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(1D, null, coerceIncrementCallback));
        
        private static object coerceIncrementCallback(DependencyObject d, object value)
        {
            NumericUpDown numericUpDown = ((NumericUpDown)d);
            double i = numericUpDown.MaxValue - numericUpDown.MinValue;
            if ((double)value > i)
                return i;

            return value;
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(0D, valueChangedCallback, coerceValueCallback), validateValueCallback);

        private static void valueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)d;
            ValueChangedEventArgs ea =
                new ValueChangedEventArgs(NumericUpDown.ValueChangedEvent, d, (double)e.OldValue, (double)e.NewValue);
            numericUpDown.RaiseEvent(ea);
            numericUpDown.PART_TextBox.Text = e.NewValue.ToString();
        }

        private static bool validateValueCallback(object value)
        {
            double val = (double)value;
            if (val > double.MinValue && val < double.MaxValue)
                return true;
            else
                return false;
        }

        private static object coerceValueCallback(DependencyObject d, object value)
        {
            double val = (double)value;
            double minValue = ((NumericUpDown)d).MinValue;
            double maxValue = ((NumericUpDown)d).MaxValue;
            double result;
            if (val < minValue)
                result = minValue;
            else if (val > maxValue)
                result = maxValue;
            else
                result = (double)value;

            return result;
        }

        private void buttonUp_Click(object sender, RoutedEventArgs e)
        {
            if (Value < MaxValue)
                Value += Increment;

            Value = Math.Round(Value, Places);
        }
        private void buttonDown_Click(object sender, RoutedEventArgs e)
        {
            if (Value > MinValue)
                Value -= Increment;

            Value = Math.Round(Value, Places);
        }

        private void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int index = PART_TextBox.CaretIndex;
            double result;
            if (!double.TryParse(PART_TextBox.Text, out result))
            {
                var changes = e.Changes.FirstOrDefault();
                if (changes != null)
                {
                    PART_TextBox.Text = PART_TextBox.Text.Remove(changes.Offset, changes.AddedLength);
                    PART_TextBox.CaretIndex = index > 0 ? index - changes.AddedLength : 0;
                }
            }
            else if (result < MaxValue && result > MinValue)
                Value = result;
            else
            {
                PART_TextBox.Text = Value.ToString();
                PART_TextBox.CaretIndex = index > 0 ? index - 1 : 0;
            }
        }
    }
}
