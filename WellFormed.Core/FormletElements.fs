// ----------------------------------------------------------------------------------------------
// Copyright (c) Mårten Rånge.
// ----------------------------------------------------------------------------------------------
// This source code is subject to terms and conditions of the Microsoft Public License. A 
// copy of the license can be found in the License.html file at the root of this distribution. 
// If you cannot locate the  Microsoft Public License, please send an email to 
// dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
//  by the terms of the Microsoft Public License.
// ----------------------------------------------------------------------------------------------
// You must not remove this notice, or any other, from this software.
// ----------------------------------------------------------------------------------------------

namespace WellFormed.Core

open System

open System.Collections.Generic
open System.Collections.ObjectModel

open System.Windows
open System.Windows.Controls
open System.Windows.Documents
open System.Windows.Input
open System.Windows.Media

[<AbstractClass>]
type FormletElement() =
    inherit FrameworkElement()

    let mutable isInitialized = false

    static let rebuildEvent     = CreateRoutedEvent<FormletElement> "Rebuild"
    static let submitEvent      = CreateRoutedEvent<FormletElement> "Submit"
    static let resetEvent       = CreateRoutedEvent<FormletElement> "Reset"

    static member RebuildEvent  = rebuildEvent
    static member SubmitEvent   = submitEvent 
    static member ResetEvent    = resetEvent  

    static member RaiseRebuild (sender : FrameworkElement) = RaiseRoutedEvent FormletElement.RebuildEvent  sender
    static member RaiseSubmit  (sender : FrameworkElement) = RaiseRoutedEvent FormletElement.SubmitEvent   sender
    static member RaiseReset   (sender : FrameworkElement) = RaiseRoutedEvent FormletElement.ResetEvent    sender

    override this.MeasureOverride(sz : Size) =
        if not isInitialized then
            isInitialized <- true
            this.OnStartUp ()
            FormletElement.RaiseRebuild this

        base.MeasureOverride sz

    abstract member OnStartUp : unit -> unit

    default this.OnStartUp () = ()

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

    member this.Rebuild () = FormletElement.RaiseRebuild this

[<AbstractClass>]
type UnaryElement() =
    inherit FormletElement()

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
        ignore <| base.MeasureOverride sz        
        let v = value
        if v <> null 
            then    v.Measure(sz)
                    v.DesiredSize
            else    Size()

    override this.ArrangeOverride(sz : Size) =
        ignore <| base.ArrangeOverride sz        
        let v = value
        if v <> null 
            then    v.Arrange(Rect(sz))
        sz



[<AbstractClass>]
type BinaryElement() =
    inherit FormletElement()

    let mutable left                : FrameworkElement  = null
    let mutable right               : FrameworkElement  = null
    let mutable orientation         : LayoutOrientation = TopToBottom
    let mutable stretchBehavior     : StretchBehavior   = NoStretch

    override this.Children 
        with    get ()   = 
            match left, right with
                |   null, null  -> [||]
                |   l,null      -> [|l|]
                |   null,r      -> [|r|]
                |   l,r         -> [|l;r;|]


    member this.Orientation                 
        with get ()                         =   orientation
        and  set (value)                    =   orientation <- value
                                                this.InvalidateMeasure ()

    member this.StretchBehavior             
        with get ()                         =   stretchBehavior
        and  set (value)                    =   stretchBehavior <- value
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
        ignore <| base.MeasureOverride sz        
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
            |   _       ->  HardFail_InvalidCase ()

    override this.ArrangeOverride(sz : Size) =
        ignore <| base.ArrangeOverride sz        
        let c = this.Children
        match c with 
            |   [||]    ->  ()
            |   [|v|]   ->  let r = TranslateUsingOrientation orientation false sz EmptyRect v.DesiredSize
                            ignore <| v.Arrange(r)
            |   [|l;r;|]->  let fillRight = stretchBehavior = RightStretches
                            let lr = TranslateUsingOrientation orientation false sz EmptyRect l.DesiredSize
                            let rr = TranslateUsingOrientation orientation fillRight sz lr r.DesiredSize
                            l.Arrange(lr)
                            r.Arrange(rr)
                            ignore <| r.Arrange(rr)
            |   _       ->  HardFail_InvalidCase ()
            
        sz

[<AutoOpen>]
module internal FormletElements =

    type JoinElement<'T>() =
        inherit BinaryElement()

        member val Formlet  : 'T option     = None                  with get, set
        member val Collect  : Collect<'T>   = Fail_NeverBuiltUp ()  with get, set


    type InputTextElement(initialText : string) as this =
        inherit TextBox()

        do
            this.Text   <- initialText
            this.Margin <- DefaultMargin

        override this.OnLostFocus(e) = 
            base.OnLostFocus(e)
            FormletElement.RaiseRebuild this

    type InputDateTimeElement(initialDate : DateTime option) as this =
        inherit DatePicker()

        do
            this.Margin <- DefaultMargin
            match initialDate with
            | Some dt -> this.SelectedDate <- new Nullable<DateTime> (dt)
            | _       -> ()

        override this.OnSelectedDateChanged(e) = 
            base.OnSelectedDateChanged(e)
            FormletElement.RaiseRebuild this

    type InputOptionElement<'T>() as this =
        inherit ComboBox()

        let itemSource = new ObservableCollection<ComboBoxItem> ()

        let mutable options : (string * 'T) array = [||]

        do
            this.Margin         <- DefaultMargin
            this.ItemsSource    <- itemSource
                          
        member this.Options 
            with get ()         =   options
            and  set (value)    =   options <- value
                                    for i in itemSource.Count..options.Length - 1 do
                                        let t,_ = options.[i]
                                        let tb = new TextBlock ()
                                        tb.Text <- t
                                        let cbi = new ComboBoxItem ()
                                        cbi.Content <- tb
                                        itemSource.Add (cbi)

                                    for i in 0..options.Length - 1 do
                                        let t,_ = options.[i]
                                        let cbi = itemSource.[i]
                                        let tb = cbi.Content :?> TextBlock
                                        tb.Text <- t
                                
                                    if this.SelectedIndex < 0 && itemSource.Count > 0 then
                                        this.SelectedIndex <- 0 

        member this.Collect ()  =   let i = this.SelectedIndex
                                    if i < 0 || i >= options.Length then None
                                    else 
                                        let _,v = options.[i]
                                        Some v

        override this.OnSelectionChanged(e) = 
            base.OnSelectionChanged(e)
            FormletElement.RaiseRebuild this

    type ManyElement(initialCount : int) as this =
        inherit BinaryElement()

        let listBox, buttons, newButton, deleteButton = CreateMany this.CanExecuteNew this.ExecuteNew this.CanExecuteDelete this.ExecuteDelete

        let inner = new ObservableCollection<FrameworkElement> ()

        do
            for i in 0..initialCount - 1 do
                inner.Add null
            this.Margin <- DefaultMargin
            this.StretchBehavior <- RightStretches
            listBox.ItemsSource <- inner
            this.Left <- buttons
            this.Right <- listBox

            FormletElement.RaiseRebuild this

        member this.ExecuteNew ()   =   inner.Add null
                                        FormletElement.RaiseRebuild this
        member this.CanExecuteNew ()=   true

        member this.ExecuteDelete ()    =   let selectedItems = listBox.SelectedItems
                                            let selection = Array.create selectedItems.Count (null :> FrameworkElement)
                                            for i in 0..selectedItems.Count - 1 do
                                                selection.[i] <- selectedItems.[i] :?> FrameworkElement

                                            for i in selectedItems.Count - 1..-1..0 do
                                                ignore <| inner.Remove(selection.[i])

                                            FormletElement.RaiseRebuild this
        member this.CanExecuteDelete () =   listBox.SelectedItems.Count > 0

        member this.Inner
            with get ()             = inner


    type LegendElement() as this =
        inherit UnaryElement()

        let outer, label, inner = CreateLegend "Group"

        do
            this.Margin <- DefaultMargin
            this.Value <- outer

        member this.Inner
            with get ()                         = inner.Child :?> FrameworkElement
            and  set (value : FrameworkElement) = inner.Child <- value

        member this.Text
            with get ()                         = label.Text
            and  set (value)                    = label.Text <- value



    type LabelElement(labelWidth : double) as this =
        inherit BinaryElement()

        let label = CreateLabel "Label"

        do
            label.Width <- labelWidth
            this.Orientation    <- LeftToRight
            this.StretchBehavior<- RightStretches
            this.Left           <- label

        member this.Text
            with get ()                         = label.Text
            and  set (value)                    = label.Text <- value

    type ErrorSummaryElement() as this =
        inherit BinaryElement()

        let label = CreateTextBlock ""

        do
            label.Foreground    <- Brushes.Red
            label.FontWeight    <- FontWeights.Bold
            this.Left           <- label

        member this.Failures
            with set (value : Failure list) =   
                label.Inlines.Clear ()
                let inlines = 
                    value
                    |>  List.collect (fun f -> 
                        [
                            new Run (f.Context |> LastOrDefault "No context")   :> Inline
                            new Run (" - ")         :> Inline
                            new Run (f.Message)     :> Inline
                            new LineBreak()         :> Inline
                        ])
                    |>  List.toArray
                label.Inlines.AddRange (inlines)
                if label.Inlines.Count = 0 then
                    label.Visibility <- Visibility.Collapsed
                else
                    label.Visibility <- Visibility.Visible

    type ErrorVisualAdorner(adornedElement) as this = 
        inherit Adorner(adornedElement)

        static let pen = CreatePen Brushes.Red 2.0

        do 
            this.IsHitTestVisible <- true

        override this.OnRender (drawingContext) =
            let rect = Rect (this.AdornedElement.RenderSize)
            drawingContext.DrawRectangle (null, pen, rect)

    type SubmitResetElement() as this =
        inherit BinaryElement()

        let submit      = CreateButton "_Submit" this.CanSubmit this.Submit
        let reset       = CreateButton "_Reset" this.CanReset this.Reset
        let stackPanel  = CreateStackPanel Orientation.Horizontal

        let mutable submitAllowed = false

        do
            ignore <| stackPanel.Children.Add(submit)
            ignore <| stackPanel.Children.Add(reset)
            this.Left <- stackPanel

        member this.SubmitAllowed   
            with get()          = submitAllowed
            and  set(value)     = submitAllowed <- value
                                  CommandManager.InvalidateRequerySuggested()

        member this.Submit ()   = FormletElement.RaiseSubmit this
        member this.CanSubmit ()= this.SubmitAllowed

        member this.Reset ()    = FormletElement.RaiseReset this
        member this.CanReset () = true

