namespace WellFormed.Core

open System

open System.Collections.Generic

open System.Windows
open System.Windows.Controls
open System.Windows.Media

[<AbstractClass>]
type FormletContainerControl() =
    inherit FrameworkElement()

    abstract Children : array<FrameworkElement> with get

    override this.LogicalChildren = this.Children |> Enumerator

    override this.VisualChildrenCount = this.Children.Length

    override this.GetVisualChild (i : int) = this.Children.[i] :> Visual

    member this.RemoveChild (fe : FrameworkElement) =
            this.RemoveVisualChild(fe)
            this.RemoveLogicalChild(fe)

    member this.AddChild (fe : FrameworkElement) =
            this.AddLogicalChild(fe)
            this.AddVisualChild(fe)

type DelayControl() =
    inherit FormletContainerControl()

    let mutable value : FrameworkElement = null

    override this.Children 
        with    get ()   = if value <> null then [|value|] else [||]

    member this.Value 
        with    get ()                      = value
        and     set (fe : FrameworkElement)  = 
            if not (Object.ReferenceEquals (value,fe)) then    
                this.RemoveChild(value)
                value <- fe
                this.AddChild(value)
                this.InvalidateMeasure()

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
    inherit FormletContainerControl()

    let mutable left    : FrameworkElement = null
    let mutable right   : FrameworkElement = null

    override this.Children 
        with    get ()   = 
            match left, right with
                |   null, null  -> [||]
                |   l,null      -> [|l|]
                |   null,r      -> [|r|]
                |   l,r         -> [|l;r;|]


    member this.Left 
        with    get ()                      = left
        and     set (fe : FrameworkElement)  = 
            if not (Object.ReferenceEquals (left,fe)) then    
                this.RemoveChild(left)
                left <- fe
                this.AddChild(left)
                this.InvalidateMeasure()

    member this.Right 
        with    get ()                      = right
        and     set (fe : FrameworkElement)  = 
            if not (Object.ReferenceEquals (right,fe)) then    
                this.RemoveChild(right)
                right <- right
                this.AddChild(right)
                this.InvalidateMeasure()

    member val Formlet : 'T option = None with get, set

    override this.LogicalChildren = this.Children |> Enumerator

    override this.VisualChildrenCount = this.Children.Length

    override this.GetVisualChild (i : int) = this.Children.[i] :> Visual

    override this.MeasureOverride(sz : Size) =
        let adjustedSize = Size (sz.Width, Double.PositiveInfinity)
        let c = this.Children
        match c with 
            |   [||]    ->  Size()
            |   [|v|]   ->  v.Measure(adjustedSize)
                            v.DesiredSize
            |   [|l;r;|]->  l.Measure(adjustedSize)
                            r.Measure(adjustedSize)
                            CombineVertically sz l.DesiredSize r.DesiredSize

    override this.ArrangeOverride(sz : Size) =
        let c = this.Children
        match c with 
            |   [||]    ->  ()
            |   [|v|]   ->  let r = Rect (0.0, 0.0, sz.Width, v.DesiredSize.Height)
                            ignore <| v.Arrange(r)
            |   [|l;r;|]->  let lr = Rect (0.0, 0.0, sz.Width, l.DesiredSize.Height)
                            let rr = Rect (0.0, l.DesiredSize.Height, sz.Width, r.DesiredSize.Height)
                            l.Arrange(lr)
                            r.Arrange(rr)
                            ignore <| r.Arrange(rr)
            
        sz



type InputControl(text : string) as this =
    inherit TextBox()

    do
        this.Text <- text
        this.Margin <- DefaultMargin

