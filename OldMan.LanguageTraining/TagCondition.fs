namespace OldMan.LanguageTraining.Domain


type TagCondition=
    | AndTagCondition of TagCondition*TagCondition
    | OrTagCondition of TagCondition*TagCondition
    | TagIsContained of Tag 
module TagCondition=
    let from (tag: string)= tag |> Tag.create |> TagIsContained
    let rec isFulfilledBy condition tags = 
        match condition with
        | TagIsContained t -> tags |> List.contains t
        | AndTagCondition (left, right) -> (isFulfilledBy left tags) && (isFulfilledBy right tags)
        | OrTagCondition (left, right) -> (isFulfilledBy left tags) || (isFulfilledBy right tags)
    let filter condition pairs= pairs |> List.filter (fun p -> isFulfilledBy condition p.Tags)


