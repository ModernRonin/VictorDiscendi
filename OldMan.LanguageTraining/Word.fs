namespace OldMan.LanguageTraining.Domain

type Word = Word of string 

module Word=
    let unwrap (Word text)= text


