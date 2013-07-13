// ############################################################################
// #                                                                          #
// #        ---==>  T H I S  F I L E  I S   G E N E R A T E D  <==---         #
// #                                                                          #
// # This means that any edits to the .cs file will be lost when its          #
// # regenerated. Changes should instead be applied to the corresponding      #
// # template file (.tt)                                                      #
// ############################################################################





// ReSharper disable InconsistentNaming
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantNameQualifier

namespace WellFormed2.WPF
{
    using System.Windows;


    // -------------------------------------------------------------------------

    partial class RebuildFormEventArgs : RoutedEventArgs
    {
        public delegate void Handler (object sender, RebuildFormEventArgs eventArgs);
    }
    // -------------------------------------------------------------------------

    // -------------------------------------------------------------------------

    partial class SubmitFormEventArgs : RoutedEventArgs
    {
        public delegate void Handler (object sender, SubmitFormEventArgs eventArgs);
    }
    // -------------------------------------------------------------------------

    // -------------------------------------------------------------------------

    partial class ResetFormEventArgs : RoutedEventArgs
    {
        public delegate void Handler (object sender, ResetFormEventArgs eventArgs);
    }
    // -------------------------------------------------------------------------


    partial class FormletControl
    {
        // ----------------------------------------------------------------------
        public readonly static RoutedEvent RebuildFormEvent = EventManager.RegisterRoutedEvent (
            "RebuildForm",
            RoutingStrategy.Bubble,
            typeof (RebuildFormEventArgs.Handler),
            typeof (FormletControl)
            );
        public event RebuildFormEventArgs.Handler RebuildForm
        {
            add { this.AddHandler_RebuildForm (value); }
            remove { this.RemoveHandler_RebuildForm (value); }
        }
        // ----------------------------------------------------------------------


        // ----------------------------------------------------------------------
        public readonly static RoutedEvent SubmitFormEvent = EventManager.RegisterRoutedEvent (
            "SubmitForm",
            RoutingStrategy.Bubble,
            typeof (SubmitFormEventArgs.Handler),
            typeof (FormletControl)
            );
        public event SubmitFormEventArgs.Handler SubmitForm
        {
            add { this.AddHandler_SubmitForm (value); }
            remove { this.RemoveHandler_SubmitForm (value); }
        }
        // ----------------------------------------------------------------------


        // ----------------------------------------------------------------------
        public readonly static RoutedEvent ResetFormEvent = EventManager.RegisterRoutedEvent (
            "ResetForm",
            RoutingStrategy.Bubble,
            typeof (ResetFormEventArgs.Handler),
            typeof (FormletControl)
            );
        public event ResetFormEventArgs.Handler ResetForm
        {
            add { this.AddHandler_ResetForm (value); }
            remove { this.RemoveHandler_ResetForm (value); }
        }
        // ----------------------------------------------------------------------


    }

    static partial class RoutedEventExtensions
    {
        public static void Raise_RebuildForm (
            this UIElement uiElement, 
            RebuildFormEventArgs routedEventArgs = null
            )
        {
            if (uiElement != null)
            {
                routedEventArgs = routedEventArgs ?? new RebuildFormEventArgs ();
                routedEventArgs.RoutedEvent = FormletControl.RebuildFormEvent;
                uiElement.RaiseEvent (routedEventArgs);
            }
        }

        public static void AddHandler_RebuildForm (
            this UIElement uiElement,
            RebuildFormEventArgs.Handler eventHandler,
            bool handledEventsToo = false
            )
        {
            if (uiElement != null)
            {
                uiElement.AddHandler (FormletControl.RebuildFormEvent, eventHandler, handledEventsToo);
            }
        }

        public static void RemoveHandler_RebuildForm (
            this UIElement uiElement,
            RebuildFormEventArgs.Handler eventHandler
            )
        {
            if (uiElement != null)
            {
                uiElement.RemoveHandler (FormletControl.RebuildFormEvent, eventHandler);
            }
        }
        public static void Raise_SubmitForm (
            this UIElement uiElement, 
            SubmitFormEventArgs routedEventArgs = null
            )
        {
            if (uiElement != null)
            {
                routedEventArgs = routedEventArgs ?? new SubmitFormEventArgs ();
                routedEventArgs.RoutedEvent = FormletControl.SubmitFormEvent;
                uiElement.RaiseEvent (routedEventArgs);
            }
        }

        public static void AddHandler_SubmitForm (
            this UIElement uiElement,
            SubmitFormEventArgs.Handler eventHandler,
            bool handledEventsToo = false
            )
        {
            if (uiElement != null)
            {
                uiElement.AddHandler (FormletControl.SubmitFormEvent, eventHandler, handledEventsToo);
            }
        }

        public static void RemoveHandler_SubmitForm (
            this UIElement uiElement,
            SubmitFormEventArgs.Handler eventHandler
            )
        {
            if (uiElement != null)
            {
                uiElement.RemoveHandler (FormletControl.SubmitFormEvent, eventHandler);
            }
        }
        public static void Raise_ResetForm (
            this UIElement uiElement, 
            ResetFormEventArgs routedEventArgs = null
            )
        {
            if (uiElement != null)
            {
                routedEventArgs = routedEventArgs ?? new ResetFormEventArgs ();
                routedEventArgs.RoutedEvent = FormletControl.ResetFormEvent;
                uiElement.RaiseEvent (routedEventArgs);
            }
        }

        public static void AddHandler_ResetForm (
            this UIElement uiElement,
            ResetFormEventArgs.Handler eventHandler,
            bool handledEventsToo = false
            )
        {
            if (uiElement != null)
            {
                uiElement.AddHandler (FormletControl.ResetFormEvent, eventHandler, handledEventsToo);
            }
        }

        public static void RemoveHandler_ResetForm (
            this UIElement uiElement,
            ResetFormEventArgs.Handler eventHandler
            )
        {
            if (uiElement != null)
            {
                uiElement.RemoveHandler (FormletControl.ResetFormEvent, eventHandler);
            }
        }
    }

}

