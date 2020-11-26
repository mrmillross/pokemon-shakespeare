# pokemon-shakespeare

An Api for retrieving a shakespearian (Shakespearean?) representation of Pokemon/Pokemen 

Can be run from visual studio (F5 should work), or the command line. (dotnet run should work, provided you're in the context of the `PokemonShakespeareApi` folder.)

There are some unit tests which can be run from Visual Studio if you're lucky enough to have ReSharper, or Jetbrains Rider. Alternatively, dotnet test from the command line should work, from the context of the  `PokemonShakespeareApi.Tests` folder

You can call it with postman, or curl, or any flavour of thing that does http calls, make a request to (for example) https://localhost:5001/api/pokemon/translation/pikachu 

## Notes

I've added a naive caching mechanism, with a configurable expiry. This implementation would suffer from potential concurrency issues as it employs no locking, so that would be an improvement to the system. 

It will also cache the 429 (too many requests) response from the pokemon api, as it is quite restrictive - this is another thing that I would change if I had more time.

Thanks you for reading.
Matt