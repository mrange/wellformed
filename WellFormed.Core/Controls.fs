namespace WellFormed.Core

open System.Windows.Controls

module Controls =

    
    let Input t = 
        let build (lt : ILogicalTreeBuilder) (context : obj) =
            let control = CreateTextBox t
            lt.Add (control)

            let state = ref t

            let observable, observer = Observable.Source (fun o -> o.OnNext (Success !state))

            control.LostFocus.Add (fun er -> 
                if !state <> control.Text then
                    state := control.Text
                    observer.OnNext (Success !state))

            let dispose()   = observer.OnCompleted()

            {
                Dispose     = dispose
                State       = observable
            } :> IForm<string>
        Formlet.New build