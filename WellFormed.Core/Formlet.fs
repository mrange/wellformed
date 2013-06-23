namespace WellFormed.Core

open System

open System.Collections.Generic

open System.Windows
open System.Windows.Controls

type Result<'T> =
    | Success of 'T
    | Failure of string list

type ILogicalTreeBuilder =
    abstract member Add                 : UIElement -> unit
    abstract member Clear               : unit      -> unit
    abstract member NewGroupFromPanel   : Panel     -> ILogicalTreeBuilder
    abstract member NewGroup            : unit      -> ILogicalTreeBuilder

type LogicalTreeBuilder (panel : Panel) =
    let elements    = new List<UIElement>()
    let groups      = new List<LogicalTreeBuilder>()
    interface ILogicalTreeBuilder with
        member this.Add ue = if ue <> null then elements.Add(ue)
        member this.Clear () = elements.Clear()
        member this.NewGroupFromPanel innerPanel =  let g = new LogicalTreeBuilder(innerPanel)
                                                    groups.Add(g)
                                                    g :> ILogicalTreeBuilder
        member this.NewGroup () =   let g = new LogicalTreeBuilder(panel)
                                    groups.Add(g)
                                    g :> ILogicalTreeBuilder
        

    member this.UpdateImpl outerPanel start =  
                
        let start' = if Object.ReferenceEquals(panel, outerPanel) then start else 0

        let min = min elements.Count (panel.Children.Count - start')

        for i in 0..min - 1 do
            panel.Children.[start' + i] <- elements.[i]

        for i in min..elements.Count - 1 do
            ignore <| panel.Children.Add(elements.[i])

        let mutable i = start' + elements.Count

        for group in groups do 
            i <- group.UpdateImpl panel i

        i

    member this.Update() =  this.UpdateImpl panel 0

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


