﻿namespace FsCheck

open Gen
open System
open System.Collections.Generic
open System.Runtime.CompilerServices

///Extension methods to build generators - contains among other the Linq methods.
[<AbstractClass; Sealed; Extension>]
type GenExtensions = 

    ///Generates a value with maximum size n.
    //[category: Generating test values]
    [<Extension>]
    static member Eval(generator, size, random) =
        eval size random generator

    ///Generates n values of the given size.
    //[category: Generating test values]
    [<Extension>]
    static member Sample(generator, size, numberOfSamples) =
        sample size numberOfSamples generator

    /// Allows type annotations in LINQ expressions
    [<Extension>]
    static member Cast(g:Gen<_>) = g

    ///Map the given function to the value in the generator, yielding a new generator of the result type.
    [<Extension>]
    static member Select(g:Gen<_>, selector : Func<_,_>) = 
        if selector = null then nullArg "selector"
        g.Map(fun a -> selector.Invoke(a))

    ///Generates a value that satisfies a predicate. This function keeps re-trying
    ///by increasing the size of the original generator ad infinitum.  Make sure there is a high chance that 
    ///the predicate is satisfied.
    [<Extension>]
    static member Where(g:Gen<_>, predicate : Func<_,_>) = 
        if predicate = null then nullArg "predicate"
        where (fun a -> predicate.Invoke(a)) g
    
    [<Extension>]
    static member SelectMany(source:Gen<_>, f:Func<_, Gen<_>>) =
        if f = null then nullArg "f" 
        gen { let! a = source
              return! f.Invoke(a) }
    
    [<Extension>]
    static member SelectMany(source:Gen<_>, f:Func<_, Gen<_>>, select:Func<_,_,_>) =
        if f = null then nullArg "f"
        if select = null then nullArg "select"
        gen { let! a = source
              let! b = f.Invoke(a)
              return select.Invoke(a,b) }

    ///Generates a list of given length, containing values generated by the given generator.
    //[category: Creating generators from generators]
    [<Extension>]
    static member ListOf (generator, nbOfElements) =
        listOfLength nbOfElements generator
        |> map (fun l -> new List<_>(l) :> IList<_>)

    /// Generates a list of random length. The maximum length depends on the
    /// size parameter.
    //[category: Creating generators from generators]
    [<Extension>]
    static member ListOf (generator) =
        listOf generator
        |> map (fun l -> new List<_>(l) :> IList<_>)

    /// Generates a non-empty list of random length. The maximum length 
    /// depends on the size parameter.
    //[category: Creating generators from generators]
    [<Extension>]
    static member NonEmptyListOf<'a> (generator) = 
        nonEmptyListOf generator 
        |> map (fun list -> new List<'a>(list) :> IList<_>)
    
    /// Generates an array of a specified length.
    //[category: Creating generators from generators]
    [<Extension>]
    static member ArrayOf (generator, length) =
        arrayOfLength length generator

    /// Generates an array using the specified generator. 
    /// The maximum length is size+1.
    //[category: Creating generators from generators]
    [<Extension>]
    static member ArrayOf (generator) =
        arrayOf generator

    /// Generates a 2D array of the given dimensions.
    //[category: Creating generators from generators]
    [<Extension>]
    static member Array2DOf (generator, rows, cols) =
        array2DOfDim (rows,cols) generator

    /// Generates a 2D array. The square root of the size is the maximum number of rows and columns.
    //[category: Creating generators from generators]
    [<Extension>]
    static member Array2DOf (generator) =
        array2DOf generator

    ///Apply the given Gen function to this generator, aka the applicative <*> operator.
    //[category: Creating generators from generators]
    [<Extension>]
    static member Apply (generator, f:Gen<Func<_,_>>) =
        apply (f |> map (fun f -> f.Invoke)) generator

    ///Override the current size of the test.
    //[category: Managing size]
    [<Extension>]
    static member Resize (generator, newSize) =
        resize newSize generator

    /// Construct an Arbitrary instance from a generator.
    /// Shrink is not supported for this type.
    [<Extension>]
    static member ToArbitrary generator =
        Arb.fromGen generator

    /// Construct an Arbitrary instance from a generator and a shrinker.
    [<Extension>]
    static member ToArbitrary (generator,shrinker) =
        Arb.fromGenShrink (generator,shrinker)

    ///Build a generator that generates a 2-tuple of the values generated by the given generator.
    //[category: Creating generators from generators]
    [<Extension>]
    static member Two (generator) =
        two generator

    ///Build a generator that generates a 3-tuple of the values generated by the given generator.
    //[category: Creating generators from generators]
    [<Extension>]
    static member Three (generator) =
        three generator

    ///Build a generator that generates a 4-tuple of the values generated by the given generator.
    //[category: Creating generators from generators]
    [<Extension>]
    static member Four (generator) =
        four generator

    ///Combine two generators into a generator of pairs.
    //[category: Creating generators from generators]
    [<Extension>]
    static member Zip (generator, other)  =
        zip generator other

    ///Combine two generators into a new generator of the result of the given result selector.
    //[category: Creating generators from generators]
    [<Extension>]
    static member Zip (generator, other, resultSelector : Func<_, _, _>) =
        if resultSelector = null then nullArg "resultSelector"
        zip generator other |> map resultSelector.Invoke

    ///Combine three generators into a generator of 3-tuples.
    //[category: Creating generators from generators]
    [<Extension>]
    static member Zip (generator, second, third) =
        zip3 generator second third

    ///Combine three generators into a new generator of the result of the given result selector.
    //[category: Creating generators from generators]
    [<Extension>]
    static member Zip (generator, second, third, resultSelector : Func<_, _, _, _>) =
        if resultSelector = null then nullArg "resultSelector"
        zip3 generator second third |> map resultSelector.Invoke

    ///Build a generator that generates a value from two generators with equal probability.
    //[category: Creating generators from generators]
    [<Extension>]
    static member Or (generator, other) =
        oneof [ generator; other ]

    ///Build a generator that generates a value or `null`
    //[category: Creating generators from generators]
    [<Extension>]
    static member OrNull (generator) =
        frequency [ (7, generator); (1, constant null) ]