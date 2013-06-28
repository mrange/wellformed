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
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Threading

type LayoutOrientation = 
    |   TopToBottom
    |   LeftToRight

type StretchBehavior = 
    |   NoStretch
    |   RightStretches


type Failure =
    {
        Context : string list
        Message : string
    }

type Collect<'T> =
    {
        Value       : 'T option
        Failures    : Failure list
    }

[<AutoOpen>]
module Utils =

    let rec LastOrDefault defaultTo ls = 
        match ls with
        |   []          -> defaultTo
        |   [v]         -> v
        |   v::vs       -> LastOrDefault defaultTo vs
    let JoinFailures (l : Collect<'U>) (r : Collect<'T>) = 
        {
            Value       = r.Value
            Failures    = l.Failures @ r.Failures
        }

    let AppendFailureContext (ctx : string) (collect : Collect<'T>) = 
        {
            Value       = collect.Value
            Failures    = collect.Failures 
                            |> List.map (fun f -> {Context = ctx::f.Context; Message = f.Message})
        }

    let Success v   = 
        {
            Value       = Some v
            Failures    = []
        }

    let Fail (msg : string)   = 
        {
            Value       = None
            Failures    = [{Context = []; Message = msg;}]
        }

    let Fail_NeverBuiltUp ()= Fail "WellFormed.ProgrammmingError: Never built up"
             
    let Enumerator (e : array<'T>) = e.GetEnumerator()

    let EmptySize = new Size()
    let EmptyRect = new Rect()

    let TranslateUsingOrientation orientation (fill : bool) (sz : Size) (l : Rect) (r : Size) = 
        match fill,orientation with 
        |   false, TopToBottom  -> Rect (0.0        , l.Bottom  , sz.Width                      , r.Height                      )
        |   false, LeftToRight  -> Rect (l.Right    , 0.0       , r.Width                       , sz.Height                     )
        |   true , TopToBottom  -> Rect (0.0        , l.Bottom  , sz.Width                      , max (sz.Height - l.Bottom) 0.0)
        |   true , LeftToRight  -> Rect (l.Right    , 0.0       , max (sz.Width - l.Right) 0.0  , sz.Height                     )

    let ExceptVertically (l : Size) (r : Size) = 
        Size (max l.Width r.Width, max (l.Height - r.Height) 0.0)

    let ExceptHorizontally (l : Size) (r : Size) = 
        Size (max (l.Width - r.Width) 0.0, max l.Height r.Height)

    let ExceptUsingOrientation (o : LayoutOrientation) (l : Size) (r : Size) =
        match o with
        |   TopToBottom -> ExceptVertically    l r
        |   LeftToRight -> ExceptHorizontally  l r

    let Intersect (l : Size) (r : Size) = 
        Size (min l.Width r.Width, min l.Height r.Height)

    let UnionVertically (l : Size) (r : Size) = 
        Size (max l.Width r.Width, l.Height + r.Height)

    let UnionHorizontally (l : Size) (r : Size) = 
        Size (l.Width + r.Width, max l.Height r.Height)

    let UnionUsingOrientation (o : LayoutOrientation) (l : Size) (r : Size) =
        match o with
        |   TopToBottom -> UnionVertically    l r
        |   LeftToRight -> UnionHorizontally  l r
                       
    let CreateElement (ui : FrameworkElement) (creator : unit -> #FrameworkElement) : #FrameworkElement = 
        match ui with
        | :? #FrameworkElement as ui' -> ui'
        | _                 -> creator()

    let ApplyToElement defaultTo (ui : FrameworkElement) (apply : #FrameworkElement -> 'T) : 'T = 
        match ui with
        | :? #FrameworkElement as ui'   -> apply ui'
        | _                             -> defaultTo

    let CollectFromElement (ui : FrameworkElement) (apply : #FrameworkElement -> Collect<'T>) : Collect<'T> = 
        ApplyToElement (Fail_NeverBuiltUp ()) ui apply 

    let DoNothing() = ()

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

    let DefaultMargin           = Thickness(4.0)
    let DefaultBorderMargin     = Thickness(4.0,12.0,4.0,4.0)
    let DefaultBorderPadding    = Thickness(0.0,24.0,0.0,0.0)
    let DefaultBorderThickness  = Thickness(2.0)
    let DefaultBorderBrush      = Brushes.LightBlue

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

    let CreateLabel t w = 
        let textBlock = CreateTextBlock t
        textBlock.Width <- w
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

