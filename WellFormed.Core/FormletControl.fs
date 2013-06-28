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
        ()

module FormletControl =
    let New<'T> (action : 'T -> unit) (formlet : Formlet<'T>) = new FormletControl<'T>(action, formlet)


