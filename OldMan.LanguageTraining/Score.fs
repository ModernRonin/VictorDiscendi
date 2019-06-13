namespace OldMan.LanguageTraining.Domain

type Score = Score of int 
module Score=
    let start= Score (-3)
    let unwrap (Score s)= s
    let add lhs rhs=
        (rhs + unwrap lhs) |> Score

