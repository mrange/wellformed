namespace WellFormed.Core

open System

open System.Threading

type Observer<'T> =
    {
        OnNext      : 'T -> unit
        OnCompleted : unit -> unit
        OnError     : Exception -> unit
    }

    interface IObserver<'T> with
        member this.OnNext t =
            this.OnNext t
        member this.OnCompleted() =
            this.OnCompleted()
        member this.OnError err =
            this.OnError(err)

    static member New onNext onCompleted onError = {OnNext = onNext; OnCompleted = onCompleted; OnError = onError} :> IObserver<'T>
    static member New2 onNext onCompleted = {OnNext = onNext; OnCompleted = onCompleted; OnError = fun exc -> ()} :> IObserver<'T>

type Observable<'T> =
    {
        OnSubscribe : IObserver<'T> -> IDisposable
    }

    interface IObservable<'T> with

        member this.Subscribe o =
            this.OnSubscribe o

    member this.Subscribe (onNext: 'T -> unit) (onComplete: unit -> unit) (onError : Exception -> unit) =
        Observer.New onNext onComplete onError
        |> this.OnSubscribe

    static member New onSubscribe = {OnSubscribe = onSubscribe} :> IObservable<'T>

module Observable =

    let id = ref 0L

    let NewId () = Interlocked.Increment(id)

    let Return (x : 'T) = Observable<'T>.New <| fun observer ->
                            observer.OnNext(x)
                            observer.OnCompleted()
                            NothingToDispose()    

    let Never<'T>       = Observable<'T>.New <| fun observer ->
                            NothingToDispose()    

    let Select (m : 'T -> 'U) (o : IObservable<'T>) : IObservable<'U> = 
        Observable.New <| fun observer ->
            o.Subscribe(fun v -> observer.OnNext(m v))

    let Flatten (io: IObservable<IObservable<'T>>) : IObservable<'T> =
        Observable.New (fun o ->
            let disp = ref ignore
            let d =
                io.Subscribe(fun (o1 : IObservable<'T>) ->
                    let d = o1.Subscribe o.OnNext
                    disp := fun () ->
                        disp.Value ()
                        d.Dispose ())
            Disposable.New (fun () ->
                disp.Value ()
                d.Dispose ()))

                                
    let Where (f : 'T -> bool) (o : IObservable<'T>) : IObservable<'T> = 
        Observable.New <| fun observer ->
            o.Subscribe(fun v -> 
                if f v then observer.OnNext(v))

    let Sink (f : 'T -> unit) (o : IObservable<'T>) = o.Subscribe f

    let Source (initial : IObserver<'T> -> unit)  : IObservable<'T>*IObserver<'T> = 
        let completed = ref false
        let observers = ref Map.empty
        let observable = Observable.New <| fun observer -> 
            let id = NewId()
            observers := Map.add id observer !observers
            initial observer
            Disposable.New <| fun () -> 
                observers := Map.remove id !observers

        let onNext v = if not !completed then Map.iter (fun _ (observer : IObserver<'T>) -> observer.OnNext v) !observers
        let onCompleted() = if not !completed then 
                                        completed := true
                                        Map.iter (fun _ (observer : IObserver<'T>) -> observer.OnCompleted()) !observers
        let onError e = if not !completed then Map.iter (fun _ (observer : IObserver<'T>) -> observer.OnError e) !observers
        let observer = Observer.New onNext onCompleted onError
        observable, observer

    let Merge (io1: IObservable<'T>) (io2: IObservable<'T>) : IObservable<'T> =
        Observable.New <| fun o ->
            let completed1 = ref false
            let completed2 = ref false
            let disp1 =
                Observer.New2 o.OnNext <| fun () ->
                    completed1 := true
                    if completed1.Value && completed2.Value then
                        o.OnCompleted ()
                |> io1.Subscribe
            let disp2 =
                Observer.New2 o.OnNext <| fun () ->
                    completed2 := true
                    if completed1.Value && completed2.Value then
                        o.OnCompleted ()
                |> io2.Subscribe
            Disposable.New (fun () -> disp1.Dispose(); disp2.Dispose())
