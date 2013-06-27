namespace WellFormed.Core

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Threading
open System.Windows.Media

type FormletControl<'T>(action : 'T -> unit, formlet : Formlet<'T>) as this=
    inherit ContentControl()

    let mutable isDispatching = false

    do
        this.LayoutTransform <- new ScaleTransform (1.5, 1.5)

        let d = RoutedEventAsDelegate <| this.OnRebuild

        this.AddHandler (FormletContainerControl.RebuildEvent, d)
    
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

    override this.OnApplyTemplate()         =   
        base.OnApplyTemplate()
        DispatchOnce this.BuildForm

    member this.BuildForm() = 
        match this.Content with 
            | :? FrameworkElement as fe -> this.Content <- formlet.Rebuild (fe)
            | _ -> this.Content <- formlet.Rebuild(null)
        ()

module FormletControl =
    let New<'T> (action : 'T -> unit) (formlet : Formlet<'T>) = new FormletControl<'T>(action, formlet)


