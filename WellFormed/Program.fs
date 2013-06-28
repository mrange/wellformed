// ----------------------------------------------------------------------------------------------
// Copyright (c) Mårten Rånge.
// ----------------------------------------------------------------------------------------------
// This source code is subject to terms and conditions of the Microsoft Public License. A 
// copy of the license can be found in the License.html file at the root of this distribution. 
// If you cannot locate the  Microsoft Public License, please send an email to 
// dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
//  by the terms of the Microsoft Public License.
// ----------------------------------------------------------------------------------------------
// You must not remove this notice, or any other, from this software.
// ----------------------------------------------------------------------------------------------

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


type AddressInfo =
    {
        CarryOver           :   string
        AddressLine1        :   string
        AddressLine2        :   string
        AddressLine3        :   string
        Zip                 :   string
        City                :   string
        County              :   string
        Country             :   string
    }

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


let CountryFormlet = Select 0 ["Sweden", "SE"; "Norway", "NO"; "Denmark", "DK"]
                        |> Enchance.WithLabel "Country"
let AddressFormlet = 
    Formlet.Do
        {
            let!    carryOver       = LabelInput "C/O"
            let!    addressLine1    = LabelInput "Address"
            let!    addressLine2    = LabelInput "Address"
            let!    addressLine3    = LabelInput "Address"
            let!    zip             = LabelInput "Zip"
            let!    city            = LabelInput "City"
            let!    county          = LabelInput "County"
            let!    country         = CountryFormlet

            return 
                {
                    CarryOver       = carryOver      
                    AddressLine1    = addressLine1  
                    AddressLine2    = addressLine2  
                    AddressLine3    = addressLine3  
                    Zip             = zip           
                    City            = city          
                    County          = county        
                    Country         = country       
                }
        }
        |> Enchance.WithGroup "Address Information"


let EntityFormlet = 
    Formlet.Do
        {
            let! select = Select 0 ["Individual", IndividualFormlet; "Company",  CompanyFormlet]

            return! select
        }

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
                            let! entity = EntityFormlet

                            let! address = AddressFormlet

                            return entity, address
                        }
                        |> Enchance.WithGroup "Partner registration"


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

