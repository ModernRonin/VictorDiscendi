namespace OldMan.LanguageTraining.Tests.DomainLogic




open FsCheck
open FsUnitTyped
open NUnit.Framework
open OldMan.LanguageTraining.Domain
//open Setup

type FsProperty= FsCheck.Xunit.PropertyAttribute

module Id=
    [<FsProperty>]
    let ``unwrapping returns what was passed when wrapping``
        (wrappee: int64)=
        wrappee |> Id.wrap |> Id.unwrap = wrappee

    let ``wrapping returns what was passed when unwrapping``
        (wrappee: Id)=
        wrappee |> Id.unwrap |> Id.wrap = wrappee

    type HasRawMember= {Id: int64}
    module HasRawMember=
        let create (raw) : HasRawMember= {Id= raw}

    [<Test>]
    let ``fromRaw returns the Id property of its argument``()=
        {Id=13L} |> Id.fromRaw |> shouldEqual (Id.wrap 13L)

    type HasTypedMember= {Id: Id}
    [<Test>]
    let ``from returns the Id property of its argument``()=
        {Id= Id.wrap 13L} |> Id.from |> shouldEqual (Id.wrap 13L)

    [<Test>]
    let ``nextAfter returns 1 for an empty sequence``()=
        List.empty<HasRawMember> |> Id.nextAfter |> shouldEqual (Id.wrap 1L)

    [<Test>]
    let ``nextAfter returns 1 + the maximum Id in the sequence, provided the sequence has at least one element``()=
        let rows: HasRawMember list=
                [
                    HasRawMember.create 13L
                    HasRawMember.create 17L
                    HasRawMember.create 11L
                ] 
        rows |> Id.nextAfter |> Id.unwrap |> shouldEqual 18L