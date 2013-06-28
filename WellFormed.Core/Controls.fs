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

open System.Windows
open System.Windows.Controls

[<AutoOpen>]
module Controls =

    let Input t = 
        let rebuild (ui :FrameworkElement) = CreateElement ui (fun () -> new InputControl(t)) :> FrameworkElement
        let collect (ui :FrameworkElement) = CollectFromElement ui (fun (ui' : InputControl)-> Success ui'.Text)

        Formlet.New rebuild collect

    let Select<'T> (i : int) (options : (string * 'T)  list)  = 
        let rebuild (ui :FrameworkElement) = CreateElement ui (fun () -> new SelectControl<'T>(i, options)) :> FrameworkElement
        let collect (ui :FrameworkElement) = CollectFromElement ui (fun (ui' : SelectControl<'T>) ->    
            let c = ui'.Collect()
            match c with
            |   Some v  -> Success v
            |   None    -> Fail "Select a value"
            )

        Formlet.New rebuild collect
