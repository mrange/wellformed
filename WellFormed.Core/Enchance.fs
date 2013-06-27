namespace WellFormed.Core

open System

open System.Windows
open System.Windows.Controls

module Enchance = 
    
(*
    let WithGroup (t : string) (f : Formlet<'T>) : Formlet<'T> = 
        let build (lt : ILogicalTreeBuilder) =

            let outer, inner = CreateGroup t

            lt.Add (outer)

            let ilt = lt.NewGroupFromPanel inner

            let innerForm = f.Build(ilt)

            let dispose() = inner.Children.Clear()
                            innerForm.Dispose()

            {
                Dispose     = dispose
                State       = innerForm.State
            } :> IForm<'T>
        Formlet.New build

    let WithLabel (t : string) (f : Formlet<'T>) : Formlet<'T> = 
        let build (lt : ILogicalTreeBuilder) =
            let label = CreateTextBlock t

            lt.Add (label)
            let innerForm = f.Build(lt)

            let dispose() = innerForm.Dispose()

            {
                Dispose     = dispose
                State       = innerForm.State
            } :> IForm<'T>
        Formlet.New build
*)

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
