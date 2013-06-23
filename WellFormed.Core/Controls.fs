namespace WellFormed.Core

open System.Windows.Controls

module Controls =

    
    let Input t = 
        let build (lt : ILogicalTreeBuilder) =
            let control = CreateTextBox t
            lt.Add (control)

            let observable, observer = Observable.Source()

            control.Loaded.Add (fun er -> observer.OnNext(Success t))
            control.TextChanged.Add (fun er -> observer.OnNext(Success t))

            let dispose()   = observer.OnCompleted()

            {
                Dispose     = dispose
                State       = observable
            } :> IForm<string>
        Formlet.New build