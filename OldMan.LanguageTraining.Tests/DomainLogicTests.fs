module OldMan.LanguageTraining.Tests.DomainLogic

open NUnit.Framework
open FsUnitTyped
open OldMan.LanguageTraining.Domain

module doTagsMatch=
    let underTest= OldMan.LanguageTraining.Domain.doTagsMatch

    module ``for a TagIsCondition``=
        let underTest= underTest (TagIsContained "alpha")

        [<Test>]
        let ``and an empty list is false``()=
            [] |> underTest |> shouldEqual false

        [<Test>]
        let ``and a list containing the desired tag is true``()=
            ["charlie"; "alpha"; "bravo"] |> underTest |> shouldEqual true

        [<Test>]
        let ``and a list not containing the desired tag is false``()=
            ["bravo"; "charlie"] |> underTest |> shouldEqual false

    module ``for an AndTagCondition``=
        let underTest= underTest (AndTagCondition 
                                    (
                                        (TagIsContained "a"), 
                                        (TagIsContained "b")
                                     )
                                  )

        [<Test>]
        let ``and a list matching neither sub-condition is false``()=
            ["x"; "y"; "z"] |> underTest |> shouldEqual false

        [<Test>]
        let ``and a list only matching the second sub-condition is false``()=
            ["x"; "b"; "z"] |> underTest |> shouldEqual false

        [<Test>]
        let ``and a list only matching the first sub-condition is false``()=
            ["x"; "a"; "z"] |> underTest |> shouldEqual false

        [<Test>]
        let ``and a list matching both sub-conditions is true``()=
            ["x"; "a"; "z"; "b"] |> underTest |> shouldEqual true

    module ``for an OrTagCondition``=
        let underTest= underTest (OrTagCondition 
                                    (
                                        (TagIsContained "a"), 
                                        (TagIsContained "b")
                                     )
                                  )

        [<Test>]
        let ``and a list matching neither sub-condition is false``()=
            ["x"; "y"; "z"] |> underTest |> shouldEqual false

        [<Test>]
        let ``and a list only matching the first sub-condition is true``()=
            ["x"; "a"; "z"] |> underTest |> shouldEqual true

        [<Test>]
        let ``and a list onyl matching the second sub-condition is true``()=
            ["x"; "b"; "z"] |> underTest |> shouldEqual true

        [<Test>]
        let ``and a list matching both sub-conditions is true``()=
            ["x"; "a"; "z"; "b"] |> underTest |> shouldEqual true