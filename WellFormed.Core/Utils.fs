namespace WellFormed.Core

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media

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

    let DoNothing() = ()

    let NothingToDispose() = Disposable.New DoNothing

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

    let CreateLabel t = CreateTextBlock t

    let CreateGroup t : FrameworkElement*Grid = 
        let label = CreateLabel t
        label.VerticalAlignment <- VerticalAlignment.Top
        label.HorizontalAlignment <- HorizontalAlignment.Left
        label.Background <- DefaultBackgroundBrush
        label.RenderTransform <- new TranslateTransform (8.0, 0.0)
        let border = new Border ()
        let outerGrid = new Grid ()
        let innerGrid = new Grid ()
        innerGrid.ColumnDefinitions.Add(NewColumn 70.0)
        innerGrid.ColumnDefinitions.Add(NewStarColumn ())
        innerGrid.ColumnDefinitions.Add(NewColumn 32.0)
        border.Margin <- DefaultBorderMargin
        border.Padding <- DefaultBorderPadding
        border.Child <- innerGrid
        border.BorderThickness <- DefaultBorderThickness
        border.BorderBrush <- DefaultBorderBrush 
        ignore <| outerGrid.Children.Add(border)
        ignore <| outerGrid.Children.Add(label)
        upcast outerGrid, innerGrid

