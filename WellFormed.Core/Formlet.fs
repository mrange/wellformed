namespace WellFormed.Core

open System

open System.Collections.Generic

open System.Windows
open System.Windows.Controls

type Formlet<'T> = 
    {
        Rebuild : FrameworkElement -> FrameworkElement
        Collect : FrameworkElement -> Result<'T>
    }
    static member New rebuild (collect : FrameworkElement -> Result<'T>) = { Rebuild = rebuild; Collect = collect; }

module Formlet =
    
    let MapResult (m : Result<'T> -> Result<'U>) (f : Formlet<'T>) : Formlet<'U> = 
        let rebuild (ui :FrameworkElement) = f.Rebuild ui
        let collect (ui :FrameworkElement) = m (f.Collect ui)
        Formlet.New rebuild collect

    let Map (m : 'T -> 'U) (f : Formlet<'T>) : Formlet<'U> = 
        let m' r =
            match r with 
                |   Success v   -> Success (m v)
                |   Failure s   -> Failure s
        MapResult m' f

    let Join (f: Formlet<Formlet<'T>>) : Formlet<'T> = 
        let rebuild (ui :FrameworkElement) = 
            let result = CreateElement ui (fun () -> new JoinControl<Formlet<'T>> ())
            result.Left <- f.Rebuild(result.Left)
            let collect = ApplyToElement result.Left (fun ui' -> f.Collect(ui'))
            match collect with 
                |   Success f'  ->
                    result.Formlet <- Some f'
                    result.Right <- f'.Rebuild(result.Right)
                |   _           -> ()
            result :> FrameworkElement
        let collect (ui :FrameworkElement) = ApplyToElement ui (fun (ui' : JoinControl<Formlet<'T>>) -> 
                match ui'.Formlet with
                    |   Some f' ->  f'.Collect (ui'.Right)
                    |   _       ->  Fail "WellFormed.Error: Form not built up"
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


