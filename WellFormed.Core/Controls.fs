namespace WellFormed.Core

open System.Windows.Controls

module Controls =

    
    let Input t = 
        let build () =
            let control = CreateTextBox t

            let buildTree (t : ILogicalTreeBuilder) = t.Add (control)
            let collect() = Success control.Text

            {
                BuildTree   = buildTree
                Dispose     = DoNothing
                Collect     = collect
            } :> IForm<string>
        Formlet.New build