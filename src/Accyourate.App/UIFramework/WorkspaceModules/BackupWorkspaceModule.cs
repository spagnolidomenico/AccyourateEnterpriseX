using Avalonia.Controls;using Accyourate.App.Platform.Backup;
namespace Accyourate.App.UIFramework.WorkspaceModules;
public sealed class BackupWorkspaceModule:IWorkspaceModule{public string Id=>"backup-center";public string Title=>"Backup Center";public string Icon=>"💾";public bool CanClose=>true;public bool IsPinned=>false;public Control CreateView()=>new BackupCenterView();}
