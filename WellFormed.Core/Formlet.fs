namespace WellFormed.Core

open System

open System.Collections.Generic

open System.Windows
open System.Windows.Controls

type Formlet<'T> = 
    {
        Build : ILogicalTreeBuilder -> IForm<'T>
    }
    static member New (build : ILogicalTreeBuilder -> IForm<'T>) = { Build = build; }

module Formlet =

    let MapResult (m : Result<'T> -> Result<'U>) (f : Formlet<'T>) : Formlet<'U> = 
        let build (lt : ILogicalTreeBuilder) = 
            let form = f.Build(lt)
            
            let state = Observable.Select m form.State

            {
                Dispose     = form.Dispose
                State       = state
            } :> IForm<'U>
        Formlet.New build

    let Map (m : 'T -> 'U) (f : Formlet<'T>) : Formlet<'U> = 
        let m' r =
            match r with 
                |   Success v   -> Success (m v)
                |   Failure s   -> Failure s
        MapResult m' f

    let Join (formlet: Formlet<Formlet<'T>>) : Formlet<'T> = 
        let build (lt : ILogicalTreeBuilder) = 
            let form = formlet.Build(lt)

            let previousState : (Result<Formlet<'T>>*IForm<'T> option) option ref = ref None

            let ilt = lt.NewGroup()
            let select (result : Result<Formlet<'T>>) : IObservable<Result<'T>> = 
                match !previousState, result with 
                    |   Some (Success innerFormlet', Some innerForm')   ,   Success innerFormlet    when Object.ReferenceEquals (innerFormlet', innerFormlet) -> innerForm'.State
                    |   Some (Success innerFormlet', Some innerForm')   ,   Success innerFormlet    ->  innerForm'.Dispose()
                                                                                                        ilt.Clear()
                                                                                                        let innerForm = innerFormlet.Build(ilt)
                                                                                                        previousState := Some (Success innerFormlet, Some innerForm)
                                                                                                        innerForm.State
                    |   _                                               ,   Success innerFormlet    ->  let innerForm = innerFormlet.Build(ilt)
                                                                                                        previousState := Some (Success innerFormlet, Some innerForm)
                                                                                                        innerForm.State
                    |   _                                               ,   Failure f               ->  Observable.Return (Failure f)

//            let select (result : Result<Formlet<'T>>) : IObservable<Result<'T>> = null

            let state = form.State |> (Observable.Select select) |> Observable.Flatten


            let dispose() =     match !previousState with
                                    |   Some (_, Some innerForm')   -> innerForm'.Dispose()
                                    |   _                           -> ()
                                form.Dispose()                                            

            {
                Dispose     = dispose
                State       = state
            } :> IForm<'T>
        Formlet.New build

    let Bind<'T1, 'T2> (f : Formlet<'T1>) (b : 'T1 -> Formlet<'T2>) : Formlet<'T2> = 
        f |> Map b |> Join

                    
    let Return (x : 'T) : Formlet<'T> = 
        let state = Observable.Return (Success x)
        let form =          
            {
                Dispose     = DoNothing
                State       = state
            } :> IForm<'T>
        let build (lt : ILogicalTreeBuilder) = form
        Formlet.New build

    let Delay (f : unit -> Formlet<'T>) : Formlet<'T> = 
        let build (lt : ILogicalTreeBuilder) = 
            let formlet = f ()
            formlet.Build (lt)
        Formlet.New build

    let ReturnFrom (f : Formlet<'T>) = f

    type FormletBuilder() =
        member this.Return x = Return x
        member this.Bind(x, f) = Bind x f
        member this.Delay f = Delay f
        member this.ReturnFrom f = ReturnFrom f

    let Do = new FormletBuilder()


