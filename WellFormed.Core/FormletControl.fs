namespace WellFormed.Core

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Threading
open System.Windows.Media

type FormletControl<'T>(action : 'T -> unit, formlet : Formlet<'T>) as this=
    inherit ContentControl()

    [<DefaultValue>] val mutable state      : IForm<'T>*LogicalTreeRoot*LogicalTreeBuilder
    [<DefaultValue>] val mutable observer   : IDisposable

    let (outer, inner) = CreateGroup "Form"

    do
        this.LayoutTransform <- new ScaleTransform (1.5, 1.5)

    override this.OnApplyTemplate() =   Dispatch this.Dispatcher this.BuildForm

    member this.UpdateVisualTree() = 
        let (_,_,lt) = this.state
        lt.Update()
        ()

    member this.UpdateState (result : Result<'T>) = 
        match result with
            |   Success v   -> ()
            |   Failure fs  -> ()

    member this.BuildForm() = 

        let (outer, inner) = CreateGroup "Form"

        let lr = new LogicalTreeRoot (this.Dispatcher, this.UpdateVisualTree)
        let lt = new LogicalTreeBuilder(lr, inner)

        let form = formlet.Build(upcast lt)

        this.state <- form, lr, lt

        this.observer <- form.State |> Observable.Sink this.UpdateState

        this.Content <- outer

        ()


module FormletControl =
    let New<'T> (action : 'T -> unit) (formlet : Formlet<'T>) = new FormletControl<'T>(action, formlet)