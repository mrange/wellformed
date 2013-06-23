namespace WellFormed.Core

open System

open System.Collections.Generic

open System.Windows
open System.Windows.Controls

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

        let min = max (min elements.Count (panel.Children.Count - start')) 0

        for i in 0..min - 1 do
            panel.Children.[start' + i] <- elements.[i]

        for i in min..elements.Count - 1 do
            ignore <| panel.Children.Add(elements.[i])

        let mutable i = start' + elements.Count

        for group in groups do 
            i <- group.UpdateImpl panel i

        i

    member this.Update() =  this.UpdateImpl panel 0

