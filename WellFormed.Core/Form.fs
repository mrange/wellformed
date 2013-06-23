namespace WellFormed.Core

open System

open System.Collections.Generic

open System.Windows
open System.Windows.Controls


type Result<'T> =
    | Success of 'T
    | Failure of string list

type IForm<'T> =
    abstract member BuildTree   : ILogicalTreeBuilder -> unit
    abstract member Collect     : unit -> Result<'T>
    inherit IDisposable

type Form<'T> =
    {
        BuildTree   : ILogicalTreeBuilder -> unit
        Collect     : unit -> Result<'T>
        Dispose     : unit -> unit
    }
    interface IForm<'T> with
        member this.BuildTree t = this.BuildTree t
        member this.Collect() = this.Collect()
    interface IDisposable with
        member this.Dispose() = this.Dispose()
    static member New buildTree collect dispose = {BuildTree = buildTree; Collect = collect; Dispose = dispose; }

