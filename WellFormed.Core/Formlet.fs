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

open System.Collections.Generic

open System.Windows
open System.Windows.Controls

(*  The semantics of a formlet
    --------------------------

    A formlet has two tasks:
        1. Rebuilding the UI
        2. Collecting data from the UI

    It has to methods for that purpose

    1. Rebuild
        A formlet is supposed to rebuild a UI from an existing UI state
        The reason is that during input the UI might need to be changed dramatically
        but we like to preserve the user input as far as possible

        As input the formlet gets the FrameworkElement that occupy the same position in the 
        visual tree that UI of the current formlet will be placed

        The formlet is supposed to return a FrameworkElement, preferably the input is preserved
        but updated otherwise return a new instance (loses user input)

        There are basically tree cases that needs to be handled
        a.  Input is null
            No UI has been created in this position and needs creating from scratch
        b.  Input is not null but non-matching type
            UI has been created in the current position but are being replaced and needs creating from scratch 
        c.  Input is not null and the type matches
            UI of the same type has been created in the current position. It's strongly recommend to preserve
            the user input but other mutable state should be updated accordingly

        Typically a rebuild action could look like this:
        let rebuild (ui : FrameworkElement) =   let option = CreateElement (fun () -> new InputOptionElement<'T>()) ui 
                                                option.Options <- options
                                                option :> FrameworkElement

        The method CreateElement creates a new InputOptionElement if the input UI doesn't match, 
        otherwise returns existing instance.

        The mutable state of the InputOptionElement control is updated, 
        if the state is identical it shouldn't update the control

        Finally the control is returned

    2. Collect
        A formlet is supposed to collect the user input from the UI on demand.

        As input the formlet gets the FrameworkElement that corresponds the Formlet.

        The formlet is supposed to return a Collect<'T>. Collet<'T> consists of two-parts
            1.  Value - The input generated from the formlet. 
                The formlet should strive to produce a value even though the input doesn't
                fully validate.
            2.  Failures - Zero or more failures that happened during the collection 
                of input. This is the way formlets signals to the container whether state
                is invalid or not. 

        There are basically tree cases that needs to be handled
        a.  Input is null
            This is in an error case and the Formlet should return a failure (don't throw)
        b.  Input is not null but non-matching type
            This is in an error case and the Formlet should return a failure (don't throw)
        c.  Input is not null and the type matches
            The formlet should collect the user input and return it

        Typically a collect action could look like this:

        let collect (fe : FrameworkElement) = CollectFromElement ui (fun (ui : InputOptionElement<'T>) ->    
            let c = ui.Collect()
            match c with
            |   Some v  -> Success v
            |   None    -> Fail "Select a value"
            )

        The method CollectFromElement applies the action if the UI element is the expected,
        otherwise it returns a failure.

        The result is tested to see if it has a value and returns that, otherwise a failure is returned


        These are the semantics of Formlet<'T>
*)
type Formlet<'T> = 
    {
        Rebuild : FrameworkElement -> FrameworkElement
        Collect : FrameworkElement -> Collect<'T>
    }
    static member New rebuild (collect : FrameworkElement -> Collect<'T>) = { Rebuild = rebuild; Collect = collect;}

module Formlet =
    
    let MapResult (m : Collect<'T> -> Collect<'U>) (f : Formlet<'T>) : Formlet<'U> = 
        let rebuild (ui : FrameworkElement) = f.Rebuild ui
        let collect (ui : FrameworkElement) = m (f.Collect ui)
        Formlet.New rebuild collect

    let Map (m : 'T -> 'U) (f : Formlet<'T>) : Formlet<'U> = 
        let m' collect = { Value = m collect.Value ; Failures = collect.Failures; }
        MapResult m' f

    let Join (f: Formlet<Formlet<'T>>) : Formlet<'T> = 
        let rebuild (fe : FrameworkElement) = 
            let result = fe |> CreateElement (fun () -> new JoinElement<Formlet<'T>> ()) 
            result.Left <- f.Rebuild result.Left
            let collect = f.Collect result.Left
            result.Collect <- collect
            let f' = collect.Value
            result.Formlet  <- Some f'
            result.Right    <- f'.Rebuild result.Right
            result :> FrameworkElement
        let collect (fe : FrameworkElement) = fe |> CollectFromElement (fun (ui : JoinElement<Formlet<'T>>) -> 
                match ui.Formlet with
                |   Some f' -> JoinFailures ui.Collect (f'.Collect (ui.Right))
                |   None    -> JoinFailures ui.Collect (Fail_NeverBuiltUp ())
                )
        Formlet.New rebuild collect

    let Bind (f : Formlet<'T1>) (b : 'T1 -> Formlet<'T2>) : Formlet<'T2> = 
        f |> Map b |> Join

    let Return (x : 'T) : Formlet<'T> = 
        let rebuild (ui : FrameworkElement) = null
        let collect (ui : FrameworkElement) = Success x
        Formlet.New rebuild collect

    let Delay (f : unit -> Formlet<'T>) : Formlet<'T> = 
        let f' = lazy (f())
        let rebuild (ui : FrameworkElement) = f'.Value.Rebuild ui
        let collect (ui : FrameworkElement) = f'.Value.Collect ui
        Formlet.New rebuild collect

    let ReturnFrom (f : Formlet<'T>) = f

    [<Sealed>]
    type FormletBuilder() =
        member this.Return      x       = Return        x
        member this.Bind        (x, f)  = Bind          x f
        member this.Delay       f       = Delay         f
        member this.ReturnFrom  f       = ReturnFrom    f

    let Do = FormletBuilder()

