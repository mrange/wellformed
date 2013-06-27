namespace WellFormed.Core

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Threading

type Disposable =
    {
        Dispose : unit -> unit
    }
    interface IDisposable with

        member this.Dispose () =
            this.Dispose ()

    static member New d =
        {Dispose = d} :> IDisposable


type LayoutOrientation = 
    |   TopToBottom
    |   LeftToRight


type Result<'T> =
    | Success of 'T
    | Failure of string list

[<AutoOpen>]
module Utils =

    let IsEqual (l : obj) (r : obj) =   match l <> null,r <> null with
                                            |   true    , true  ->  l.Equals(r)
                                            |   true    , false ->  false
                                            |   false   , true  ->  false
                                            |   false   , false ->  true

    let Fail (f : string) = Failure [f] 
             
    let Enumerator (e : array<'T>) = e.GetEnumerator()

    let MakeFirstRectUsingOrientation orientation (sz : Size) (f : FrameworkElement) = 
        match orientation with 
        |   TopToBottom -> Rect (0.0, 0.0, sz.Width, f.DesiredSize.Height)
        |   LeftToRight -> Rect (0.0, 0.0, f.DesiredSize.Width, sz.Height)

    let MakeSecondRectUsingOrientation orientation (sz : Size) (f : FrameworkElement) (s : FrameworkElement) = 
        match orientation with 
        |   TopToBottom -> Rect (0.0, f.DesiredSize.Height, sz.Width, s.DesiredSize.Height)
        |   LeftToRight -> Rect (f.DesiredSize.Width, 0.0, s.DesiredSize.Width, sz.Height)

    let AdjustUsingOrientation orientation (sz : Size) = 
        match orientation with
        |   TopToBottom -> Size (sz.Width, Double.PositiveInfinity)
        |   LeftToRight -> Size (Double.PositiveInfinity, sz.Height)

    let CombineVertically (sz : Size) (l : Size) (r : Size) = 
        Size (Math.Min(Math.Max(l.Width, r.Width), sz.Width), Math.Min(l.Height + r.Height, sz.Height))

    let CombineHorizontally (sz : Size) (l : Size) (r : Size) = 
        Size (Math.Min(l.Width + r.Width, sz.Width), Math.Min(Math.Max (l.Height, r.Height), sz.Height))

    let CombineUsingOrientation (o : LayoutOrientation) (sz : Size) (l : Size) (r : Size) =
        match o with
        |   TopToBottom -> CombineVertically    sz l r
        |   LeftToRight -> CombineHorizontally  sz l r
                       
    let CreateElement (ui : FrameworkElement) (creator : unit -> #FrameworkElement) : #FrameworkElement = 
        match ui with
        | :? #FrameworkElement as ui' -> ui'
        | _                 -> creator()

    let ApplyToElement (ui : FrameworkElement) (apply : #FrameworkElement -> Result<'T>) : Result<'T> = 
        match ui with
        | :? #FrameworkElement as ui' -> apply ui'
        | _                 -> Fail "Couldn't apply to element as it wasn't of a compatible type"

    let DoNothing() = ()

    let NothingToDispose() = Disposable.New DoNothing

    let RoutedEventAsDelegate (action : obj -> RoutedEventArgs -> unit) = 
        let a = RoutedEventHandler action
        let d : Delegate = upcast a
        d

    let ActionAsDelegate (action : unit -> unit) = 
        let a = Action action 
        let d : Delegate = upcast a
        d

    let Dispatch (dispatcher : Dispatcher) (action : unit -> unit) = 
        let d = ActionAsDelegate action
        ignore <| dispatcher.BeginInvoke (DispatcherPriority.ApplicationIdle, d)


    let DefaultBackgroundBrush  = Brushes.White

    let DefaultMargin           = new Thickness(4.0)
    let DefaultBorderMargin     = new Thickness(4.0,12.0,4.0,4.0)
    let DefaultBorderPadding    = new Thickness(0.0,24.0,0.0,0.0)
    let DefaultBorderThickness  = new Thickness(2.0)
    let DefaultBorderBrush      = Brushes.LightBlue


    let NewColumn w = 
        let c = new ColumnDefinition()
        c.Width <- new GridLength(w)
        c                                    
    let NewStarColumn () = 
        let c = new ColumnDefinition()
        c.Width <- new GridLength(1.0, GridUnitType.Star)
        c                                    

    let NewAutoRow () = 
        let r = new RowDefinition()
        r.Height <- GridLength.Auto
        r                                    

    let CreateEmpty () = null :> UIElement

    let CreateTextBlock t = 
        let textBlock = new TextBlock()
        textBlock.Text <- t
        textBlock.Margin <- DefaultMargin
        textBlock

    let CreateTextBox t = 
        let textBox = new TextBox()
        textBox.Text <- t
        textBox.Margin <- DefaultMargin
        textBox

    let CreateLabel t = 
        let textBlock = CreateTextBlock t
        textBlock.Width <- 100.0
        textBlock

    let CreateGroup t : FrameworkElement*TextBox*Decorator = 
        let label = CreateTextBox t
        label.IsReadOnly <- true
        label.VerticalAlignment <- VerticalAlignment.Top
        label.HorizontalAlignment <- HorizontalAlignment.Left
        label.RenderTransform <- new TranslateTransform (8.0, 0.0)
        let border = new Border ()
        let outer = new Grid ()
        border.Margin <- DefaultBorderMargin
        border.Padding <- DefaultBorderPadding
        border.BorderThickness <- DefaultBorderThickness
        border.BorderBrush <- DefaultBorderBrush 
        ignore <| outer.Children.Add(border)
        ignore <| outer.Children.Add(label)
        upcast outer, label, upcast border

