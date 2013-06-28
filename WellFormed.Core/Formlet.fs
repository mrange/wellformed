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

type Formlet<'T> = 
    {
        Rebuild : FrameworkElement -> FrameworkElement
        Collect : FrameworkElement -> Collect<'T>
    }
    static member New rebuild (collect : FrameworkElement -> Collect<'T>) = { Rebuild = rebuild; Collect = collect;}

module Formlet =
    
    let MapResult (m : Collect<'T> -> Collect<'U>) (f : Formlet<'T>) : Formlet<'U> = 
        let rebuild (ui :FrameworkElement) = f.Rebuild ui
        let collect (ui :FrameworkElement) = m (f.Collect ui)
        Formlet.New rebuild collect

    let Map (m : 'T -> 'U) (f : Formlet<'T>) : Formlet<'U> = 
        let m' collect =
            match collect.Value with 
            |   Some v  -> { Value = Some (m v) ; Failures = collect.Failures; }
            |   None    -> { Value = None       ; Failures = collect.Failures; }
        MapResult m' f

    let Join (f: Formlet<Formlet<'T>>) : Formlet<'T> = 
        let rebuild (ui :FrameworkElement) = 
            let result = CreateElement ui (fun () -> new JoinControl<Formlet<'T>> ())
            result.Left <- f.Rebuild(result.Left)
            let collect = f.Collect(result.Left)
            result.Collect <- collect
            match collect.Value with 
            |   Some f'  ->
                result.Formlet  <- Some f'
                result.Right    <- f'.Rebuild(result.Right)
            |   _           -> ()
            result :> FrameworkElement
        let collect (ui :FrameworkElement) = CollectFromElement ui (fun (ui' : JoinControl<Formlet<'T>>) -> 
                match ui'.Formlet with
                |   Some f' -> JoinFailures ui'.Collect (f'.Collect (ui'.Right))
                |   None    -> JoinFailures ui'.Collect (Fail_NeverBuiltUp ())
                )
        Formlet.New rebuild collect

    let Bind<'T1, 'T2> (f : Formlet<'T1>) (b : 'T1 -> Formlet<'T2>) : Formlet<'T2> = 
        f |> Map b |> Join


    let Return (x : 'T) : Formlet<'T> = 
        let rebuild (ui :FrameworkElement) = null
        let collect (ui :FrameworkElement) = Success x
        Formlet.New rebuild collect

    let Delay (f : unit -> Formlet<'T>) : Formlet<'T> = 
        let f' = lazy (f())
        let rebuild (ui :FrameworkElement) = f'.Value.Rebuild(ui)
        let collect (ui :FrameworkElement) = f'.Value.Collect(ui)
        Formlet.New rebuild collect

    let ReturnFrom (f : Formlet<'T>) = f

    type FormletBuilder() =
        member this.Return x = Return x
        member this.Bind(x, f) = Bind x f
        member this.Delay f = Delay f
        member this.ReturnFrom f = ReturnFrom f

    let Do = new FormletBuilder()


