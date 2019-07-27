module OldMan.LanguageTraining.Clients.Api

open System
open OldMan
open OldMan.LanguageTraining
open OldMan.LanguageTraining.Domain

open WebSharper.UI
open WebSharper.UI.Storage

type private LocalStoragePersistenceStore()=
    let create name keyExtractor=
        let serializer= (Serializer.Default<'T>)
        let storage= (LocalStorage name serializer)
        ListModel.CreateWithStorage keyExtractor storage

    let configList= create "config-store" (fun (_: SerializableConfiguration) -> "default")
    let tagsList= create "tags-store" (fun (t: SerializableTag) -> t.Id)
    let pairsList= create "pairs-store" (fun (p: SerializablePair) -> p.Id)
    let associationsList= create "associations-store" (fun (a: SerializableTagPairAssociation) -> a)

    interface IPersistenceStore with
        member this.loadConfig()=
            async {
                return match configList |> List.ofSeq with 
                        | [] -> {LeftName=""; RightName=""}
                        | head::_ -> head
            }
        member this.saveConfig(config)= 
            async {
                configList.Set [config]
            }

        member this.loadTags()= tagsList |> List.ofSeq |> Async.from
        member this.saveTags(tags)= tagsList.Set tags |> Async.from
        member this.loadPairs()= pairsList |> List.ofSeq |> Async.from
        member this.savePairs(pairs)= pairsList.Set pairs |> Async.from
        member this.loadAssociations()= associationsList |> List.ofSeq |> Async.from
        member this.saveAssociations(associations)= associationsList.Set associations |> Async.from
            

let private store= new LocalStoragePersistenceStore() :> IPersistenceStore
let mutable private _service= (new Service(store)) :> IService

let private doSeed()=
    async {
        do! {LeftName="English"; RightName="Deutsch"} |> store.saveConfig
        let mutable nextId= 1L
        let makeId()=
            let result= nextId
            nextId <- result + 1L
            result
        let makeTag text: SerializableTag= 
            {
                Id= makeId()
                Text= text
            }
        do! ([
                "noun"
                "fruit"
                "verb"
                "cooking"
                "adjective"
                "color"
             ] |> List.map makeTag |> store.saveTags)
        let makePair (english, german)=
            {
                Id= makeId()
                Left= english
                Right= german
                Created= "2019-03-07-07-30-00"
                LastAsked= "2019-03-07-07-30-00"
                TimesAsked= 0L
                LeftScore= -2
                RightScore= -2
            }
        let special= 
            {
                Id= makeId()
                Left= "old"
                Right= "alt"
                Created= "1974-07-23-00-02-00"
                LastAsked= "2019-03-07-07-30-00"
                TimesAsked= 13L
                LeftScore= 7
                RightScore= 5
            }
        let others=
            [
                ("apple", "Apfel")
                ("orange", "Orange")
                ("peel", "schälen")
                ("red", "rot")
            ] |> List.map makePair
        do! special::others |> store.savePairs
        let makeAssociations (english, tags)=
            let makeOne tagText= 
                async {
                    let! pairs= store.loadPairs() 
                    let! tags= store.loadTags()
                    return {
                                PairId= pairs
                                        |> List.filter (fun p -> p.Left=english) 
                                        |> List.map (fun p -> p.Id) 
                                        |> List.head
                                TagId= tags
                                        |> List.filter (fun t -> t.Text=tagText)
                                        |> List.map (fun t -> t.Id)
                                        |> List.head
                           }
                }
            tags |> List.map makeOne

        let! associations= ([
                                ("apple", ["noun"; "fruit"])
                                ("orange", ["noun"; "fruit"])
                                ("peel", ["verb"; "cooking"])
                                ("red", ["adjective"; "color"])
                                ("old", ["adjective"])
                            ] |> List.collect makeAssociations |> Async.Parallel) 
        do! associations |> List.ofArray |> store.saveAssociations
    }
let seed()=
    async {
        let! config= store.loadConfig()
        match  config with 
        | {LeftName=""; RightName=""} -> do! doSeed()
        | _ -> ()
    }
    
let service()= _service