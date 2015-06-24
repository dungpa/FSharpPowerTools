namespace FSharpVSPowerTools.FSharpInteractive

open System
open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Classification
open FSharpVSPowerTools.ProjectSystem
open FSharpVSPowerTools
open Microsoft.FSharp.Compiler.SourceCodeServices

type FsiClassifier(buffer: ITextBuffer, 
                   classificationRegistry: IClassificationTypeRegistryService,
                   vsLanguageService: VSLanguageService) as self =

    let classificationChanged = Event<_,_>()
    let data = Atom None

    let getClassificationType typ = 
        classificationRegistry.GetClassificationType(typ)

    let updateClassifier() = 
        let snapshot = buffer.CurrentSnapshot
        let content = snapshot.GetText()
        // We use the property of F# Interactive Window: it is either empty (being clear) or having a specific header.
        // The goal is to create harmless classifiers if certain text buffers pass our initial check.
        if content.StartsWith("> ", StringComparison.Ordinal) || 
           content.StartsWith("\r\nMicrosoft (R) F# Interactive version ", StringComparison.Ordinal) then
            let classifications =
                content
                |> String.split StringSplitOptions.None [|"\r\n"; "\r"; "\n"|]
                |> Array.mapi (fun i line ->
                    if line.StartsWith("> ", StringComparison.Ordinal) then
                        let sourceCode = "  " + line.[2..]
                        vsLanguageService.TokenizeSource(sourceCode, [|sourceCode|], [||])
                        |> Seq.concat
                        |> Seq.choose (fun token ->
                            let line = snapshot.GetLineFromLineNumber(i)
                            let span = SnapshotSpan(line.Start.Add(token.LeftColumn), line.Start.Add(token.RightColumn+1))
                            match token.ColorClass with
                            | FSharpTokenColorKind.Keyword -> 
                                let clType = getClassificationType "Keyword"
                                Some (ClassificationSpan(span, clType))
                            | FSharpTokenColorKind.Comment ->
                                let clType = getClassificationType "Comment"
                                Some (ClassificationSpan(span, clType))
                            | FSharpTokenColorKind.Identifier
                            | FSharpTokenColorKind.UpperIdentifier ->
                                let clType = getClassificationType "Identifier"
                                Some (ClassificationSpan(span, clType))
                            | FSharpTokenColorKind.String ->
                                let clType = getClassificationType "String"
                                Some (ClassificationSpan(span, clType))
                            | FSharpTokenColorKind.InactiveCode ->
                                let clType =  getClassificationType "Excluded Code"
                                Some (ClassificationSpan(span, clType))
                            | FSharpTokenColorKind.PreprocessorKeyword ->
                                let clType = getClassificationType "Preprocessor Keyword"
                                Some (ClassificationSpan(span, clType))
                            | FSharpTokenColorKind.Number ->
                                let clType = getClassificationType "Number"
                                Some (ClassificationSpan(span, clType))
                            | FSharpTokenColorKind.Operator ->
                                let clType = getClassificationType "Operator"
                                Some (ClassificationSpan(span, clType))
                            | FSharpTokenColorKind.Text 
                            | _ -> None)
                        |> Seq.toArray
                    elif String.IsNullOrWhiteSpace(line) then
                        [||]
                    else
                        let snapshotLine = snapshot.GetLineFromLineNumber(i)
                        let span = SnapshotSpan(snapshotLine.Start, snapshotLine.End)
                        let clType = getClassificationType "Symbol Definition"
                        [| ClassificationSpan(span, clType) |])
                |> Array.concat
            data.Swap(fun _ -> Some classifications) |> ignore
            let span = SnapshotSpan(snapshot, 0, snapshot.Length)
            classificationChanged.Trigger(self, ClassificationChangedEventArgs(span))

    let docEventListener = 
        new DocumentEventListener ([ViewChange.bufferEvent buffer], 200us, updateClassifier)

    let getClassificationSpans _span = 
        match data.Value with
        | Some spans -> 
            spans
        | None ->
            updateClassifier()
            [||]

    interface IClassifier with
        [<CLIEvent>]
        member __.ClassificationChanged = 
            classificationChanged.Publish
        
        member __.GetClassificationSpans(span: SnapshotSpan) = 
            upcast (protectOrDefault (fun _ -> getClassificationSpans span) [||])

    interface IDisposable with
        member __.Dispose() = 
            (docEventListener :> IDisposable).Dispose()
