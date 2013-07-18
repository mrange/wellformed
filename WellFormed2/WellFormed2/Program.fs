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

open WellFormed2.Core
open WellFormed2.WPF

type CustomerInfo =
    {
        FirstName   :   string
        LastName    :   string
    }

let CustomerFormlet = 
    Formlet.Do
        {
            let! firstName = Input.Text ""
            let! lastName = Input.Text ""

            return {FirstName = firstName; LastName = lastName; }
        }

[<EntryPoint>]
[<STAThread>]
let main argv = 
    let window = new Window ()
    window.MinWidth <- 600.0
    window.MinHeight <- 400.0
    window.Title <- "WellFormed2 App"

    let formlet = CustomerFormlet

    let control = new FormletControl ()

    control.ShowFormlet (formlet, fun ci -> ())

    window.Content <- control

    let result = window.ShowDialog()    

    0
