﻿namespace WellFormed.Core

open System.Windows
open System.Windows.Controls

[<AutoOpen>]
module Controls =

    let Input t = 
        let rebuild (ui :FrameworkElement) = CreateElement ui (fun () -> new InputControl(t)) :> FrameworkElement
        let collect (ui :FrameworkElement) = ApplyToElement ui (fun (ui' : InputControl)-> Success ui'.Text)

        Formlet.New rebuild collect

    let Select<'T> (i : int) (options : (string * 'T)  list)  = 
        let rebuild (ui :FrameworkElement) = CreateElement ui (fun () -> new SelectControl<'T>(i, options)) :> FrameworkElement
        let collect (ui :FrameworkElement) = ApplyToElement ui (fun (ui' : SelectControl<'T>)-> let collect = ui'.Collect()
                                                                                                match collect with
                                                                                                    |   Some v  -> Success v
                                                                                                    |   None    -> Nothing
                                                                                                    )

        Formlet.New rebuild collect
