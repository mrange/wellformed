// ----------------------------------------------------------------------------------------------
// Copyright (c) Mårten Rånge.
// ----------------------------------------------------------------------------------------------
// This source code is subject to terms and conditions of the Microsoft Public License. A 
// copy of the license can be found in the License.html file at the root of this distribution. 
// If you cannot locate the  Microsoft Public License, please send an email to 
// dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
//  by the terms of the Microsoft Public License.
// ----------------------------------------------------------------------------------------------
// You must not remove this notice, or any other, from this software.
// ----------------------------------------------------------------------------------------------

namespace WellFormed.Core

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Threading
open System.Windows.Media

type FormletControl<'T>(action : 'T -> unit, formlet : Formlet<'T>) as this=
    inherit UnaryControl()

    let mutable initialized                         = false
    let mutable isDispatching                       = false
    let         scrollViewer                        = new ScrollViewer()

    do
        this.LayoutTransform <- new ScaleTransform (1.5, 1.5)

        let d = RoutedEventAsDelegate <| this.OnRebuild

        this.AddHandler (FormletContainerControl.RebuildEvent, d)

        scrollViewer.HorizontalScrollBarVisibility  <- ScrollBarVisibility.Disabled
        scrollViewer.VerticalScrollBarVisibility    <- ScrollBarVisibility.Visible
        this.Value <- scrollViewer

    let DispatchOnce (action : unit -> unit) =
        if not isDispatching then
            isDispatching <- true
            Dispatch this.Dispatcher (fun () ->
                try
                    action()
                finally
                    isDispatching <- false
                )

    member this.OnRebuild (sender : obj) (e : RoutedEventArgs)
                                            =   DispatchOnce this.BuildForm

    override this.MeasureOverride(sz : Size) =
        if not initialized then
            this.BuildForm()

        initialized <- true

        base.MeasureOverride(sz)
    

    member this.BuildForm() = 
        match scrollViewer.Content with 
            | :? FrameworkElement as fe -> scrollViewer.Content <- formlet.Rebuild(fe)
            | _                         -> scrollViewer.Content <- formlet.Rebuild(null)

        let collect = 
            match scrollViewer.Content with 
            | :? FrameworkElement as fe -> formlet.Collect (fe)
            | _                         -> Fail_NeverBuiltUp()


        ()

module FormletControl =
    let New<'T> (action : 'T -> unit) (formlet : Formlet<'T>) = new FormletControl<'T>(action, formlet)


