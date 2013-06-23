namespace WellFormed.Core

open System

open System.Windows
open System.Windows.Controls

module Enchance = 

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
                Collect     = innerForm.Collect
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
                Collect     = innerForm.Collect
            } :> IForm<'T>
        Formlet.New build
