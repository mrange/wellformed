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

namespace WellFormed2.Core

module Input = 

    type TextState = 
        {
            Text : IText
        }
        static member New text = { Text = text; }

    let Text t : IFormlet<string> = 
        let rebuild (ctx : FormletRebuildContext) (form : StatefulForm<_,_> option)  = 
            let text = 
                match form with
                | Some oldState ->  oldState.State.Text
                | _             ->  let text = ctx.CreateInstance<IText> ()
                                    text.Text <- t
                                    text

            let newState = TextState.New <| text
            let collect (state : TextState) = Success <| state.Text.Text
            let render (state : TextState) ctx  = Leaf <| state.Text.Visual
            StatefulForm<_,_>.New newState collect render
        ToFormlet <| Formlet<_,_>.New rebuild
