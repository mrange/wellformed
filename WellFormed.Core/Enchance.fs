namespace WellFormed.Core

open System

open System.Windows
open System.Windows.Controls

module Enchance = 

    let WithGroup (t : string) (f : Formlet<'T>) : Formlet<'T> = 
        let build() =
            let outer, inner = CreateGroup t

            let innerForm = f.Build()

            let body() = Group (outer, inner, innerForm.Body())
            let dispose() = inner.Children.Clear()
                            innerForm.Dispose()

            {
                Body        = body
                Dispose     = dispose
                Collect     = innerForm.Collect
            } :> IForm<'T>
        Formlet.New build

    let WithLabel (t : string) (f : Formlet<'T>) : Formlet<'T> = 
        let build() =
            let label = CreateLabel t

            let innerForm = f.Build()

            let body() = Body.Label (label, innerForm.Body())
            let dispose() = innerForm.Dispose()

            {
                Body        = body
                Dispose     = dispose
                Collect     = innerForm.Collect
            } :> IForm<'T>
        Formlet.New build
