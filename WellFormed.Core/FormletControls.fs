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
type FormletControl() =
    inherit FrameworkElement()

    let mutable isInitialized = false

    static let rebuildEvent     = CreateRoutedEvent<FormletControl> "Rebuild"
    static let submitEvent      = CreateRoutedEvent<FormletControl> "Submit"
    static let resetEvent       = CreateRoutedEvent<FormletControl> "Reset"

    static member RebuildEvent  = rebuildEvent
    static member SubmitEvent   = submitEvent 
    static member ResetEvent    = resetEvent  

    static member RaiseRebuild (sender : FrameworkElement) = RaiseRoutedEvent FormletControl.RebuildEvent  sender
    static member RaiseSubmit  (sender : FrameworkElement) = RaiseRoutedEvent FormletControl.SubmitEvent   sender
    static member RaiseReset   (sender : FrameworkElement) = RaiseRoutedEvent FormletControl.ResetEvent    sender

    override this.MeasureOverride(sz : Size) =
        if not isInitialized then
            isInitialized <- true
            this.OnStartUp ()

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

    member this.Rebuild () = FormletControl.RaiseRebuild this

[<AbstractClass>]
type UnaryControl() =
    inherit FormletControl()

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
type BinaryControl() =
    inherit FormletControl()

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
module internal FormletControls =

    type JoinControl<'T>() =
        inherit BinaryControl()

        member val Formlet  : 'T option     = None                  with get, set
        member val Collect  : Collect<'T>   = Fail_NeverBuiltUp ()  with get, set


    type InputTextControl(initialText : string) as this =
        inherit TextBox()

        do
            this.Text   <- initialText
            this.Margin <- DefaultMargin

        override this.OnLostFocus(e) = 
            base.OnLostFocus(e)
            FormletControl.RaiseRebuild this

    type InputOptionControl<'T>() as this =
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
            FormletControl.RaiseRebuild this

    type GroupControl() as this =
        inherit UnaryControl()

        let outer, label, inner = CreateGroup "Group"

        do
            this.Value <- outer

        member this.Inner
            with get ()                         = inner.Child :?> FrameworkElement
            and  set (value : FrameworkElement) = inner.Child <- value

        member this.Text
            with get ()                         = label.Text
            and  set (value)                    = label.Text <- value



    type LabelControl(labelWidth : double) as this =
        inherit BinaryControl()

        let label = CreateLabel "Label" labelWidth

        do
            this.Orientation    <- LeftToRight
            this.StretchBehavior<- RightStretches
            this.Left           <- label

        member this.Text
            with get ()                         = label.Text
            and  set (value)                    = label.Text <- value

    type ValidationErrorLogControl() as this =
        inherit BinaryControl()

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

    type ValidationErrorBorderControl()=
        inherit UnaryControl()

        let mutable failures : Failure list = []

        static member pen = CreatePen Brushes.Red 1.0

        member this.Failures 
            with get ()         =   failures
            and  set (value)    =   failures <- value
                                    this.InvalidateVisual ()

        override this.OnRender (dc : DrawingContext) = 
            if this.Failures.Length > 0 then
                let rs = this.RenderSize
                let rect = Rect (rs)
                dc.DrawRectangle (null, ValidationErrorBorderControl.pen, rect)
            



    type SubmitResetControl() as this =
        inherit BinaryControl()

        let submit      = lazy CreateButton "_Submit" this.CanSubmit this.Submit
        let reset       = lazy CreateButton "_Reset" this.CanReset this.Reset
        let stackPanel  = lazy CreateStackPanel Orientation.Horizontal

        let mutable submitAllowed = false

        override this.OnStartUp () =
            ignore <| stackPanel.Value.Children.Add(submit.Value)
            ignore <| stackPanel.Value.Children.Add(reset.Value)
            this.Left <- stackPanel.Value

        member this.SubmitAllowed   
            with get()          =   submitAllowed
            and  set(value)     =   submitAllowed <- value
                                    CommandManager.InvalidateRequerySuggested()

        member this.Submit ()   = FormletControl.RaiseSubmit this
        member this.CanSubmit ()= this.SubmitAllowed

        member this.Reset ()    = FormletControl.RaiseReset this
        member this.CanReset () = true

