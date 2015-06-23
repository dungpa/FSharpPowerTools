namespace FSharpVSPowerTools.FSharpInteractive

open FSharpVSPowerTools
open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Classification
open System.ComponentModel.Composition
open Microsoft.VisualStudio.Utilities

[<ContentType("text")>]
[<Export(typeof<IClassifierProvider>)>]
type FsiClassifierProvider() =
    static let mutable fsiClassifier = None
    interface IClassifierProvider with
        member __.GetClassifier(_textBuffer: ITextBuffer): IClassifier = 
            fsiClassifier
            |> Option.getOrTry (fun _ -> FsiClassifier() :> _)