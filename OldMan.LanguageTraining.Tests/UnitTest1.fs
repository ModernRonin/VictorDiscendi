module OldMan.LanguageTraining.Tests.Persistence

open NUnit.Framework
open FsUnitTyped

[<Test>]
let firstTest () =
    1 + 1 |> shouldEqual 2
        