﻿namespace WellFormed.Core

open System

open System.Windows
open System.Windows.Controls

module Enchance = 
    
    let WithGroup (t : string) (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui : FrameworkElement) =   let group = CreateElement ui (fun () -> new GroupControl(t))
                                                group.Text <- t
                                                group.Inner <- f.Rebuild(group.Inner)
                                                group :> FrameworkElement
        let collect (ui : FrameworkElement) =   CollectFromElement ui (fun (ui' : GroupControl) -> f.Collect(ui'.Inner))
        let failures(ui :FrameworkElement)  =   FailuresFromElement ui (fun (ui' : GroupControl) -> f.Failures(ui'.Inner))

        Formlet.New rebuild collect failures

    let WithWidth (width : double) (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui : FrameworkElement) =   let ui' = f.Rebuild(ui)
                                                ui'.Width <- width
                                                ui'
        let collect (ui : FrameworkElement) =   f.Collect(ui)
        let failures(ui :FrameworkElement)  =   f.Failures(ui)

        Formlet.New rebuild collect failures

    let WithHeight (height : double) (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui : FrameworkElement) =   let ui' = f.Rebuild(ui)
                                                ui'.Height <- height
                                                ui'
        let collect (ui : FrameworkElement) =   f.Collect(ui)
        let failures(ui :FrameworkElement)  =   f.Failures(ui)

        Formlet.New rebuild collect failures

    let WithLabel (t : string) (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui :FrameworkElement) =    let label = CreateElement ui (fun () -> new LabelControl(t, 100.0)) 
                                                label.Text <- t
                                                label.Right <- f.Rebuild(label.Right)
                                                label :> FrameworkElement
        let collect (ui :FrameworkElement) =    CollectFromElement ui (fun (ui' : LabelControl) -> f.Collect(ui'.Right))
        let failures(ui :FrameworkElement) =    FailuresFromElement ui (fun (ui' : LabelControl) -> f.Failures(ui'.Right))

        Formlet.New rebuild collect failures

 
