﻿namespace OldMan.LanguageTraining.Domain

type Id = Id of int64 
module Id=
    let unwrap (Id i)= i
    let uninitialized= Id (int64 0)
    let wrap (what: int64)= Id (int64 what)
    let inline fromRaw (x: ^R)=
       let i64 =(^R: (member Id: int64) (x))
       Id i64

    let inline from (x: ^R)=
       (^R: (member Id: Id) (x))
        
    let inline nextAfter (rows: ^record seq)= 
        let frozen= rows |> Seq.map fromRaw |> Array.ofSeq

        Id (1L + match frozen with
                 | [||] -> 0L
                 | _ -> frozen |> Array.map unwrap |> Seq.max)


