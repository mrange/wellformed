
open System
open System.Windows

open WellFormed.Core

[<EntryPoint>]
[<STAThread>]
let main argv = 
    let window = new Window ()
    window.MinWidth <- 600.0
    window.MinHeight <- 400.0
    window.Title <- "WellFormed App"

    let formlet = Formlet.Do
                        {
                            let! first = Controls.Input ""
                            let! second = Controls.Input ""
                            if first = "" then 
                                return! Controls.Input ""
                            else
                                return first
                        }

    window.Content <- FormletControl.New (fun v -> ()) formlet :> obj

    let result = window.ShowDialog()

    0

