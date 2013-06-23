namespace WellFormed.Core

open System

open System.Collections.Generic

open System.Windows
open System.Windows.Controls

type Formlet<'T> = 
    {
        Build : unit -> IForm<'T>
    }
    static member New (build : unit -> IForm<'T>) = { Build = build; }

module Formlet =

    let MapResult (m : Result<'T> -> Result<'U>) (f : Formlet<'T>) : Formlet<'U> = 
        let build () =
            let form = f.Build()
            
            let collect() = m (form.Collect())

            {
                BuildTree   = form.BuildTree
                Dispose     = form.Dispose
                Collect     = collect
            } :> IForm<'U>
        Formlet.New build

    let Map (m : 'T -> 'U) (f : Formlet<'T>) : Formlet<'U> = 
        let m' r =
            match r with 
                |   Success v   -> Success (m v)
                |   Failure s   -> Failure s
        MapResult m' f

    let Join (formlet: Formlet<Formlet<'T>>) : Formlet<'T> = 
        let build () =
            let form = formlet.Build()

            let innerForm() = form.Collect()

            let buildTree (t : ILogicalTreeBuilder) =   
                                let g = t.NewGroup()
                                form.BuildTree t
                                let innerCollect = form.Collect()
                                match innerCollect with 
                                    |   Success innerFormlet -> let innerForm = innerFormlet.Build()
                                                                ignore <| innerForm.BuildTree g
                                    |   _ -> ()

            let dispose() =     form.Dispose()                                            

            let collect() =     let innerCollect = form.Collect()
                                match innerCollect with 
                                    |   Success innerFormlet -> let innerForm = innerFormlet.Build()
                                                                innerForm.Collect()
                                    |   Failure f -> Failure f

               
            {
                BuildTree   = buildTree
                Dispose     = dispose
                Collect     = collect 
            } :> IForm<'T>
        Formlet.New build

    let Bind<'T1, 'T2> (f : Formlet<'T1>) (b : 'T1 -> Formlet<'T2>) : Formlet<'T2> = 
        f |> Map b |> Join

                    
    let Return (x : 'T) : Formlet<'T> = 
        let buildTree t = ()
        let collect() = Success x
        let state =          
            {
                BuildTree   = buildTree
                Dispose     = DoNothing
                Collect     = collect
            } :> IForm<'T>
        let build() = state
        Formlet.New build

    let Delay (f : unit -> Formlet<'T>) : Formlet<'T> = 
        let build () = 
            let formlet = f ()
            formlet.Build ()
        Formlet.New build

    let ReturnFrom (f : Formlet<'T>) = f

    type FormletBuilder() =
        member this.Return x = Return x
        member this.Bind(x, f) = Bind x f
        member this.Delay f = Delay f
        member this.ReturnFrom f = ReturnFrom f

    let Do = new FormletBuilder()


