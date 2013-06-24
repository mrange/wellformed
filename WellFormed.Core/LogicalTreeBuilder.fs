namespace WellFormed.Core

open System

open System.Collections.Generic

open System.Windows
open System.Windows.Controls
open System.Windows.Threading

type ILogicalTreeBuilder =
    abstract member Add                 : UIElement -> unit
    abstract member Clear               : unit      -> unit
    abstract member NewGroupFromPanel   : Panel     -> ILogicalTreeBuilder
    abstract member NewGroup            : unit      -> ILogicalTreeBuilder

type LogicalTreeRoot (dispatcher : Dispatcher, action : unit -> unit) = 
    [<DefaultValue>] val mutable isDispatching  : bool

    member this.Dispatch() = 
        action()
        this.isDispatching <- false

    member this.UpdateLogicalTree () =
        if not this.isDispatching then
            this.isDispatching <- true
            Dispatch dispatcher this.Dispatch

and LogicalTreeBuilder (root : LogicalTreeRoot, panel : Panel) =
    let elements    = new List<UIElement>()
    let groups      = new List<LogicalTreeBuilder>()
    interface ILogicalTreeBuilder with
        member this.Add ue =    if ue <> null then 
                                    elements.Add(ue)
                                    root.UpdateLogicalTree()
        member this.Clear () =  elements.Clear()
                                root.UpdateLogicalTree()
        member this.NewGroupFromPanel innerPanel =  let g = new LogicalTreeBuilder(root, innerPanel)
                                                    groups.Add(g)
                                                    root.UpdateLogicalTree()
                                                    g :> ILogicalTreeBuilder
        member this.NewGroup () =   let g = new LogicalTreeBuilder(root, panel)
                                    groups.Add(g)
                                    g :> ILogicalTreeBuilder
        

    member this.UpdateImpl outerPanel start =  
                
        let newPanel = not (Object.ReferenceEquals(panel, outerPanel))
        let start' = if newPanel then 0 else start 

        let min = max (min elements.Count (panel.Children.Count - start')) 0

        for i in 0..min - 1 do
            let currentElement = panel.Children.[start' + i]
            let element = elements.[i]
            panel.Children.[start' + i] <- element

        for i in min..elements.Count - 1 do
            ignore <| panel.Children.Add(elements.[i])

        let mutable ig = start' + elements.Count

        for group in groups do 
            ig <- group.UpdateImpl panel ig

        if newPanel && ig < panel.Children.Count then
            panel.Children.RemoveRange(ig, panel.Children.Count - ig)

        ig

    member this.Update() =  this.UpdateImpl null 0

