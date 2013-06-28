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
    
    let WithGroup (t : string) (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui : FrameworkElement) =   let group = CreateElement ui (fun () -> new GroupControl(t))
                                                group.Text <- t
                                                group.Inner <- f.Rebuild(group.Inner)
                                                group :> FrameworkElement
        let collect (ui : FrameworkElement) =   CollectFromElement ui (fun (ui' : GroupControl) -> f.Collect(ui'.Inner))

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
        let rebuild (ui :FrameworkElement) =    let label = CreateElement ui (fun () -> new LabelControl(t, 100.0)) 
                                                label.Text <- t
                                                label.Right <- f.Rebuild(label.Right)
                                                label :> FrameworkElement
        let collect (ui :FrameworkElement) =    CollectFromElement ui (fun (ui' : LabelControl) -> f.Collect(ui'.Right))

        Formlet.New rebuild collect

 
    let WithValidation (validator : 'T -> string option) (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui :FrameworkElement) =    f.Rebuild(ui)
        let collect (ui :FrameworkElement) =    let result = CollectFromElement ui (fun (ui' : LabelControl) -> f.Collect(ui'.Right))
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

 
