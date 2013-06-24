namespace WellFormed.Core

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Threading
open System.Windows.Media

type FormletControl<'T>(action : 'T -> unit, formlet : Formlet<'T>) as this=
    inherit ContentControl()

    do
        this.LayoutTransform <- new ScaleTransform (1.5, 1.5)

    override this.OnApplyTemplate() =   Dispatch this.Dispatcher this.BuildForm

    member this.BuildForm() = 
        this.Content <- ApplyToElement (this.Content :?> FrameworkElement) (null :> FrameworkElement) (fun ui' -> formlet.Rebuild (ui'))
        ()


module FormletControl =
    let New<'T> (action : 'T -> unit) (formlet : Formlet<'T>) = new FormletControl<'T>(action, formlet)