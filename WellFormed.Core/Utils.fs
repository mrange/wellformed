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


[<AutoOpen>]
module Utils =

    let IsEqual (l : obj) (r : obj) =   match l <> null,r <> null with
                                            |   true    , true  ->  l.Equals(r)
                                            |   true    , false ->  false
                                            |   false   , true  ->  false
                                            |   false   , false ->  true

    let CreateElement (ui : FrameworkElement) (creator : unit -> #FrameworkElement) : #FrameworkElement = 
        match ui with
            | :? #FrameworkElement as ui' -> ui'
            | _                 -> creator()

    let ApplyToElement (ui : FrameworkElement) fallback (apply : #FrameworkElement -> 'T) : 'T = 
        match ui with
            | :? #FrameworkElement as ui' -> apply ui'
            | _                 -> fallback
    let DoNothing() = ()

    let NothingToDispose() = Disposable.New DoNothing

    let Dispatch (dispatcher : Dispatcher) (action : unit -> unit) = 
        let a = Action action 
        let d : Delegate = upcast a
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

    let CreateGroup t : FrameworkElement*Panel = 
        let label = CreateTextBox t
        label.IsReadOnly <- true
        label.VerticalAlignment <- VerticalAlignment.Top
        label.HorizontalAlignment <- HorizontalAlignment.Left
        label.RenderTransform <- new TranslateTransform (8.0, 0.0)
        let border = new Border ()
        let outer = new Grid ()
        let inner = new StackPanel ()
        border.Margin <- DefaultBorderMargin
        border.Padding <- DefaultBorderPadding
        border.Child <- inner
        border.BorderThickness <- DefaultBorderThickness
        border.BorderBrush <- DefaultBorderBrush 
        ignore <| outer.Children.Add(border)
        ignore <| outer.Children.Add(label)
        upcast outer, upcast inner

