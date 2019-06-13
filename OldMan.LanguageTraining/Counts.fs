namespace OldMan.LanguageTraining.Domain


open System

type Count = Count of uint32
module Count=
    let zero= Count (uint32 0)
    let unwrap (Count c)= c


type ChoiceCount= ChoiceCount of uint8 
module ChoiceCount=
    let create = function
        | n when n > 1 -> n
        | _ -> raise (ArgumentOutOfRangeException())
    let minusOne (ChoiceCount c)= int c-1
