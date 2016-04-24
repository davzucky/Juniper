﻿// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

module Progam
open Microsoft.FSharp.Text.Lexing


[<EntryPoint>]
let main argv =
    // List of includes of custom Juniper std library modules
    let stdLibrary = ["Prelude"; "List"; "Signal"; "Io"; "Maybe"; "Time"; "Math"; "Button"; "Vector"]
    let executingDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    // Make the include modules names by prepending the executing directory and /junstd/, and appending the .jun file extension
    let stdFiles = stdLibrary |> List.map (fun name -> "junstd/" + name + ".jun")
    // Lexer and parser operations on the file
    let parseFromFile (fileName:string) =
        let fileName' = (if System.IO.File.Exists(fileName) then
                            fileName
                        else
                            executingDir + "/" + fileName) |> System.IO.Path.GetFullPath
        let fileStr = System.IO.File.ReadAllText fileName'
        let lexbuf = LexBuffer<char>.FromString fileStr
        lexbuf.EndPos <- { pos_bol = 0;
                           pos_fname=fileName';
                           pos_cnum=0;
                           pos_lnum=0 }
        try
            Parser.start Lexer.token lexbuf
        with
          | _ -> printfn "Syntax error in %s on line %d, column %d" fileName (lexbuf.StartPos.Line + 1) (lexbuf.StartPos.Column + 1);
                 failwith "Syntax error"
    // All of the file names includes all the specified ones, plus the std Juniper library
    let fnames = List.append stdFiles (List.ofArray argv)
    // Run parseFromFile (the lexer and parser)
    let asts = List.map parseFromFile fnames
    try
        // Typecheck the ASTs
        let typedAsts = TypeChecker.typecheckProgram asts fnames
        // Compile to C++ the typechecked (and typed) ASTs
        let compiledProgram = Compiler.compileProgram typedAsts
        printfn "%s" compiledProgram
        0
    with
        Failure(msg) ->
            1