namespace FSharpVSPowerTools.ProjectSystem

open FSharpVSPowerTools
open Microsoft.VisualStudio.Shell.Interop
open Microsoft.VisualStudio
open System
open System.ComponentModel.Composition
open Microsoft.VisualStudio.Shell

/// Listen to events related to solution builds
[<Export>]
type SolutionBuildEventListener
    [<ImportingConstructor>] 
    ([<Import(typeof<SVsServiceProvider>)>] serviceProvider: IServiceProvider) as self =
    let solutionBuildManager = serviceProvider.GetService<IVsSolutionBuildManager2, SVsSolutionBuildManager>()
    let mutable updateSolutionEventsCookie = 0u
    do solutionBuildManager.AdviseUpdateSolutionEvents(self, &updateSolutionEventsCookie) |> ignore
    
    let activeConfigChanged = Event<_>()
    let solutionBuildDone = Event<_>()

    [<CLIEvent>]
    member __.ActiveConfigChanged = activeConfigChanged.Publish

    [<CLIEvent>]
    member __.SolutionBuildDone = solutionBuildDone.Publish

    interface IVsUpdateSolutionEvents with
        member __.OnActiveProjectCfgChange(pIVsHierarchy) = 
             match getProject pIVsHierarchy with
             | Some project ->
                 activeConfigChanged.Trigger(project)
             | None ->
                 ()
             VSConstants.S_OK
        
        member __.UpdateSolution_Begin(_pfCancelUpdate) = 
            VSConstants.E_NOTIMPL
        
        member __.UpdateSolution_Cancel() = 
            VSConstants.E_NOTIMPL
        
        member __.UpdateSolution_Done(fSucceeded, _fModified, _fCancelCommand) = 
            solutionBuildDone.Trigger(fSucceeded)
            VSConstants.S_OK
        
        member __.UpdateSolution_StartUpdate(_pfCancelUpdate) = 
            VSConstants.E_NOTIMPL
    
    interface IVsUpdateSolutionEvents2 with
        member __.OnActiveProjectCfgChange(_pIVsHierarchy) = 
            VSConstants.E_NOTIMPL

        member __.UpdateProjectCfg_Begin(_pHierProj, _pCfgProj, _pCfgSln, _dwAction, _pfCancel) = 
            VSConstants.E_NOTIMPL

        member __.UpdateProjectCfg_Done(_pHierProj, _pCfgProj, _pCfgSln, _dwAction, _fSuccess, _fCancel) = 
            VSConstants.E_NOTIMPL

        member __.UpdateSolution_Begin(_pfCancelUpdate) = 
            VSConstants.E_NOTIMPL

        member __.UpdateSolution_Cancel() = 
            VSConstants.E_NOTIMPL
        
        member __.UpdateSolution_Done(_fSucceeded, _fModified, _fCancelCommand) = 
            VSConstants.E_NOTIMPL
        
        member __.UpdateSolution_StartUpdate(_pfCancelUpdate) = 
            VSConstants.E_NOTIMPL
    
    interface IDisposable with
        member __.Dispose() = 
            solutionBuildManager.UnadviseUpdateSolutionEvents(updateSolutionEventsCookie) |> ignore
