namespace WellFormed.Core

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Threading

type FormletControl<'T>(action : 'T -> unit, formlet : Formlet<'T>) =
    inherit ContentControl()

    [<DefaultValue>] val mutable form : IForm<'T>

    member this.Dispatch (action : Action) = 
        let d : Delegate = upcast action
        this.Dispatcher.BeginInvoke (DispatcherPriority.ApplicationIdle, d)

    override this.OnApplyTemplate() =   let x = this.Dispatch (fun () -> this.BuildForm())
                                        ()

    let rec FlattenBody (b : Body) : FrameworkElement list = 
        match b with
            |   Empty       -> []
            |   Element e   -> [e]
            |   Join (l,r)  ->  let l' = FlattenBody l
                                let r' = FlattenBody r
                                l' @ r'

    member this.BuildForm() = 
        this.form <- formlet.Build()
        let body = this.form.Body()
        let flattened = FlattenBody body
        let dockPanel = new DockPanel()
        dockPanel.LastChildFill <- false
        for fe in flattened do
            dockPanel.Children.Add(fe)
        this.Content <- dockPanel

    //static member New (action : 'T -> unit) (formlet : Formlet<'T>) = new FormletControl<'T>(action, formlet)
    


module FormletControl =
    let New<'T> (action : 'T -> unit) (formlet : Formlet<'T>) = new FormletControl<'T>(action, formlet)