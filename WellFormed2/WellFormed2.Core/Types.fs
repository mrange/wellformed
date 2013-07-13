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
        |   Leaf    of obj
        |   Fork    of LayoutOrientation*VisualTree*VisualTree

    type FlatVisualTree = 
        |   Visual  of obj
        |   Layout  of LayoutOrientation*(FlatVisualTree array)

    type IText = 
        abstract member Text    : string    with get, set
        abstract member Visual  : obj       with get

    type IFormletRebuildContext = 
        abstract member CreateInstance<'T>  : unit -> 'T

    type FormRenderContext = 
        {
            LayoutOrientation   : LayoutOrientation
        }
        static member New layoutOrientation = { LayoutOrientation = layoutOrientation; }

    type IForm<'T> = 
        abstract member Collect : unit                          -> Collect<'T>
        abstract member Render  : FormRenderContext             -> VisualTree

    type Form<'T> = 
        {
            Collect         : unit                          -> Collect<'T>
            Render          : FormRenderContext             -> VisualTree
        }
        interface IForm<'T> with
            member this.Collect ()      = this.Collect ()
            member this.Render ctx      = this.Render ctx
        static member New collect render = { Collect = collect; Render = render;}

    type StatefulForm<'T, 'State> = 
        {
            State           : 'State
            Collect         : 'State                        -> Collect<'T>
            Render          : 'State -> FormRenderContext   -> VisualTree
        }
        interface IForm<'T> with
            member this.Collect ()      = this.Collect this.State
            member this.Render ctx      = this.Render this.State ctx
        static member New state collect render = { State = state; Collect = collect; Render = render;}

    type IFormlet<'T> = 
        abstract member Rebuild : IFormletRebuildContext -> IForm<'T> option -> IForm<'T>

    type PlainFormlet<'T> = 
        {
            Rebuild     : IFormletRebuildContext -> IForm<'T> option -> IForm<'T>
        }
        interface IFormlet<'T> with
            member this.Rebuild ctx f   = this.Rebuild ctx f
        static member New rebuild       = { Rebuild = rebuild; }

    type Formlet<'T, 'F when 'F :> IForm<'T>> = 
        {
            Rebuild     : IFormletRebuildContext -> 'F option -> 'F
        }
        interface IFormlet<'T> with
            member this.Rebuild ctx f   =   let form = 
                                                match f with
                                                | Some form ->
                                                    match form with
                                                    | :? 'F as typedForm-> this.Rebuild ctx (Some typedForm)
                                                    | _                 -> this.Rebuild ctx None
                                                | _                 -> this.Rebuild ctx None
                                            upcast form 
        static member New rebuild = { Rebuild = rebuild; }

