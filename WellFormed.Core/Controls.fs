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

module Input =

    let Text t = 
        let rebuild (ui :FrameworkElement) = CreateElement ui (fun () -> new InputTextControl(t)) :> FrameworkElement
        let collect (ui :FrameworkElement) = CollectFromElement ui (fun (ui' : InputTextControl)-> Success ui'.Text)

        Formlet.New rebuild collect

    let Option (options : (string * 'T) array)  = 
        let rebuild (ui :FrameworkElement) =    let option = CreateElement ui (fun () -> new InputOptionControl<'T>())
                                                option.Options <- options
                                                option :> FrameworkElement
        let collect (ui :FrameworkElement) = CollectFromElement ui (fun (ui' : InputOptionControl<'T>) ->    
            let c = ui'.Collect()
            match c with
            |   Some v  -> Success v
            |   None    -> Fail "Select a value"
            )

        Formlet.New rebuild collect
