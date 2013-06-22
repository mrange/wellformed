namespace WellFormed.Core

open System

type Disposable =
    {
        Dispose : unit -> unit
    }
    interface IDisposable with

        member this.Dispose () =
            this.Dispose ()

    static member New d =
        {Dispose = d} :> IDisposable


[<AutoOpen>]
module Utils =

    let DoNothing() = ()

    let NothingToDispose() = Disposable.New DoNothing

