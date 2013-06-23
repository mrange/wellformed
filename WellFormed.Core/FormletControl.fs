namespace WellFormed.Core

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Threading
open System.Windows.Media

type FlatBody =
    |   Visual          of FrameworkElement
    |   LabeledVisual   of FrameworkElement*FrameworkElement

type FormletControl<'T>(action : 'T -> unit, formlet : Formlet<'T>) as this=
    inherit ContentControl()

    [<DefaultValue>] val mutable form : IForm<'T>

    let (outer, inner) = CreateGroup "Form"

    do
        this.LayoutTransform <- new ScaleTransform (1.5, 1.5)

    member this.Dispatch (action : Action) = 
        let d : Delegate = upcast action
        this.Dispatcher.BeginInvoke (DispatcherPriority.ApplicationIdle, d)

    override this.OnApplyTemplate() =   let x = this.Dispatch (fun () -> this.BuildForm())
                                        ()


    member this.BuildForm() = 

        let (outer, inner) = CreateGroup "Form"

        let lt = new LogicalTreeBuilder(inner)

        this.form <- formlet.Build(upcast lt)

        lt.Update()

        this.Content <- outer

        inner.BeginInit()

        ()


module FormletControl =
    let New<'T> (action : 'T -> unit) (formlet : Formlet<'T>) = new FormletControl<'T>(action, formlet)