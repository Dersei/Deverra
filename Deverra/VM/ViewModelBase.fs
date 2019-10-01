namespace VM

open System.ComponentModel
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open FSharp.Core
open System.Windows.Input

type ViewModelBase() =
    let propertyChanged = new Event<_, _>()
    let toPropName(query : Expr) = 
        match query with
        | PropertyGet(a, b, list) ->
            b.Name
        | _ -> ""

    member internal __.createCommand action canExecute=
        let event1 = Event<_, _>()
        {
            new ICommand with
                member _.CanExecute(obj) = canExecute(obj)
                member _.Execute(obj) = action(obj)
                member _.add_CanExecuteChanged(handler) = event1.Publish.AddHandler(handler)
                member _.remove_CanExecuteChanged(handler) = event1.Publish.AddHandler(handler)
        }

    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member _.PropertyChanged = propertyChanged.Publish

    abstract member OnPropertyChanged: string -> unit
    default x.OnPropertyChanged(propertyName : string) =
        propertyChanged.Trigger(x, new PropertyChangedEventArgs(propertyName))

    member x.OnPropertyChanged(expr : Expr) =
        let propName = toPropName(expr)
        x.OnPropertyChanged(propName)
