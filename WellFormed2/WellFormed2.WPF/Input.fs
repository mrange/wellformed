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

namespace WellFormed2.WPF

open System.Windows
open System.Windows.Controls

open WellFormed2.Core

module Input = 

    type TextState = 
        {
            TextBox : TextBox
        }
        static member New textBox = { TextBox = textBox; }

    let Text t : IFormlet<string> = 
        let rebuild (form : StatefulForm<_,_> option)  = 
            let textBox = 
                match form with
                | Some oldState ->  oldState.State.TextBox
                | _             ->  let tb = new TextBox ()
                                    tb.Text <- t
                                    tb

            let newState = TextState.New <| textBox
            let collect (state : TextState) = Success state.TextBox.Text
            let render (state : TextState) ctx  = Visual state.TextBox
            StatefulForm<_,_>.New newState collect render
        ToFormlet <| Formlet<_,_>.New rebuild
