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

open System
open System.Collections.Generic

[<AutoOpen>]
module Types =
    type LayoutOrientation = 
        |   TopToBottom
        |   LeftToRight

    [<StructuralEquality>]
    [<StructuralComparison>]
    type Failure =
        {
            Context : string list
            Message : string
        }
        static member New (context : string list) (message : string) = { Context = context; Message = message;}

    [<StructuralEquality>]
    [<StructuralComparison>]
    type Collect<'T> =
        {
            Value       : 'T
            Failures    : Failure list
        }
        static member New (value : 'T) (failures : Failure list) = { Value = value; Failures = failures;}

    type VisualTree = 
        |   Empty
        |   Visual  of obj
        |   Fork    of LayoutOrientation*VisualTree*VisualTree

    type FormUpdateContext = 
        {
            LayoutOrientation   : LayoutOrientation
        }
        static member New layoutOrientation = { LayoutOrientation = layoutOrientation; }

    type IForm<'T> = 
        abstract member Collect : unit                          -> Collect<'T>
        abstract member Render  : FormUpdateContext             -> VisualTree

    type Form<'T> = 
        {
            Collect         : unit                          -> Collect<'T>
            Render          : FormUpdateContext             -> VisualTree
        }
        interface IForm<'T> with
            member this.Collect ()      = this.Collect ()
            member this.Render ctx      = this.Render ctx
        static member New collect render = { Collect = collect; Render = render;}

    type StatefulForm<'T, 'State> = 
        {
            State           : 'State
            Collect         : 'State                        -> Collect<'T>
            Render          : 'State -> FormUpdateContext   -> VisualTree
        }
        interface IForm<'T> with
            member this.Collect ()      = this.Collect this.State
            member this.Render ctx      = this.Render this.State ctx
        static member New state collect render = { State = state; Collect = collect; Render = render;}

    type IFormlet<'T> = 
        abstract member Rebuild : IForm<'T> option -> IForm<'T>

    type PlainFormlet<'T> = 
        {
            Rebuild     : IForm<'T> option -> IForm<'T>
        }
        interface IFormlet<'T> with
            member this.Rebuild f   =   this.Rebuild f
        static member New rebuild   = { Rebuild = rebuild; }

    type Formlet<'T, 'F when 'F :> IForm<'T>> = 
        {
            Rebuild     : 'F option    -> 'F
        }
        interface IFormlet<'T> with
            member this.Rebuild f       =   let form = 
                                                match f with
                                                | Some form ->
                                                    match form with
                                                    | :? 'F as typedForm-> this.Rebuild <| Some typedForm
                                                    | _                 -> this.Rebuild None
                                                | _                 -> this.Rebuild None
                                            upcast form 
        static member New rebuild = { Rebuild = rebuild; }

[<AutoOpen>]
module Common =
    
    let ToForm (f : #IForm<'T>) = f :> IForm<'T>
    let ToFormlet (f : #IFormlet<'T>) = f :> IFormlet<'T>

    let EmptyArray = [||]

    let Success v = Collect.New v []

    let HardFail msg            = failwith msg

    let HardFail_InvalidCase () = HardFail "WellFormed2.ProgrammmingError: This case shouldn't be reached"

    let FailWithValue value (msg : string) = Collect.New value [{Context = []; Message = msg;}]

    let Fail<'T> (msg : string)   = Collect.New Unchecked.defaultof<'T> [{Context = []; Message = msg;}]

    let Fail_NeverBuiltUp ()= Fail "WellFormed2.ProgrammmingError: Never built up"


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

