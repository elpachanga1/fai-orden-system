// InMemoryRequestRepository uses a non-thread-safe Dictionary as a singleton.
// Disable parallelism within this assembly to prevent race conditions.
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
