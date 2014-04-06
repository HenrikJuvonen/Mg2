using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Mg2.Behaviors
{
    // https://caliburnmicro.codeplex.com/discussions/222164/

    public enum KeyEventAction
    {
        KeyUp,
        KeyDown
    }

    public class KeyPressTrigger : TriggerBase<FrameworkElement>
    {
        public static readonly DependencyProperty KeyActionProperty =
            DependencyProperty.Register("KeyAction", typeof(KeyEventAction), typeof(KeyPressTrigger),
            new PropertyMetadata(null));

        public static readonly DependencyProperty GestureProperty =
            DependencyProperty.Register("Gesture", typeof(InputGesture), typeof(KeyPressTrigger),
            new PropertyMetadata(null));

        [TypeConverter(typeof(KeyGestureConverter)), Category("KeyPress Properties")]
        public InputGesture Gesture
        {
            get { return (InputGesture)GetValue(GestureProperty); }
            set { SetValue(GestureProperty, value); }
        }

        [Category("KeyPress Properties")]
        public KeyEventAction KeyAction
        {
            get { return (KeyEventAction)GetValue(KeyActionProperty); }
            set { SetValue(KeyActionProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (KeyAction == KeyEventAction.KeyUp)
                AssociatedObject.KeyUp += OnKeyPress;
            else if (KeyAction == KeyEventAction.KeyDown)
                AssociatedObject.KeyDown += OnKeyPress;
            else
                throw new ArgumentOutOfRangeException("KeyAction", string.Format("{0} is not support.", KeyAction));
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (KeyAction == KeyEventAction.KeyUp)
                AssociatedObject.KeyUp -= OnKeyPress;
            else if (KeyAction == KeyEventAction.KeyDown)
                AssociatedObject.KeyDown -= OnKeyPress;
            else
                throw new ArgumentOutOfRangeException("KeyAction", string.Format("{0} is not support.", KeyAction));
        }

        private void OnKeyPress(object sender, KeyEventArgs args)
        {
            var kGesture = Gesture as KeyGesture;
            if (kGesture == null)
                return;

            if (kGesture.Matches(null, args))
                InvokeActions(null);
        }
    }
}
