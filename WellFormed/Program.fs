
open System
open System.Windows

open WellFormed.Core


type IndividualInfo =
    {
        FirstName           :   string
        LastName            :   string
        RegNo               :   string
    }

type CompanyInfo =
    {
        Name                :   string
        RegNo               :   string
        Contact             :   string
    }

type EntityInfo = 
    |   Individual      of  IndividualInfo
    |   Company         of  CompanyInfo 

let LabelInput t = Input "" |> Enchance.WithLabel t

let IndividualFormlet = 
    Formlet.Do
        {
            let!    firstName   = LabelInput "First name"
            let!    lastName    = LabelInput "Last name"
            let!    regno       = LabelInput "Registration no"

            return Individual {FirstName = firstName; LastName = lastName; RegNo = regno;}
        }
        |> Enchance.WithGroup "Individual Information"

let CompanyFormlet = 
    Formlet.Do
        {
            let!    name        = LabelInput "Company name"
            let!    regno       = LabelInput "Registration no"
            let!    contact     = LabelInput "Contact"

            return Company {Name = name; RegNo = regno; Contact = contact;}
        }
        |> Enchance.WithGroup "Company Information"


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

    let formlet = Formlet.Do
                        {
                            let! select = Select 0 ["Individual", IndividualFormlet; "Company",  CompanyFormlet]

                            return! select
                        }
                        |> Enchance.WithGroup "My test"


//    let inner name = Formlet.Do
//                        {
//                            let! first = Input "Test"
//                                            |> Enchance.WithLabel "This is the test"
//
//                            if first <> "" then 
//                                return first
//                            else
//                                return! Input "Bogus"
//                                            |> Enchance.WithLabel "This is some bogus"
//                                
//                            
//                        }
//                        |> Enchance.WithGroup name
//    let formlet = Formlet.Do
//                        {
//                            let! first = inner "First"
//                            let! second = inner "Second"
//
//                            return first, second
//                        }
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

