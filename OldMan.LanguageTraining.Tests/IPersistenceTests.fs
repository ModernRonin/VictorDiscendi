(*
    These tests should be true for any type implementing IPersistence.
    If at some point we get another implementation, we will need to figure 
    out a way to parameterize this test suite to the implementation type.
*)

namespace OldMan.LanguageTraining.Tests.Persistence
module Setup=
    open System
    open OldMan.LanguageTraining.Persistence
    open OldMan.LanguageTraining.Domain

    open FsCheck

    type BackStore()= 
        let mutable data: Map<DataKind, string>= Map.empty.Add(DataKind.Configuration,"").Add(DataKind.Tagging, "").Add(DataKind.Words, "").Add(DataKind.WordTagAssociation, "")
        member this.Load kind= data |> Map.find kind
        member this.Save kind what= data <- data.Add (kind, what)

    let createWithEmptyBackStore()= 
        let backStore= new BackStore()
        (new CsvPersistence(backStore.Load, backStore.Save)) :> OldMan.LanguageTraining.IPersistence

    let stringGenerator= 
        Arb.Default.Char().Generator |> 
        Gen.filter (fun c -> Char.IsLetterOrDigit(c) || Char.IsPunctuation(c)) |> 
        Gen.nonEmptyListOf |> Gen.map Array.ofList |> Gen.map (fun c -> new string(c))

    type Generators=
        static member String()= 
            {
                new Arbitrary<string>() with 
                    override x.Generator= stringGenerator
            }
        static member Id()=
            {
                new Arbitrary<Id>() with 
                    override x.Generator= Gen.constant Id.uninitialized
            }


open FsCheck.Xunit
open FsUnitTyped
open NUnit.Framework
open OldMan.LanguageTraining.Domain
open Setup

type FsProperty= FsCheck.Xunit.PropertyAttribute

[<Properties(Arbitrary= [| typeof<Generators> |])>]
module IPersistence=
    let hasOnlyUniqueValues l = l |> Seq.groupBy id |> Seq.forall (fun (_, l) -> (Array.ofSeq l).Length=1)

    [<FsProperty>]
    let ``getting the configuration after updating it returns what it was updated to`` 
        (config: LanguageConfiguration)=
        let persistence= createWithEmptyBackStore()
        persistence.UpdateConfiguration(config) 
        persistence.GetConfiguration() = config

    [<Test>]
    let ``getting the configuration from an empty backstore returns two empty language names``()=
        let persistence= createWithEmptyBackStore()
        persistence.GetConfiguration() |> shouldEqual {LeftLanguageName=""; RightLanguageName=""}

    [<FsProperty>]
    let ``getting pairs returns the combined return values of all added pairs``
        (pairs: WordPair list)=
        let persistence= createWithEmptyBackStore()
        let expected= pairs |> List.map persistence.AddPair
        persistence.GetPairs() = expected

    [<FsProperty>]
    let ``adding a pair returns the pair with Id set``
        (pair: WordPair)=
        let persistence= createWithEmptyBackStore()
        let result= persistence.AddPair pair
        result.Id <> pair.Id

    [<FsProperty>]
    let ``all added pairs have different Ids``
        (pairs: WordPair list)=
        let persistence= createWithEmptyBackStore()
        let ids= pairs |> List.map persistence.AddPair |> List.map (fun p -> p.Id)
        ids |> hasOnlyUniqueValues

    [<Test>]
    let ``adding the pairs X and Y followed by updating X to Z followed by gettting all pairs returns Z and Y``()=
        let persistence= createWithEmptyBackStore()
        let x= WordPair.create (Word "xl", Word "xr", [])
        let y= WordPair.create (Word "yl", Word "yr", [])
        let addedX= persistence.AddPair x
        let addedY= persistence.AddPair y
        let z= {addedX with Left=Word "alpha"; Right=Word "bravo"}
        persistence.UpdatePair z
        let allPairs= persistence.GetPairs() 
        allPairs |> shouldHaveLength 2
        allPairs |> shouldContain addedY
        allPairs |> shouldContain z

    [<Test>]
    let ``from an empty backstore, GetTags() returns an empty list``()=
        let persistence= createWithEmptyBackStore()
        persistence.GetTags() |> shouldBeEmpty

    [<Test>]
    let ``adding a pair implicitly adds its tags``()=
        let persistence= createWithEmptyBackStore()
        (Word "xl", Word "xr", [Tag.create "alpha"; Tag.create "bravo"]) |> WordPair.create |> persistence.AddPair |> ignore
        let tags= persistence.GetTags()
        tags |> shouldHaveLength 2
        tags |> Tag.textsOf |> List.ofSeq |> shouldEqual ["alpha"; "bravo"]
        tags |> List.map (fun t -> (Id.from t)) |> hasOnlyUniqueValues |> shouldEqual true

    [<Test>]
    let ``adding the same tag to two pairs does not create multiple copies``()=
        let persistence= createWithEmptyBackStore()
        (Word "xl", Word "xr", [Tag.create "alpha"]) |> WordPair.create |> persistence.AddPair |> ignore
        let alpha= persistence.GetTags() |> List.head
        (Word "yl", Word "yr", [alpha]) |> WordPair.create |> persistence.AddPair |> ignore
        match persistence.GetTags() with
        |[] -> Assert.Fail() 
        |head::[] -> head |> shouldEqual {Id=Id.wrap 1L; Text="alpha"}
        |_ -> Assert.Fail()

    [<Test>]
    let ``removing a tag from a wordpair does not remove it from others``()=
        let persistence= createWithEmptyBackStore()
        let x= (Word "xl", Word "xr", [Tag.create "alpha"]) |> WordPair.create |> persistence.AddPair
        let alpha= persistence.GetTags() |> List.head
        (Word "yl", Word "yr", [alpha]) |> WordPair.create |> persistence.AddPair |> ignore
        persistence.UpdatePair {x with Tags=[]}
        let other= persistence.GetPairs() |> List.filter (fun p -> p.Left=Word "yl") |> List.head
        other.Tags |> shouldContain alpha
        persistence.GetTags() |> shouldHaveLength 1
        
    [<Test>]
    let ``removing a tag from the last word-pair removes it from the general list``()=
        let persistence= createWithEmptyBackStore()
        let x= (Word "xl", Word "xr", [Tag.create "alpha"]) |> WordPair.create |> persistence.AddPair
        let alpha= persistence.GetTags() |> List.head
        let y= (Word "yl", Word "yr", [alpha]) |> WordPair.create |> persistence.AddPair 
        persistence.UpdatePair {x with Tags=[]}
        persistence.UpdatePair {y with Tags=[]}
        persistence.GetTags() |> shouldBeEmpty
        
    [<Test>]
    let ``getTags returns all distinct tags in all added pairs``()=
        let persistence= createWithEmptyBackStore()
        let add= WordPair.create >> persistence.AddPair >> ignore
        let getTag text= persistence.GetTags() |> List.filter (fun t -> t.Text=text) |> List.head
        (Word "a", Word "b", [Tag.create "alpha"; Tag.create "bravo"]) |> add
        let bravo= getTag "bravo"
        (Word "c", Word "d", [bravo; Tag.create "charlie"]) |> add
        let charlie= getTag "charlie"
        (Word "e", Word "f", [charlie; Tag.create "delta"]) |> add
        let tags= persistence.GetTags()
        tags |> shouldHaveLength 4
        let texts= tags |> List.map (fun t -> t.Text)
        texts |> shouldContain "alpha"
        texts |> shouldContain "bravo"
        texts |> shouldContain "charlie"
        texts |> shouldContain "delta"
        tags |> List.map (fun t -> t.Id) |> hasOnlyUniqueValues |> shouldEqual true
        
    [<Test>]
    let ``AddOrUpdateTag() adds it if it doe not yet exist``()=
        let persistence= createWithEmptyBackStore()
        persistence.AddOrUpdateTag (Tag.create "alpha")
        persistence.GetTags() |> shouldContain {Id=Id.wrap 1L; Text="alpha"}
      
    [<Test>]
    let ``AddOrUpdateTag() updates if the tag already exists``()=
        let persistence= createWithEmptyBackStore()
        persistence.AddOrUpdateTag (Tag.create "alpha")
        persistence.AddOrUpdateTag {Id= Id.wrap 1L; Text="bravo"}
        persistence.GetTags() |> shouldContain {Id= Id.wrap 1L; Text="bravo"}

