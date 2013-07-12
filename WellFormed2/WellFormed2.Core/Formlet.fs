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

module Formlet =

    type MapCollectState<'T> = 
        {   
            Form    :   IForm<'T>
        }
        static member New form = { Form = form }

    let MapCollect (m : Collect<'T> -> Collect<'U>) (f : IFormlet<'T>) : IFormlet<'U> = 
        let rebuild (form : StatefulForm<'U, MapCollectState<'T>> option)  = 
            let oldForm = 
                match form with
                | Some oldState ->  Some oldState.State.Form
                | _             ->  None

            let newState = MapCollectState<_>.New <| f.Rebuild oldForm
            let collect (state : MapCollectState<'T>)     = m <| state.Form.Collect ()
            let render (state : MapCollectState<'T>) ctx  = state.Form.Render ctx
            StatefulForm<_,_>.New newState collect render
        ToFormlet <| Formlet<_,_>.New rebuild

    let Map (mapper : 'T -> 'U) (f : IFormlet<'T>) : IFormlet<'U> = 
        let m collect = { Value = mapper collect.Value ; Failures = collect.Failures; }
        MapCollect m f

    type JoinState<'T> = 
        {   
            Left            :   IForm<IFormlet<'T>>
            Right           :   IForm<'T>
        }
        static member New left right = { Left = left; Right = right; }

    let Join (f: IFormlet<IFormlet<'T>>) : IFormlet<'T> =
        let rebuild (form : StatefulForm<'T, JoinState<'T>> option)  = 
            let left, right = 
                match form with
                | Some oldState ->  Some oldState.State.Left, Some oldState.State.Right
                | _             ->  None, None
            let lform       = f.Rebuild left
            let rformlet    = lform.Collect ()
            let rform       = rformlet.Value.Rebuild right

            let newState    = JoinState<_>.New lform rform
            let collect (state : JoinState<'T>)     = state.Right.Collect ()
            let render (state : JoinState<'T>) ctx  = 
                let l = state.Left.Render ctx
                let r = state.Right.Render ctx
                Fork (ctx.LayoutOrientation, l, r)
            StatefulForm<_,_>.New newState collect render
        ToFormlet <| Formlet<_,_>.New rebuild

    let Bind (f : IFormlet<'T1>) (b : 'T1 -> IFormlet<'T2>) : IFormlet<'T2> = 
        f |> Map b |> Join

    let Return (x : 'T) : IFormlet<'T> = 
        let rebuild (form : Form<'T> option)  = 
            let collect ()  = Success x
            let render ctx  = Empty
            Form<_>.New collect render
        ToFormlet <| Formlet<_,_>.New rebuild

    let Delay (f : unit -> IFormlet<'T>) : IFormlet<'T> = 
        let f' = lazy (f())
        let rebuild (form : IForm<'T> option)  = f'.Value.Rebuild form
        ToFormlet <| PlainFormlet<_>.New rebuild

    let ReturnFrom (f : IFormlet<'T>) = f

    [<Sealed>]
    type FormletBuilder() =
        member this.Return      x       = Return        x
        member this.Bind        (x, f)  = Bind          x f
        member this.Delay       f       = Delay         f
        member this.ReturnFrom  f       = ReturnFrom    f

    let Do = FormletBuilder()

