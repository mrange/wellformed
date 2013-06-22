namespace WellFormed.Core

open System.Windows.Controls

module Controls =

    
    let Input x = 
        let build () =
            let control = new TextBox()
            control.Height <- 24.0
            control.Text <- x
            DockPanel.SetDock(control, Dock.Top)
            {
                Body        = fun () -> Element control     
                Dispose     = DoNothing
                Collect     = fun () -> Success control.Text
            } :> IForm<string>
        Formlet.New build