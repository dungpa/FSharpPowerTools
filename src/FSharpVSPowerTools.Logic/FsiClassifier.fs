namespace FSharpVSPowerTools.FSharpInteractive

open System
open System.Collections.Generic
open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Classification

type FsiClassifier() =
    interface IClassifier with
        [<CLIEvent>]
        member __.ClassificationChanged: IEvent<EventHandler<ClassificationChangedEventArgs>,ClassificationChangedEventArgs> = 
            failwith ""
        
        member __.GetClassificationSpans(_span: SnapshotSpan): IList<ClassificationSpan> = 
            failwith ""
