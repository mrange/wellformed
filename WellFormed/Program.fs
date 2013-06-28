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

let NonEmpty t = 
    Input "" 
    |> Enhance.WithValidation_NonEmpty
    |> Enhance.WithLabel t

let AllowEmpty t = 
    Input "" 
    |> Enhance.WithLabel t

let IndividualFormlet = 
    Formlet.Do
        {
            let!    firstName   = NonEmpty "First name"
            let!    lastName    = NonEmpty "Last name"
            let!    regno       = NonEmpty "Registration no"

            return Individual {FirstName = firstName; LastName = lastName; RegNo = regno;}
        }
        |> Enhance.WithGroup "Individual Information"

let CompanyFormlet = 
    Formlet.Do
        {
            let!    name        = NonEmpty "Company name"
            let!    regno       = NonEmpty "Registration no"
            let!    contact     = NonEmpty "Contact"

            return Company {Name = name; RegNo = regno; Contact = contact;}
        }
        |> Enhance.WithGroup "Company Information"


let CountryFormlet = Select 0 ["Sweden", "SE"; "Norway", "NO"; "Denmark", "DK"]
                        |> Enhance.WithLabel "Country"
let AddressFormlet = 
    Formlet.Do
        {
            let!    carryOver       = AllowEmpty    "C/O"
            let!    addressLine1    = NonEmpty      "Address"
            let!    addressLine2    = AllowEmpty    "Address"
            let!    addressLine3    = AllowEmpty    "Address"
            let!    zip             = AllowEmpty    "Zip"
            let!    city            = NonEmpty      "City"
            let!    county          = AllowEmpty    "County"
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
        |> Enhance.WithGroup "Address Information"


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

    let formlet = Formlet.Do
                        {
                            let! entity = EntityFormlet

                            let! address = AddressFormlet

                            return entity, address
                        }
                        |> Enhance.WithErrorLog
                        |> Enhance.WithGroup "Partner registration"


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

//    let formlet = Input ""

    window.Content <- FormletControl.New (fun v -> ()) formlet :> obj

    let result = window.ShowDialog()

    0

