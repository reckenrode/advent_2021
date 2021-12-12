// SPDX,License,Identifier: GPL,3.0,only

module Advent2021.Tests.Solutions.Day12

open System.IO
open System.Text

open Expecto

open Advent2021.Solutions.Day12

[<Tests>]
let tests = testList "Day12.Graph" [
    test "it finds the trivial path" {
        let expectedPaths = Set.ofList ["start,end"]
        let input = Graph.ofList [
            "start", Set.ofList ["end"]
            "end", Set.ofList ["start"]
        ]
        let paths = Graph.paths input
        Expect.equal paths expectedPaths "the paths match"
    }

    test "it ends on end" {
        let expectedPaths = Set.ofList ["start,end"]
        let input = Graph.ofList [
            "start", Set.ofList ["A"; "end"]
            "A", Set.ofList ["start"]
            "end", Set.ofList ["A"]
        ]
        let paths = Graph.paths input
        Expect.equal paths expectedPaths "the paths match"
    }

    testList "normal nodes" [
        test "it finds the path with one node" {
            let expectedPaths = Set.ofList ["start,A,end"]
            let input = Graph.ofList [
                "start", Set.ofList ["A"]
                "A", Set.ofList ["end"; "start"]
                "end", Set.ofList ["A"]
            ]
            let paths = Graph.paths input
            Expect.equal paths expectedPaths "the paths match"
        }

        test "it finds the paths each with one node" {
            let expectedPaths = Set.ofList [
                "start,A,end"
                "start,B,end"
            ]
            let input = Graph.ofList [
                "start", Set.ofList ["A"; "B"]
                "A", Set.ofList ["end"; "start"]
                "B", Set.ofList ["end"; "start"]
                "end", Set.ofList ["A"; "B"]
            ]
            let paths = Graph.paths input
            Expect.equal paths expectedPaths "the paths match"
        }

        test "it finds the path with multiple nodes" {
            let expectedPaths = Set.ofList [
                "start,A,b,end"
            ]
            let input = Graph.ofList [
                "start", Set.ofList ["A"]
                "A", Set.ofList ["b"; "start"]
                "b", Set.ofList ["b"; "end"]
                "end", Set.ofList ["b"]
            ]
            let paths = Graph.paths input
            Expect.equal paths expectedPaths "the paths match"
        }

        test "it finds the paths with multiple nodes" {
            let expectedPaths = Set.ofList [
                "start,b,C,end"
                "start,A,b,C,end"
                "start,C,b,C,end"
                "start,C,end"
            ]
            let input = Graph.ofList [
                "start", Set.ofList ["A"; "b"; "C"]
                "A", Set.ofList ["b"]
                "b", Set.ofList ["A"; "C"; "start"]
                "C", Set.ofList ["b"; "start"; "end"]
                "end", Set.ofList ["C"]
            ]
            let paths = Graph.paths input
            Expect.equal paths expectedPaths "the paths match"
        }
    ]

    testList "small nodes" [
        test "it finds the path with one small node" {
            let expectedPaths = Set.ofList ["start,a,end"]
            let input = Graph.ofList [
                "start", Set.ofList ["a"]
                "a", Set.ofList ["end"; "start"]
                "end", Set.ofList ["a"]
            ]
            let paths = Graph.paths input
            Expect.equal paths expectedPaths "the paths match"
        }

        test "it finds the paths each with one small node" {
            let expectedPaths = Set.ofList [
                "start,a,end"
                "start,b,end"
            ]
            let input = Graph.ofList [
                "start", Set.ofList ["a"; "b"]
                "a", Set.ofList ["end"; "start"]
                "b", Set.ofList ["end"; "start"]
                "end", Set.ofList ["a"; "b"]
            ]
            let paths = Graph.paths input
            Expect.equal paths expectedPaths "the paths match"
        }

        test "it finds the path with multiple small nodes" {
            let expectedPaths = Set.ofList [
                "start,a,b,end"
            ]
            let input = Graph.ofList [
                "start", Set.ofList ["a"]
                "a", Set.ofList ["b"; "start"]
                "b", Set.ofList ["b"; "end"]
                "end", Set.ofList ["b"]
            ]
            let paths = Graph.paths input
            Expect.equal paths expectedPaths "the paths match"
        }

        test "it finds the paths with multiple small nodes" {
            let expectedPaths = Set.ofList [
                "start,a,c,end"
                "start,a,b,c,end"
                "start,b,c,end"
                "start,b,a,c,end"
            ]
            let input = Graph.ofList [
                "start", Set.ofList ["a"; "b"]
                "a", Set.ofList ["b"; "c"; "start"]
                "b", Set.ofList ["a"; "c"; "start"]
                "c", Set.ofList ["a"; "b"; "end"]
                "end", Set.ofList ["c"]
            ]
            let paths = Graph.paths input
            Expect.equal paths expectedPaths "the paths match"
        }

        test "it visits small nodes at most once" {
            let expectedPaths = Set.ofList [
                "start,a,c,end"
                "start,a,b,c,end"
                "start,b,c,end"
                "start,b,a,c,end"
            ]
            let input = Graph.ofList [
                "start", Set.ofList ["a"; "b"]
                "a", Set.ofList ["b"; "c"; "start"]
                "b", Set.ofList ["a"; "c"; "start"]
                "c", Set.ofList ["a"; "b"; "end"]
                "end", Set.ofList ["c"]
            ]
            let paths = Graph.paths input
            Expect.equal paths expectedPaths "the paths match"
        }

        test "it disregards dead-ends" {
            let expectedPaths = Set.ofList [
                "start,a,b,end"
            ]
            let input = Graph.ofList [
                "start", Set.ofList ["a"]
                "a", Set.ofList ["b"; "start"]
                "b", Set.ofList ["a"; "c"; "end"]
                "c", Set.ofList ["b"]
                "end", Set.ofList ["b"]
            ]
            let paths = Graph.paths input
            Expect.equal paths expectedPaths "the paths match"
        }
    ]

    testList "parsing" [
        test "it parses the graph" {
            let expectedGraph = Ok (Graph.ofList [
                "start", Set.ofList ["a"; "b"]
                "a", Set.ofList ["start"; "b"; "c"]
                "b", Set.ofList ["a"; "c"; "start"]
                "c", Set.ofList ["end"; "a"; "b"]
                "end", Set.ofList ["c"]
            ])
            use input =
                Encoding.UTF8.GetBytes "start-b\na-c\nb-c\nb-a\na-b\nc-end\nstart-a\n"
                |> (fun f -> new MemoryStream (f))
            let graph = Graph.parse "test" input
            Expect.equal graph expectedGraph "the graph parsed"
        }
    ]
]
