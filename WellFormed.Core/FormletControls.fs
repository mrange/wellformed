namespace WellFormed.Core

open System

open System.Collections.Generic

open System.Windows
open System.Windows.Controls
open System.Windows.Media

type DelayControl() =
    inherit FrameworkElement()

    let mutable value : FrameworkElement = null

    let mutable children = [||]

    member this.UpdateChildren() = 
        if value <> null 
            then    children    <- [|value|]
            else    children    <- [||]

    member this.Value 
        with    get ()                      = value
        and     set (v : FrameworkElement)  = 
            if not (Object.ReferenceEquals (value,v)) 
                then    this.RemoveVisualChild(value)
                        this.RemoveLogicalChild(value)
                        value <- v
                        this.AddLogicalChild(value)
                        this.AddVisualChild(value)
                        this.InvalidateMeasure()

            this.UpdateChildren()
            


    override this.LogicalChildren = children |> Enumerator

    override this.VisualChildrenCount = children.Length

    override this.GetVisualChild (i : int) = children.[i] :> Visual

    override this.MeasureOverride(sz : Size) =
        let v = value
        if v <> null 
            then    v.Measure(sz)
                    v.DesiredSize
            else    Size()

    override this.ArrangeOverride(sz : Size) =
        let v = value
        if v <> null 
            then    v.Arrange(Rect(sz))
        sz

type JoinControl<'T>() =
    inherit FrameworkElement()

    let mutable left    : FrameworkElement = null
    let mutable right   : FrameworkElement = null

    let mutable children = [||]

    member this.UpdateChildren() = 
        match left, right with
            |   null,null   ->  children <- [||]
            |   null,r      ->  children <- [|r|]     
            |   l,null      ->  children <- [|l|]     
            |   l,r         ->  children <- [|l;r;|]  

    member this.Left 
        with    get ()                      = left
        and     set (v : FrameworkElement)  = 
            if not (Object.ReferenceEquals (left,v)) 
                then    this.RemoveVisualChild(left)
                        this.RemoveLogicalChild(left)
                        left <- v
                        this.AddLogicalChild(left)
                        this.AddVisualChild(left)
                        this.InvalidateMeasure()
            this.UpdateChildren()

    member this.Right 
        with    get ()                      = right
        and     set (v : FrameworkElement)  = 
            if not (Object.ReferenceEquals (right,v)) 
                then    this.RemoveVisualChild(right)
                        this.RemoveLogicalChild(right)
                        right <- v
                        this.AddLogicalChild(right)
                        this.AddVisualChild(right)
                        this.InvalidateMeasure()
            this.UpdateChildren()

    member val Formlet : 'T option = None with get, set

    override this.LogicalChildren = children |> Enumerator

    override this.VisualChildrenCount = children.Length

    override this.GetVisualChild (i : int) = children.[i] :> Visual

    override this.MeasureOverride(sz : Size) =
        let c = children
        match c with 
            |   [||]    ->  Size()
            |   [|v|]   ->  v.Measure(sz)
                            v.DesiredSize
            |   [|l;r;|]->  let adjustedSize = Size (sz.Width, Double.PositiveInfinity)
                            l.Measure(adjustedSize)
                            r.Measure(adjustedSize)
                            CombineVertically sz l.DesiredSize r.DesiredSize

    override this.ArrangeOverride(sz : Size) =
        let c = children
        match c with 
            |   [||]    ->  ()
            |   [|v|]   ->  ignore <| v.Arrange(Rect(sz))
            |   [|l;r;|]->  let lr = Rect (0.0, 0.0, sz.Width, l.DesiredSize.Height)
                            let rr = Rect (0.0, l.DesiredSize.Height, sz.Width, r.DesiredSize.Height)
                            l.Arrange(lr)
                            ignore <| r.Arrange(rr)
        sz



type InputControl(text : string) as this =
    inherit TextBox()

    do
        this.Text <- text
        this.Margin <- DefaultMargin

