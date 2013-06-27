
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

//    let innerFormlet = Formlet.Do
//                        {
//                            let! first =    Controls.Input ""
//                                                |> Enchance.WithLabel "First"
//                            let! second =   Controls.Input ""
//                                                |> Enchance.WithLabel "Second"
//
//                            if first = "" then 
//                                let! third = Controls.Input ""
//                                                |> Enchance.WithLabel "Third"
//                                return third, second
//                            else
//                                return first, second                                              
//                        } |> Enchance.WithGroup "Testing"
//
//    let formlet = Formlet.Do
//                        {
//                            let! first =    Controls.Input ""
//                                                |> Enchance.WithLabel "First"
//
//                            let! (second, third)  =   innerFormlet
//
//                            return first,second, third
//                            
//                        }

    let inner name = Formlet.Do
                        {
                            let! first = Input "Test"
                                            |> Enchance.WithLabel "This is the test"

                            if first <> "" then 
                                return first
                            else
                                return! Input "Bogus"
                                            |> Enchance.WithLabel "This is some bogus"
                                
                            
                        }
                        |> Enchance.WithGroup name
    let formlet = Formlet.Do
                        {
                            let! first = inner "First"
                            let! second = inner "Second"

                            return first, second
                        }
//    let formlet = Formlet.Do
//                        {
//                            let! first = Input ""
//
//                            if first <> "" then 
//                                return first
//                            else
//                                return! Input "Bogus"
//                                
//                            
//                        }

    window.Content <- FormletControl.New (fun v -> ()) formlet :> obj

    let result = window.ShowDialog()

    0

