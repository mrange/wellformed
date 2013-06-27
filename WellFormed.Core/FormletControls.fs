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

[<AbstractClass>]
type UnaryControl() =
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

[<AbstractClass>]
type BinaryControl() =
    inherit FormletContainerControl()

    let mutable left        : FrameworkElement  = null
    let mutable right       : FrameworkElement  = null
    let mutable orientation : LayoutOrientation = LayoutOrientation.TopToBottom

    override this.Children 
        with    get ()   = 
            match left, right with
                |   null, null  -> [||]
                |   l,null      -> [|l|]
                |   null,r      -> [|r|]
                |   l,r         -> [|l;r;|]


    member this.Orientation
        with get ()                         = orientation
        and  set (value)                    = orientation <- value
                                              this.InvalidateMeasure ()



    member this.Left 
        with    get ()                      = left
        and     set (fe : FrameworkElement) = 
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
                right <- fe
                this.AddChild(right)
                this.InvalidateMeasure()

    override this.LogicalChildren = this.Children |> Enumerator

    override this.VisualChildrenCount = this.Children.Length

    override this.GetVisualChild (i : int) = this.Children.[i] :> Visual

    override this.MeasureOverride(sz : Size) =
        let adjustedSize = AdjustUsingOrientation orientation sz
        
        let c = this.Children
        match c with 
            |   [||]    ->  Size()
            |   [|v|]   ->  v.Measure(adjustedSize)
                            v.DesiredSize
            |   [|l;r;|]->  l.Measure(adjustedSize)
                            r.Measure(adjustedSize)
                            CombineUsingOrientation orientation sz l.DesiredSize r.DesiredSize

    override this.ArrangeOverride(sz : Size) =
        let c = this.Children
        match c with 
            |   [||]    ->  ()
            |   [|v|]   ->  let r = MakeFirstRectUsingOrientation orientation sz v
                            ignore <| v.Arrange(r)
            |   [|l;r;|]->  let lr = MakeFirstRectUsingOrientation orientation sz l
                            let rr = MakeSecondRectUsingOrientation orientation sz l r
                            l.Arrange(lr)
                            r.Arrange(rr)
                            ignore <| r.Arrange(rr)
            
        sz

type DelayControl() =
    inherit UnaryControl()

type JoinControl<'T>() =
    inherit BinaryControl()

    member val Formlet : 'T option = None with get, set


type InformationControl(text : string) as this =
    inherit TextBlock()

    do
        this.Text <- text
        this.Margin <- DefaultMargin


type InputControl(text : string) as this =
    inherit TextBox()

    do
        this.Text <- text
        this.Margin <- DefaultMargin

type GroupControl(text : string) as this =
    inherit UnaryControl()

    let outer, inner = CreateGroup text

    do
        this.Value <- outer

    member this.Inner
        with get ()                         = inner.Child :?> FrameworkElement
        and  set (value : FrameworkElement) = inner.Child <- value

