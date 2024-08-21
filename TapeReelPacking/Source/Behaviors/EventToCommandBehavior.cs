 using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
namespace TapeReelPacking.Source.Behaviors

{ 
    public class EventToCommandBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(EventToCommandBehavior));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventToCommandBehavior));

        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.Register("EventName", typeof(string), typeof(EventToCommandBehavior), new PropertyMetadata(OnEventNameChanged));

        private Delegate _handler;

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }

        protected override void OnAttached()
        {
            RegisterEvent(EventName);
        }

        protected override void OnDetaching()
        {
            DeregisterEvent(EventName);
        }

        private static void OnEventNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (EventToCommandBehavior)d;
            if (behavior.AssociatedObject == null)
                return;

            behavior.DeregisterEvent((string)e.OldValue);
            behavior.RegisterEvent((string)e.NewValue);
        }

        private void RegisterEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            var eventInfo = AssociatedObject.GetType().GetEvent(eventName);
            if (eventInfo == null)
                throw new ArgumentException($"The event '{eventName}' was not found on type '{AssociatedObject.GetType().Name}'.");

            var methodInfo = GetType().GetMethod(nameof(OnEvent), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            _handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);
            eventInfo.AddEventHandler(AssociatedObject, _handler);
        }

        private void DeregisterEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName) || _handler == null)
                return;

            var eventInfo = AssociatedObject.GetType().GetEvent(eventName);
            eventInfo?.RemoveEventHandler(AssociatedObject, _handler);
            _handler = null;
        }

        private void OnEvent(object sender, EventArgs e)
        {
            if (Command == null)
                return;

            var parameter = CommandParameter ?? e;
            if (Command.CanExecute(parameter))
            {
                Command.Execute(parameter);
            }
        }
    }
}
