namespace WellFormed.Core

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Threading
open System.Windows.Media

type FlatBody =
    |   Visual          of FrameworkElement
    |   LabeledVisual   of FrameworkElement*FrameworkElement

type FormletControl<'T>(action : 'T -> unit, formlet : Formlet<'T>) as this=
    inherit ContentControl()

    [<DefaultValue>] val mutable form : IForm<'T>

    let (outer, inner) = CreateGroup "Form"

    do
        this.LayoutTransform <- new ScaleTransform (1.5, 1.5)

    member this.Dispatch (action : Action) = 
        let d : Delegate = upcast action
        this.Dispatcher.BeginInvoke (DispatcherPriority.ApplicationIdle, d)

    override this.OnApplyTemplate() =   let x = this.Dispatch (fun () -> this.BuildForm())
                                        ()

    let rec FlattenBody (b : Body) : FlatBody list = 
        match b with
            |   Empty                   ->  []
            |   Element e               ->  [Visual e]
            |   Label (t,b)             ->  let flatBodies = (FlattenBody b)
                                            if (List.length flatBodies) = 0 
                                                then    []
                                                else    let head = List.head flatBodies
                                                        match head with
                                                            |   Visual e            -> [LabeledVisual (t, e)]
                                                            |   LabeledVisual (_,e) -> [LabeledVisual (t, e)]
            |   Group (f, g, b)         ->  let flatBodies = (FlattenBody b) |> List.toArray
                                            let length = flatBodies.Length

                                            g.Children.Clear()

                                            for row in g.RowDefinitions.Count..length-1 do
                                                g.RowDefinitions.Add (NewAutoRow())

                                            for row in 0..length-1 do
                                                let flatBody = flatBodies.[row]
                                                match flatBody with
                                                    |   Visual e -> Grid.SetColumnSpan(e, 2)
                                                                    Grid.SetColumn(e, 0)
                                                                    Grid.SetRow(e, row)
                                                                    ignore <| g.Children.Add(e)
                                                    |   LabeledVisual (l, e)    -> Grid.SetColumnSpan(l, 1)
                                                                                   Grid.SetColumn(l, 0)
                                                                                   Grid.SetRow(l, row)
                                                                                   Grid.SetColumnSpan(e, 1)
                                                                                   Grid.SetColumn(e, 1)
                                                                                   Grid.SetRow(e, row)
                                                                                   ignore <| g.Children.Add(l)
                                                                                   ignore <| g.Children.Add(e)
                                                


                                            [Visual f]
            |   Join (l,r)  ->  let l' = FlattenBody l
                                let r' = FlattenBody r
                                l' @ r'

    member this.BuildForm() = 
        this.form <- formlet.Build()
        let body = this.form.Body()
        let group = Group (outer, inner, body)

        this.Content <- null

        let fbs = FlattenBody group
        let fb =  List.head fbs

        match fb with
            |   Visual f -> this.Content <- f
            |   LabeledVisual (_, f) -> this.Content <- f

        ()
//        grid.ColumnDefinitions.Add(NewColumn 100.0)
//        grid.ColumnDefinitions.Add(NewStarColumn())
//        for x in 0..rows do
//            grid.RowDefinitions
//        for fe in flattened do
//            dockPanel.Children.Add(fe)
//        this.Content <- dockPanel

    //static member New (action : 'T -> unit) (formlet : Formlet<'T>) = new FormletControl<'T>(action, formlet)
    


module FormletControl =
    let New<'T> (action : 'T -> unit) (formlet : Formlet<'T>) = new FormletControl<'T>(action, formlet)