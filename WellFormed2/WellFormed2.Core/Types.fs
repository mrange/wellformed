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

type FormUpdateContext = 
    {
        Collapser       : LayoutOrientation*List<obj>
    }
    static member New (orientation : LayoutOrientation) = { Collapser = orientation, List<obj> ()}

type IForm<'T> = 
    abstract member Collect : unit                          -> Collect<'T>
    abstract member Update  : FormUpdateContext             -> unit

type Form<'T> = 
    {
        Collect         : unit                          -> Collect<'T>
        Update          : FormUpdateContext             -> unit
    }
    interface IForm<'T> with
        member this.Collect ()      = this.Collect ()
        member this.Update ctx      = this.Update ctx
    static member New collect update = { Collect = collect; Update = update;}

type StatefulForm<'T, 'State> = 
    {
        State           : 'State
        Collect         : 'State                        -> Collect<'T>
        Update          : 'State -> FormUpdateContext   -> unit
    }
    interface IForm<'T> with
        member this.Collect ()      = this.Collect this.State
        member this.Update ctx      = this.Update this.State ctx
    static member New state collect update = { State = state; Collect = collect; Update = update;}

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
                                                | :? 'F as typedForm-> this.Rebuild (Some typedForm)
                                                | _                 -> this.Rebuild None
                                            | _                 -> this.Rebuild None
                                        upcast form 
    static member New rebuild = { Rebuild = rebuild; }

[<AutoOpen>]
module Utils =
    
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

    let MapResult (m : Collect<'T> -> Collect<'U>) (f : IFormlet<'T>) : IFormlet<'U> = 
        let rebuild (form : IForm<'U> option)  = 
            let collect ()  = m (f.Collect ())
            let update ctx  = f.Update ctx
            ToForm <| StatefulForm<'U>.New () collect update 
        Formlet<'U>.New rebuild

    let Map (mapper : 'T -> 'U) (f : IFormlet<'T>) : IFormlet<'U> = 
        let m collect = { Value = mapper collect.Value ; Failures = collect.Failures; }
        MapResult m f

    let Return (x : 'T) : IFormlet<'T> = 
        let rebuild (form : Form<'T> option)  = 
            let collect ()  = Success x
            let update ctx  = ()
            Form<_>.New collect update 
        ToFormlet <| Formlet<_,_>.New rebuild

    let Delay (f : unit -> IFormlet<'T>) : IFormlet<'T> = 
        let f' = lazy (f())
        let rebuild (form : IForm<'T> option)  = f'.Value.Rebuild form
        ToFormlet <| PlainFormlet<_>.New rebuild

    let ReturnFrom (f : IFormlet<'T>) = f

//    [<Sealed>]
//    type FormletBuilder() =
//        member this.Return      x       = Return        x
//        member this.Bind        (x, f)  = Bind          x f
//        member this.Delay       f       = Delay         f
//        member this.ReturnFrom  f       = ReturnFrom    f
//
//    let Do = FormletBuilder()
//
