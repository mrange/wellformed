namespace WellFormed.Core

open System

open System.Collections.Generic

open System.Windows
open System.Windows.Controls


type Result<'T> =
    | Success of 'T
    | Failure of string list

type IForm<'T> =
    abstract member State       : IObservable<Result<'T>>
    inherit IDisposable

type Form<'T> =
    {
        State       : IObservable<Result<'T>>
        Dispose     : unit -> unit
    }
    interface IForm<'T> with
        member this.State = this.State
    interface IDisposable with
        member this.Dispose() = this.Dispose()
    static member New buildTree state dispose = {State = state; Dispose = dispose; }

