namespace SemVer.FromAssembly
open System

type Changes<'Key,'Value when 'Key : comparison > =
    {
        // added :: Map.Map k v
        added: Map<'Key,'Value>
        // changed :: Map.Map k (v,v)
        changed: Map<'Key,'Value*'Value>
        // removed :: Map.Map k v
        removed: Map<'Key,'Value>
    }

type NamespaceChanges=
    {
        // adtChanges :: Changes String ([String], Map.Map String [Type.Type])
        adtChanges: Changes<string,string list*Map<string,Type list>>
        // aliasChanges :: Changes String ([String], Type.Type)
        //aliasChanges: Changes<string,string list*Type>
        // valueChanges :: Changes String Type.Type
        //valueChanges: Changes<string,Type>
    }

type PackageChanges=
    { 
        // modulesAdded :: [String]
        NamespacesAdded : string list
        // modulesChanged :: Map.Map String ModuleChanges
        NamespacesChanged: Map<string,NamespaceChanges>
        // modulesRemoved :: [String]
        NamespacesRemoved: string list
    }

type Namespace=
    {
        // adts :: Map.Map String ([String], Map.Map String [Type.Type])
        adts: Map<string,string list*Map<string,Type list>>
    }
(*
data Module = Module
    { adts :: Map.Map String ([String], Map.Map String [Type.Type])
    , aliases :: Map.Map String ([String], Type.Type)
    , values :: Map.Map String Type.Type
    , version :: Docs.Version
    }
    *)
module Compare=
    let private keys map = map |> Map.toSeq |> Seq.map fst
    let isEquivalentAdt (ignoreOrigin:bool) 
                        (oldVars:string list, oldCtors: Map<string,Type list>) 
                        (newVars:string list, newCtors: Map<string,Type list>) 
                            =
                                oldCtors.Count = newCtors.Count
                                //&& List.zip (keys oldCtors) (keys newCtors)
                                // && and (zipWith (==) (Map.keys oldCtors) (Map.keys newCtors))
                         
(*

isEquivalentAdt
    :: Bool
    -> ([String], Map.Map String [Type.Type])
    -> ([String], Map.Map String [Type.Type])
    -> Bool
isEquivalentAdt ignoreOrigin (oldVars, oldCtors) (newVars, newCtors) =
    Map.size oldCtors == Map.size newCtors
    && and (zipWith (==) (Map.keys oldCtors) (Map.keys newCtors))
    && and (Map.elems (Map.intersectionWith equiv oldCtors newCtors))
  where
    equiv :: [Type.Type] -> [Type.Type] -> Bool
    equiv oldTypes newTypes =
        let
          allEquivalent =
              zipWith
                (isEquivalentType ignoreOrigin)
                (map ((,) oldVars) oldTypes)
                (map ((,) newVars) newTypes)
        in
          length oldTypes == length newTypes
          && and allEquivalent


isEquivalentType :: Bool -> ([String], Type.Type) -> ([String], Type.Type) -> Bool
isEquivalentType ignoreOrigin (oldVars, oldType) (newVars, newType) =
  case diffType ignoreOrigin oldType newType of
    Nothing ->
        False

    Just renamings ->
        length oldVars == length newVars
        && isEquivalentRenaming (zip oldVars newVars ++ renamings)

*)