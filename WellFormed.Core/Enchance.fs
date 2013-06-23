namespace WellFormed.Core

open System

open System.Windows
open System.Windows.Controls

module Enchance = 

    let WithGroup (t : string) (f : Formlet<'T>) : Formlet<'T> = 
        let build() =
            let outer, inner = CreateGroup t

            let innerForm = f.Build()

            let buildTree (t : ILogicalTreeBuilder) = 
                            t.Add(outer)
                            let g = t.NewGroupFromPanel inner
                            innerForm.BuildTree g
            let dispose() = inner.Children.Clear()
                            innerForm.Dispose()

            {
                BuildTree   = buildTree
                Dispose     = dispose
                Collect     = innerForm.Collect
            } :> IForm<'T>
        Formlet.New build

    let WithLabel (t : string) (f : Formlet<'T>) : Formlet<'T> = 
        let build() =
            let label = CreateTextBlock t

            let innerForm = f.Build()

            let buildTree (t : ILogicalTreeBuilder) = 
                            t.Add(label)
                            innerForm.BuildTree t                            
            let dispose() = innerForm.Dispose()

            {
                BuildTree   = buildTree
                Dispose     = dispose
                Collect     = innerForm.Collect
            } :> IForm<'T>
        Formlet.New build
