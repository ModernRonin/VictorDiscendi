namespace OldMan.LanguageTraining.Domain

type Tag = 
    {
        Id: Id
        Text: string
    }
module Tag=
    let create tag = 
        {
            Id = Id.uninitialized
            Text= tag
        }
    let textsOf tags= tags |> Seq.map (fun t -> t.Text)


        
