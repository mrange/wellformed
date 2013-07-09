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
        let rebuild (fe :FrameworkElement) = fe |> CreateElement (fun () -> new InputTextElement(t)) :> FrameworkElement
        let collect (fe :FrameworkElement) = fe |> CollectFromElement (fun (ui : InputTextElement)-> Success ui.Text)

        Formlet.New rebuild collect

    let Integer v = 
        let map (collect : Collect<string>) : Collect<int> =
            if collect.Failures.Length > 0 then
                Collect.New 0 collect.Failures
            else
                let i = ref 0
                if Int32.TryParse (collect.Value, i) then
                    Success !i
                else
                    Fail "Input is not an integer" 
        Text (v.ToString())
        |> Formlet.MapResult map
    let DateTime d = 
        let failDate = DateTime.Today
        let rebuild (fe :FrameworkElement) = fe |> CreateElement (fun () -> new InputDateTimeElement(d)) :> FrameworkElement
        let collect (fe :FrameworkElement) = fe |> CollectFromElement (fun (ui : InputDateTimeElement)-> 
            let selectedDate = ui.SelectedDate
            if selectedDate.HasValue then Success selectedDate.Value
            else FailWithValue failDate "Select a date")

        Formlet.New rebuild collect

    let Option (options : (string * 'T) array)  = 
        let rebuild (fe :FrameworkElement) =    let option = fe |> CreateElement (fun () -> new InputOptionElement<'T>())
                                                option.Options <- options
                                                option :> FrameworkElement
        let collect (fe :FrameworkElement) = fe |> CollectFromElement (fun (ui : InputOptionElement<'T>) ->    
            let c = ui.Collect()
            match c with
            |   Some v  -> Success v
            |   None    -> Fail "Select a value"
            )

        Formlet.New rebuild collect
