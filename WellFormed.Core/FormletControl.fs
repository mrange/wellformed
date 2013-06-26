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

    
    let DispatchOnce (action : unit -> unit) =
        if not isDispatching then
            isDispatching <- true
            Dispatch this.Dispatcher (fun () ->
                try
                    action()
                finally
                    isDispatching <- false
                )

    override this.OnApplyTemplate()         =   DispatchOnce this.BuildForm

    override this.OnLostKeyboardFocus(e)    =   DispatchOnce this.BuildForm

    member this.BuildForm() = 
        match this.Content with 
            | :? FrameworkElement as fe -> this.Content <- formlet.Rebuild (fe)
            | _ -> this.Content <- formlet.Rebuild(null)
        ()

module FormletControl =
    let New<'T> (action : 'T -> unit) (formlet : Formlet<'T>) = new FormletControl<'T>(action, formlet)


