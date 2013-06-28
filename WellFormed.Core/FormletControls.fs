namespace WellFormed.Core

open System

open System.Collections.Generic

open System.Windows
open System.Windows.Controls
open System.Windows.Media

[<AbstractClass>]
type FormletContainerControl() =
    inherit FrameworkElement()

    static let rebuildevent = EventManager.RegisterRoutedEvent ("RebuildEvent", RoutingStrategy.Bubble, typeof<RoutedEventHandler>, typeof<FormletContainerControl>)

    static member RebuildEvent = rebuildevent

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

    static member RaiseRebuild (sender : FrameworkElement) =    let args = new RoutedEventArgs (rebuildevent, sender)
                                                                sender.RaiseEvent args
    member this.Rebuild () = FormletContainerControl.RaiseRebuild this

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

    let mutable left                : FrameworkElement  = null
    let mutable right               : FrameworkElement  = null
    let mutable orientation         : LayoutOrientation = LayoutOrientation.TopToBottom
    let mutable stretchBehavior     : StretchBehavior   = StretchBehavior.NoStretch

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

    member this.StretchBehavior
        with get ()                         = stretchBehavior
        and  set (value)                    = stretchBehavior <- value
                                              this.InvalidateArrange ()

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
        let c = this.Children
        match c with 
            |   [||]    ->  Size()
            |   [|v|]   ->  v.Measure(sz)
                            v.DesiredSize
            |   [|l;r;|]->  l.Measure(sz)
                            let sz' = ExceptUsingOrientation orientation sz l.DesiredSize
                            r.Measure(sz')
                            let result = Intersect sz (UnionUsingOrientation orientation l.DesiredSize r.DesiredSize)
                            result

    override this.ArrangeOverride(sz : Size) =
        let c = this.Children
        match c with 
            |   [||]    ->  ()
            |   [|v|]   ->  let r = TranslateUsingOrientation orientation false sz EmptyRect v.DesiredSize
                            ignore <| v.Arrange(r)
            |   [|l;r;|]->  let fillRight = stretchBehavior = StretchBehavior.RightStretches
                            let lr = TranslateUsingOrientation orientation false sz EmptyRect l.DesiredSize
                            let rr = TranslateUsingOrientation orientation fillRight sz lr r.DesiredSize
                            l.Arrange(lr)
                            r.Arrange(rr)
                            ignore <| r.Arrange(rr)
            
        sz

type JoinControl<'T>() =
    inherit BinaryControl()

    member val Formlet  : 'T option     = None                  with get, set
    member val Result   : Result<'T>    = Fail "Never built up" with get, set


type InputControl(text : string) as this =
    inherit TextBox()

    do
        this.Text   <- text
        this.Margin <- DefaultMargin

    override this.OnLostFocus(e) = 
        base.OnLostFocus(e)
        FormletContainerControl.RaiseRebuild this

type SelectControl<'T>(initial : int, options : (string * 'T) list) as this =
    inherit ComboBox()

    do
        let items = 
            options 
                |> List.map (fun (t, v) ->  let tb = new TextBlock ()
                                            tb.Text <- t
                                            let cbi = new ComboBoxItem ()
                                            cbi.Content <- tb
                                            cbi.Tag <- v
                                            cbi
                            ) 
                |> List.toArray
        this.ItemsSource <- items
        if items.Length > 0 then
            this.SelectedIndex <- Math.Min (initial, items.Length - 1)

        this.Margin <- DefaultMargin
                          
    member this.Collect () =    let item = this.SelectedValue :?> ComboBoxItem
                                if item <> null then
                                    Some (item.Tag :?> 'T)
                                else
                                    None

    override this.OnSelectionChanged(e) = 
        base.OnSelectionChanged(e)
        FormletContainerControl.RaiseRebuild this

type GroupControl(text : string) as this =
    inherit UnaryControl()

    let outer, label, inner = CreateGroup text

    do
        this.Value <- outer

    member this.Inner
        with get ()                         = inner.Child :?> FrameworkElement
        and  set (value : FrameworkElement) = inner.Child <- value

    member this.Text
        with get ()                         = label.Text
        and  set (value)                    = label.Text <- value



type LabelControl(text : string, labelWidth : double) as this =
    inherit BinaryControl()

    let label = CreateLabel text labelWidth

    do
        this.Orientation    <- LayoutOrientation.LeftToRight
        this.StretchBehavior<- StretchBehavior.RightStretches
        this.Left           <- label

    member this.Text
        with get ()                         = label.Text
        and  set (value)                    = label.Text <- value

