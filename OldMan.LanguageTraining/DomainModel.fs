﻿module OldMan.LanguageTraining.Domain

open System

// main entities
type Language = string

type Word = string option * Language

type Tag = string

type ScoreCard = 
    {
        LastAsked: DateTime
        TimesAsked: uint32
        LeftScore: int
        RightScore: int
    }
type WordPair= 
     {
        Pair: Word*Word
        Created: DateTime
        Tags: Tag list
        ScoreCard: ScoreCard
     }

type LanguageConfiguration= 
    {
        LeftLanguageName: string
        RightLanguageName: string
    }

// quizzes
type Direction=
    | LeftToRight
    | RightToLeft
    | Both

type MultipleChoice=
    {
        NumberOfChoices: uint8
    }

type QuestionType=
    | MultipleChoice of MultipleChoice
    | FreeEntry
    

type QuitSettings=
    {
        Direction: Direction
        Types: QuestionType list
        TagsToInclude: Tag list
        MaximumScore: int
    }


                   