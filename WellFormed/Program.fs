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

open System.Text.RegularExpressions

open WellFormed.Core
open WellFormed

// ----------------------------------------------------------------------------
// The model for partner registration
// ----------------------------------------------------------------------------

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

type PartnerInfo = 
    {
        RegistrationCountry :   string
        Entity              :   EntityInfo
        AddressInfo         :   AddressInfo
    }

// ----------------------------------------------------------------------------
// The formlets for partner registration
//   ie the user inteaction defined
// ----------------------------------------------------------------------------

let IndividualFormlet regNoValidator = 
    Formlet.Do
        {
            let!    firstName   = NonEmpty "First name"
            let!    lastName    = NonEmpty "Last name"
            let!    regno       = Validated "Registration no" regNoValidator

            return Individual {FirstName = firstName; LastName = lastName; RegNo = regno;}
        }
        |> Enhance.WithGroup "Individual Information"

let CompanyFormlet regNoValidator = 
    Formlet.Do
        {
            let!    name        = NonEmpty "Company name"
            let!    regno       = Validated "Registration no" regNoValidator
            let!    contact     = NonEmpty "Contact"

            return Company {Name = name; RegNo = regno; Contact = contact;}
        }
        |> Enhance.WithGroup "Company Information"


let CountryFormlet =    Input.Option [|"Sweden", ("SE", SwedenRegNo); "Norway", ("NO", NorwayRegNo)|]
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
            let!    country,_       = CountryFormlet

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


let EntityFormlet individualRegNoValidator companyRegNoValidator =  
        let options  =  Input.Option [|"Individual", IndividualFormlet individualRegNoValidator; "Company",  CompanyFormlet companyRegNoValidator|]
                        |> Enhance.WithLabel "Type"
        Formlet.Do
            {
                let! option = options  
                return! option
            }

let PartnerFormlet = 
    Formlet.Do
        {
            let! registrationCountry, regNoValidator = CountryFormlet

            let! entity = EntityFormlet regNoValidator regNoValidator

            let! address = AddressFormlet

            return 
                {
                    RegistrationCountry = registrationCountry
                    Entity              = entity
                    AddressInfo         = address
                }
        }
        |> Enhance.WithErrorLog
        |> Enhance.WithSubmitAndReset
        |> Enhance.WithGroup "Partner registration"

// ----------------------------------------------------------------------------
// Opening the FormletContainer over the PartnerFormlet
// ----------------------------------------------------------------------------

[<EntryPoint>]
[<STAThread>]
let main argv = 
    let window = new Window ()
    window.MinWidth <- 600.0
    window.MinHeight <- 400.0
    window.Title <- "WellFormed App"

    let formlet = PartnerFormlet

    window.Content <- FormletContainer.New (fun v -> 
            ignore <| MessageBox.Show ("Form submitted")
        ) formlet :> obj

    let result = window.ShowDialog()

    0

