namespace WellFormed.Core

open System.Windows
open System.Windows.Controls

[<AutoOpen>]
module Controls =

    let Input t = 
        let rebuild (ui :FrameworkElement) = CreateElement ui (fun () -> new InputControl(t)) :> FrameworkElement
        let collect (ui :FrameworkElement) = CollectFromElement ui (fun (ui' : InputControl)-> Success ui'.Text)
        let failures(ui :FrameworkElement) = []

        Formlet.New rebuild collect failures 

    let Select<'T> (i : int) (options : (string * 'T)  list)  = 
        let rebuild (ui :FrameworkElement) = CreateElement ui (fun () -> new SelectControl<'T>(i, options)) :> FrameworkElement
        let collect (ui :FrameworkElement) = CollectFromElement ui (fun (ui' : SelectControl<'T>) ->    
            let c = ui'.Collect()
            match c with
            |   Some v  -> Success v
            |   None    -> Nothing
            )
        let failures(ui :FrameworkElement) = FailuresFromElement ui (fun (ui' : SelectControl<'T>) ->    
            let c = ui'.Collect()
            match c with
            |   Some v  -> []
            |   None    -> Fail "Select a value"
            )
        Formlet.New rebuild collect failures
