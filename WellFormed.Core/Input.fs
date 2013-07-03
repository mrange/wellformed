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

namespace WellFormed.Core

open System
open System.Windows
open System.Windows.Controls

module Input =

    let Text t = 
        let rebuild (ui :FrameworkElement) = CreateElement ui (fun () -> new InputTextElement(t)) :> FrameworkElement
        let collect (ui :FrameworkElement) = CollectFromElement ui (fun (ui' : InputTextElement)-> Success ui'.Text)

        Formlet.New rebuild collect

    let DateTime d = 
        let failDate = DateTime.Today
        let rebuild (ui :FrameworkElement) = CreateElement ui (fun () -> new InputDateTimeElement(d)) :> FrameworkElement
        let collect (ui :FrameworkElement) = CollectFromElement ui (fun (ui' : InputDateTimeElement)-> 
            let selectedDate = ui'.SelectedDate
            if selectedDate.HasValue then Success selectedDate.Value
            else FailWithValue failDate "Select a date")

        Formlet.New rebuild collect

    let Option (options : (string * 'T) array)  = 
        let rebuild (ui :FrameworkElement) =    let option = CreateElement ui (fun () -> new InputOptionElement<'T>())
                                                option.Options <- options
                                                option :> FrameworkElement
        let collect (ui :FrameworkElement) = CollectFromElement ui (fun (ui' : InputOptionElement<'T>) ->    
            let c = ui'.Collect()
            match c with
            |   Some v  -> Success v
            |   None    -> Fail "Select a value"
            )

        Formlet.New rebuild collect
