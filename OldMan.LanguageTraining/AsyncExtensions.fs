namespace OldMan

module Async=
    let from result=
        async {
            return result
        }
