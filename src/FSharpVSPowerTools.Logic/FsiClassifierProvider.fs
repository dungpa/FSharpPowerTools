namespace FSharpVSPowerTools.FSharpInteractive

open System
open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Classification
open System.ComponentModel.Composition
open Microsoft.VisualStudio.Utilities
open FSharpVSPowerTools.ProjectSystem
open Microsoft.VisualStudio.Text.Editor
open FSharpVSPowerTools

[<ContentType("text")>]
[<Export(typeof<IClassifierProvider>)>]
[<TextViewRole(PredefinedTextViewRoles.Interactive)>]
type FsiClassifierProvider() =
    static let serviceType = typeof<FsiClassifierProvider>

    [<Import>]
    member val ClassificationRegistry: IClassificationTypeRegistryService = Unchecked.defaultof<_> with get, set

    [<Import>]
    member val FSharpVsLanguageService: VSLanguageService = Unchecked.defaultof<_> with get, set

    interface IClassifierProvider with
        member x.GetClassifier(textBuffer: ITextBuffer) = 
            // TODO: enhance recognition of F# Interactive buffer
            let currentText = textBuffer.CurrentSnapshot.GetText()
            if String.IsNullOrEmpty(currentText) || currentText.StartsWith("Microsoft (R) F# Interactive version ", StringComparison.Ordinal) then
                textBuffer.Properties.GetOrCreateSingletonProperty(serviceType,
                       fun () -> new FsiClassifier(textBuffer, x.ClassificationRegistry, x.FSharpVsLanguageService) :> _)
            else null