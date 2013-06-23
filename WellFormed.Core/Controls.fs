namespace WellFormed.Core

open System.Windows.Controls

module Controls =

    
    let Input t = 
        let build (lt : ILogicalTreeBuilder) =
            let control = CreateTextBox t
            lt.Add (control)

            let collect() = Success control.Text

            {
                Dispose     = DoNothing
                Collect     = collect
            } :> IForm<string>
        Formlet.New build