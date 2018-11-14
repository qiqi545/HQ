# Squire
### Allows you to treat source-containing PackageReferences like normal code.

The new `<PackageReference>` system, coupled with tight integration between NuGet and MSBuild, is a great thing.

However, by design, source packages that are consumed as `PackageReferences` have some strange properties:

- They are _invisible_ to their consumers. You can't browse the code normally unless you know a class or method name in advance, or you run into a bug.
- They are _read-only_. Any changes saved to the resulting files is immediately replaced on each build, during the `CollectionPackageReferences` target.
- They _can't be versioned_. Other than the package version itself, you cannot see how the code changed in your own source system.

To be clear, these are features: transformed, transparent, source code, distributed through the package system.

However, we can make things better, especially for library developer and their customers.

#### For Library Developers:

- Transparently generate pre-processor (`pp.cs`) files of all `.cs` files in your project that change the `$RootNamespace` of your 
source files to match the consuming project. This means you can write regular libraries, and when you want to distribute them as source,
you simply add a `PackageSources` Configuration and build. 

#### For Library Consumers:

- Consume source packages that allow you to make changes against real, browsable files. If you edit a file, your file wins. If a new package
update brings fresh files, those files win. This enables tight feedback between library consumer and developer, since consumers can make
changes to address issues and ship their product, and developers can make library fixes that are redistributed back to consumers naturally.
Everything is done using real files in the project system, so everything is trackable in source.
