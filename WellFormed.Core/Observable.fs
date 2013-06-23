namespace WellFormed.Core

open System

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

    let Return (x : 'T) = Observable<'T>.New <| fun observer ->
                            observer.OnNext(x)
                            observer.OnCompleted()
                            NothingToDispose()    

    let Never<'T>       = Observable<'T>.New <| fun observer ->
                            NothingToDispose()    

    let Map (m : 'T -> 'U) (o : IObservable<'T>) : IObservable<'U> = 
        Observable.New <| fun observer ->
            o.Subscribe(fun v -> observer.OnNext(m v))

                                
    let Where (f : 'T -> bool) (o : IObservable<'T>) : IObservable<'T> = 
        Observable.New <| fun observer ->
            o.Subscribe(fun v -> 
                if f v then observer.OnNext(v))

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
