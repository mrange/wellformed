namespace WellFormed.Core

open System.Windows
open System.Windows.Controls

[<AutoOpen>]
module Controls =

    let Input t = 
        let rebuild (ui :FrameworkElement) = CreateElement ui (fun () -> new InputControl(t)) :> FrameworkElement
        let collect (ui :FrameworkElement) = ApplyToElement ui (fun (ui' : TextBox)-> Success ui'.Text)

        Formlet.New rebuild collect
