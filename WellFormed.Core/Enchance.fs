namespace WellFormed.Core

open System

open System.Windows
open System.Windows.Controls

module Enchance = 
    
    let WithGroup (t : string) (f : Formlet<'T>) : Formlet<'T> = 
        let rebuild (ui : FrameworkElement) =   let group = CreateElement ui (fun () -> new GroupControl(t))
                                                group.Text <- t
                                                group.Inner <- f.Rebuild(group.Inner)
                                                group :> FrameworkElement
        let collect (ui : FrameworkElement) =   ApplyToElement ui (fun (ui' : GroupControl) -> f.Collect(ui'.Inner))
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
        Formlet.Do
            {
                do! Information t

                return! f
            }
