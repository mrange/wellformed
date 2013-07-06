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

type FormletContainer<'T>(action : 'T -> unit, formlet : Formlet<'T>) as this=
    inherit UnaryElement()

    let mutable isDispatching                       = false
    let         scrollViewer                        = new ScrollViewer()

    do
        AddRoutedEventHandler FormletElement.RebuildEvent  this this.OnRebuild
        AddRoutedEventHandler FormletElement.SubmitEvent   this this.OnSubmit
        AddRoutedEventHandler FormletElement.ResetEvent    this this.OnReset

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

    member this.OnRebuild   (sender : obj) (e : RoutedEventArgs) = DispatchOnce this.BuildForm
    member this.OnSubmit    (sender : obj) (e : RoutedEventArgs) = DispatchOnce this.SubmitForm
    member this.OnReset     (sender : obj) (e : RoutedEventArgs) = DispatchOnce this.ResetForm

    override this.OnStartUp () =
        this.BuildForm()

    member this.ResetForm() = 
        scrollViewer.Content <- null
        this.BuildForm()

    member this.SubmitForm() = 
        let collect = 
            match scrollViewer.Content with 
            | :? FrameworkElement as fe -> formlet.Collect fe
            | _                         -> Fail_NeverBuiltUp()

        let v = collect.Value

        match collect.Failures.Length with   
        |   0   -> action v
        |   _   -> ()


    member this.BuildForm() = 
        match scrollViewer.Content with 
            | :? FrameworkElement as fe -> scrollViewer.Content <- formlet.Rebuild fe
            | _                         -> scrollViewer.Content <- formlet.Rebuild null

        let collect = 
            match scrollViewer.Content with 
            | :? FrameworkElement as fe -> formlet.Collect fe
            | _                         -> Fail_NeverBuiltUp()


        ()

module FormletContainer =
    let New<'T> (action : 'T -> unit) (formlet : Formlet<'T>) = new FormletContainer<'T>(action, formlet)


