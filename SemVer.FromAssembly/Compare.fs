namespace SemVer.FromAssembly
open System
(*
data Changes k v =
  Changes
    { added :: Map.Map k v
    , changed :: Map.Map k (v,v)
    , removed :: Map.Map k v
    }
*)
type Changes<'Key,'Value when 'Key : comparison > =
    {
        added: Map<'Key,'Value>
        changed: Map<'Key,'Value*'Value>
        removed: Map<'Key,'Value>
    }
(*
data ModuleChanges =
  ModuleChanges
    { adtChanges :: Changes String ([String], Map.Map String [Type.Type])
    , aliasChanges :: Changes String ([String], Type.Type)
    , valueChanges :: Changes String Type.Type
    }
*)
type TypeChanges=
    {
        adtChanges: Changes<string,string list*Map<string,Type list>>
        aliasChanges: Changes<string,string list*Type>
        valueChanges: Changes<string,Type>
    }
(*
data PackageChanges =
  PackageChanges
    { modulesAdded :: [String]
    , modulesChanged :: Map.Map String ModuleChanges
    , modulesRemoved :: [String]
    }
*)
type PackageChanges=
    { 
        TypesAdded : string list
        TypesChanged: Map<string,TypeChanges>
        TypesRemoved: string list
    }

(*
data Module = Module
    { adts :: Map.Map String ([String], Map.Map String [Type.Type])
    , aliases :: Map.Map String ([String], Type.Type)
    , values :: Map.Map String Type.Type
    , version :: Docs.Version
    }
    *)