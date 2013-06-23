namespace WellFormed.Core

open System.Windows.Controls

module Controls =

    
    let Input t = 
        let build () =
            let control = CreateTextBox t

            let body() = Element control     
            let collect() = Success control.Text

            {
                Body        = body
                Dispose     = DoNothing
                Collect     = collect
            } :> IForm<string>
        Formlet.New build