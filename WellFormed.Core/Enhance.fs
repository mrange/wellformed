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

open System.Text.RegularExpressions

open System.Windows
open System.Windows.Controls

module Enhance = 
    
    let WithLegend (t : string) (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui : FrameworkElement) =   let legend = CreateElement ui (fun () -> new LegendElement())
                                                legend.Text <- t
                                                legend.Inner <- f.Rebuild(legend.Inner)
                                                legend :> FrameworkElement
        let collect (ui : FrameworkElement) =   AppendFailureContext t <| CollectFromElement ui (fun (ui' : LegendElement) -> f.Collect(ui'.Inner))

        Formlet.New rebuild collect

    let WithStyle (style : Style) (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui : FrameworkElement) =   let ui' = f.Rebuild(ui)
                                                ui'.Style <- style
                                                ui'
        let collect (ui : FrameworkElement) =   f.Collect(ui)

        Formlet.New rebuild collect

    let WithWidth (width : double) (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui : FrameworkElement) =   let ui' = f.Rebuild(ui)
                                                ui'.Width <- width
                                                ui'
        let collect (ui : FrameworkElement) =   f.Collect(ui)

        Formlet.New rebuild collect

    let WithHeight (height : double) (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui : FrameworkElement) =   let ui' = f.Rebuild(ui)
                                                ui'.Height <- height
                                                ui'
        let collect (ui : FrameworkElement) =   f.Collect(ui)

        Formlet.New rebuild collect

    let WithLabel (t : string) (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui :FrameworkElement) =    let label = CreateElement ui (fun () -> new LabelElement(100.0)) 
                                                label.Text <- t
                                                label.Right <- f.Rebuild(label.Right)
                                                label :> FrameworkElement
        let collect (ui :FrameworkElement) =    AppendFailureContext t <| CollectFromElement ui (fun (ui' : LabelElement) -> f.Collect(ui'.Right))

        Formlet.New rebuild collect

    let WithErrorSummary (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui :FrameworkElement) =    let summary = CreateElement ui (fun () -> new ErrorSummaryElement()) 
                                                summary.Right <- f.Rebuild(summary.Right)
                                                summary :> FrameworkElement
        let collect (ui :FrameworkElement) =    CollectFromElement ui (fun (ui' : ErrorSummaryElement) ->   let collect = f.Collect(ui'.Right)
                                                                                                            ui'.Failures <- collect.Failures
                                                                                                            collect
                                                                                                            )

        Formlet.New rebuild collect

    let WithErrorVisual (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui :FrameworkElement) =    let visual = CreateElement ui (fun () -> new ErrorVisualElement()) 
                                                visual.Value <- f.Rebuild(visual.Value)
                                                visual :> FrameworkElement
        let collect (ui :FrameworkElement) =    CollectFromElement ui (fun (ui' : ErrorVisualElement)   ->  let collect = f.Collect(ui'.Value)
                                                                                                            ui'.Failures <- collect.Failures
                                                                                                            collect
                                                                                                            )

        Formlet.New rebuild collect

 
    let WithValidation (validator : 'T -> string option) (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui :FrameworkElement) =    f.Rebuild(ui)
        let collect (ui :FrameworkElement) =    let result = f.Collect(ui)
                                                match result.Value with
                                                |   Some v      ->  let fs = validator v
                                                                    match fs with
                                                                    |   Some fs'    ->  { 
                                                                                            Value = Some v  
                                                                                            Failures = {Context = []; Message = fs';}::result.Failures
                                                                                        }
                                                                    |   _           -> result
                                                |   _           ->  result
                                                

        Formlet.New rebuild collect

 
    let WithValidation_NonEmpty (f : Formlet<string>) : Formlet<string> = 
        WithValidation (fun s -> if String.IsNullOrWhiteSpace s then Some "Value must not be empty" else None) f

    let WithValidation_Regex (r : Regex) (msg : string) (f : Formlet<string>) : Formlet<string> = 
        WithValidation (fun s -> if r.IsMatch s then None else Some msg) f

    let WithSubmitAndReset (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui :FrameworkElement) =    let submitReset = CreateElement ui (fun () -> new SubmitResetElement()) 
                                                submitReset.Right <- f.Rebuild(submitReset.Right)
                                                submitReset :> FrameworkElement
        let collect (ui :FrameworkElement) =    CollectFromElement ui (fun (ui' : SubmitResetElement) -> 
                                                    let collect = f.Collect(ui'.Right)
                                                    ui'.SubmitAllowed <-
                                                        match collect.Value, collect.Failures.Length with   
                                                        |   Some _, 0   -> true
                                                        |   _           -> false
                                                    collect
                                                    )
                                                
                                                

        Formlet.New rebuild collect
 
