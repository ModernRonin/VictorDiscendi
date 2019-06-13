namespace OldMan.LanguageTraining.Tests.DomainLogic.TagCondition

open NUnit.Framework
open FsUnitTyped
open OldMan.LanguageTraining.Domain

module isFulfilledBy=
    let underTest= OldMan.LanguageTraining.Domain.TagCondition.isFulfilledBy
    let toTags= List.map Tag.create

    module ``for a TagIsCondition``=
        let underTest= underTest (TagCondition.from "alpha")

        [<Test>]
        let ``and an empty list is false``()=
            [] |> underTest |> shouldEqual false

        [<Test>]
        let ``and a list containing the desired tag is true``()=
            ["charlie"; "alpha"; "bravo"] |> toTags |> underTest |> shouldEqual true

        [<Test>]
        let ``and a list not containing the desired tag is false``()=
            ["bravo"; "charlie"] |> toTags |> underTest |> shouldEqual false

    module ``for an AndTagCondition``=
        let underTest= underTest (AndTagCondition 
                                    (
                                        (TagCondition.from "a"), 
                                        (TagCondition.from "b")
                                    )
                                 )

        [<Test>]
        let ``and a list matching neither sub-condition is false``()=
            ["x"; "y"; "z"] |> toTags |> underTest |> shouldEqual false

        [<Test>]
        let ``and a list only matching the second sub-condition is false``()=
            ["x"; "b"; "z"] |> toTags |> underTest |> shouldEqual false

        [<Test>]
        let ``and a list only matching the first sub-condition is false``()=
            ["x"; "a"; "z"] |> toTags |> underTest |> shouldEqual false

        [<Test>]
        let ``and a list matching both sub-conditions is true``()=
            ["x"; "a"; "z"; "b"] |> toTags |> underTest |> shouldEqual true

    module ``for an OrTagCondition``=
        let underTest= underTest (OrTagCondition 
                                    (
                                        (TagCondition.from "a"), 
                                        (TagCondition.from "b")
                                     )
                                  )

        [<Test>]
        let ``and a list matching neither sub-condition is false``()=
            ["x"; "y"; "z"] |> toTags |> underTest |> shouldEqual false

        [<Test>]
        let ``and a list only matching the first sub-condition is true``()=
            ["x"; "a"; "z"] |> toTags |> underTest |> shouldEqual true

        [<Test>]
        let ``and a list onyl matching the second sub-condition is true``()=
            ["x"; "b"; "z"] |> toTags |> underTest |> shouldEqual true

        [<Test>]
        let ``and a list matching both sub-conditions is true``()=
            ["x"; "a"; "z"; "b"] |> toTags |> underTest |> shouldEqual true

module filter=
    [<Test>]
    let ``return those pairs whose tags fulfill the condition``()=
        let makePair (left, right, tag) =
            WordPair.create (Word left, Word right, Tag.create tag |> List.singleton)
        let pairs= 
            [
                ("l1", "r1", "a")
                ("l2", "r2", "b")
                ("l3", "r3", "a")
                ("l4", "r4", "c")
            ]  |> List.map makePair
        let condition= TagIsContained (Tag.create "a")

        pairs |> TagCondition.filter condition |> List.map (fun p -> Word.unwrap p.Left) |> shouldEqual ["l1"; "l3"]
