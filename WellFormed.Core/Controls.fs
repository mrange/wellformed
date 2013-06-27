namespace WellFormed.Core

open System.Windows
open System.Windows.Controls

[<AutoOpen>]
module Controls =

    let Information t = 
        let rebuild (ui :FrameworkElement) = CreateElement ui (fun () -> new InformationControl(t)) :> FrameworkElement
        let collect (ui :FrameworkElement) = Success ()

        Formlet.New rebuild collect

    let Input t = 
        let rebuild (ui :FrameworkElement) = CreateElement ui (fun () -> new InputControl(t)) :> FrameworkElement
        let collect (ui :FrameworkElement) = ApplyToElement ui (fun (ui' : TextBox)-> Success ui'.Text)

        Formlet.New rebuild collect
